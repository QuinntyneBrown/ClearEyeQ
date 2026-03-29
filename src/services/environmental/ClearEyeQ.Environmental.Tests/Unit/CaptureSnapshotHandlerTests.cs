using Xunit;
using ClearEyeQ.Environmental.Application.Commands.CaptureSnapshot;
using ClearEyeQ.Environmental.Application.Interfaces;
using ClearEyeQ.Environmental.Domain.Aggregates;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace ClearEyeQ.Environmental.Tests.Unit;

public sealed class CaptureSnapshotHandlerTests
{
    private readonly IEnvironmentalSnapshotRepository _repository = Substitute.For<IEnvironmentalSnapshotRepository>();
    private readonly IAirQualityClient _airQualityClient = Substitute.For<IAirQualityClient>();
    private readonly IPollenClient _pollenClient = Substitute.For<IPollenClient>();
    private readonly IWeatherClient _weatherClient = Substitute.For<IWeatherClient>();
    private readonly ILogger<CaptureSnapshotHandler> _logger = Substitute.For<ILogger<CaptureSnapshotHandler>>();
    private readonly CaptureSnapshotHandler _handler;

    public CaptureSnapshotHandlerTests()
    {
        _handler = new CaptureSnapshotHandler(
            _repository, _airQualityClient, _pollenClient, _weatherClient, _logger);
    }

    [Fact]
    public async Task Handle_WithAllDataAvailable_ShouldCreateFullSnapshot()
    {
        // Arrange
        var command = new CaptureSnapshotCommand(
            UserId: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            Latitude: 40.7128,
            Longitude: -74.0060);

        _airQualityClient.GetAirQualityAsync(command.Latitude, command.Longitude, Arg.Any<CancellationToken>())
            .Returns(new AirQualityData(Aqi: 52, Pm25: 15.2, Pm10: 30.5));

        _pollenClient.GetPollenCountAsync(command.Latitude, command.Longitude, Arg.Any<CancellationToken>())
            .Returns(new PollenData(TreeCount: 45, GrassCount: 20, WeedCount: 10));

        _weatherClient.GetWeatherDataAsync(command.Latitude, command.Longitude, Arg.Any<CancellationToken>())
            .Returns(new WeatherData(UvIndex: 6.5, HumidityPercentage: 55.0));

        // Act
        var snapshotId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        snapshotId.Should().NotBeEmpty();
        await _repository.Received(1).AddAsync(
            Arg.Is<EnvironmentalSnapshot>(s =>
                s.AirQualityReading != null &&
                s.PollenCount != null &&
                s.UvIndex != null &&
                s.HumidityReading != null &&
                s.GeoLocation != null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAirQualityFails_ShouldStillCreateSnapshot()
    {
        // Arrange
        var command = new CaptureSnapshotCommand(
            UserId: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            Latitude: 40.7128,
            Longitude: -74.0060);

        _airQualityClient.GetAirQualityAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("API unavailable"));

        _pollenClient.GetPollenCountAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<CancellationToken>())
            .Returns(new PollenData(TreeCount: 30, GrassCount: 15, WeedCount: 5));

        _weatherClient.GetWeatherDataAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<CancellationToken>())
            .Returns(new WeatherData(UvIndex: 3.0, HumidityPercentage: 60.0));

        // Act
        var snapshotId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        snapshotId.Should().NotBeEmpty();
        await _repository.Received(1).AddAsync(
            Arg.Is<EnvironmentalSnapshot>(s =>
                s.AirQualityReading == null &&
                s.PollenCount != null &&
                s.UvIndex != null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAllExternalApisFail_ShouldStillCreateMinimalSnapshot()
    {
        // Arrange
        var command = new CaptureSnapshotCommand(
            UserId: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            Latitude: 51.5074,
            Longitude: -0.1278);

        _airQualityClient.GetAirQualityAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Timeout"));

        _pollenClient.GetPollenCountAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Rate limited"));

        _weatherClient.GetWeatherDataAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        // Act
        var snapshotId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        snapshotId.Should().NotBeEmpty();
        await _repository.Received(1).AddAsync(
            Arg.Is<EnvironmentalSnapshot>(s =>
                s.AirQualityReading == null &&
                s.PollenCount == null &&
                s.UvIndex == null &&
                s.HumidityReading == null &&
                s.GeoLocation != null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldSetCorrectGeoLocation()
    {
        // Arrange
        var command = new CaptureSnapshotCommand(
            UserId: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            Latitude: 34.0522,
            Longitude: -118.2437);

        _airQualityClient.GetAirQualityAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<CancellationToken>())
            .Returns((AirQualityData?)null);
        _pollenClient.GetPollenCountAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<CancellationToken>())
            .Returns((PollenData?)null);
        _weatherClient.GetWeatherDataAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<CancellationToken>())
            .Returns((WeatherData?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(
            Arg.Is<EnvironmentalSnapshot>(s =>
                s.GeoLocation != null &&
                s.GeoLocation.Latitude == 34.0522 &&
                s.GeoLocation.Longitude == -118.2437),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidLatitude_ShouldThrow()
    {
        // Arrange
        var command = new CaptureSnapshotCommand(
            UserId: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            Latitude: 100.0,
            Longitude: -74.0060);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }
}
