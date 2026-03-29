using System.Text.Json;
using ClearEyeQ.Predictive.Application.Commands.GenerateForecast;
using ClearEyeQ.Predictive.Application.Interfaces;
using ClearEyeQ.Predictive.Domain.Aggregates;
using ClearEyeQ.Predictive.Domain.Entities;
using ClearEyeQ.Predictive.Domain.Enums;
using ClearEyeQ.Predictive.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace ClearEyeQ.Predictive.Tests.Unit;

public sealed class GenerateForecastHandlerTests
{
    private readonly IPredictionRepository _repository;
    private readonly IPredictiveMLClient _mlClient;
    private readonly IForecastCache _cache;
    private readonly IMediator _mediator;
    private readonly ILogger<GenerateForecastHandler> _logger;
    private readonly GenerateForecastHandler _handler;

    public GenerateForecastHandlerTests()
    {
        _repository = Substitute.For<IPredictionRepository>();
        _mlClient = Substitute.For<IPredictiveMLClient>();
        _cache = Substitute.For<IForecastCache>();
        _mediator = Substitute.For<IMediator>();
        _logger = Substitute.For<ILogger<GenerateForecastHandler>>();

        _handler = new GenerateForecastHandler(
            _repository, _mlClient, _cache, _mediator, _logger);
    }

    [Fact]
    public async Task Handle_ShouldCreatePredictionAndReturnId()
    {
        var command = new GenerateForecastCommand(Guid.NewGuid(), Guid.NewGuid(), 3);

        var forecastDays = new List<ForecastDay>
        {
            new(DateOnly.FromDateTime(DateTime.UtcNow), 0.5, RiskLevel.Moderate, "Pollen", 0.4, 0.6),
            new(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), 0.6, RiskLevel.High, "AQI", 0.5, 0.7),
            new(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)), 0.4, RiskLevel.Low, "Sleep", 0.3, 0.5)
        };

        _mlClient
            .GenerateForecastAsync(
                Arg.Any<UserId>(), Arg.Any<List<TimeSeriesInput>>(),
                Arg.Any<List<TimeSeriesInput>>(), Arg.Any<List<TimeSeriesInput>>(),
                Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(forecastDays);

        _mlClient
            .DetectFlareUpAsync(
                Arg.Any<UserId>(), Arg.Any<List<TimeSeriesInput>>(),
                Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(new FlareUpAlert(0.3, ["Pollen"], ["Take medicine"], RiskLevel.Low));

        _mlClient
            .ModelTrajectoryAsync(
                Arg.Any<UserId>(), Arg.Any<List<TimeSeriesInput>>(),
                Arg.Any<List<string>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new TrajectoryModel(
                6,
                [new TrajectoryPoint(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)), 0.5, 0.3, 0.7)],
                [new TrajectoryPoint(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)), 0.8, 0.6, 1.0)]));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBe(Guid.Empty);
        await _repository.Received(1).AddAsync(Arg.Any<Prediction>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).UpdateAsync(Arg.Any<Prediction>(), Arg.Any<CancellationToken>());
        await _cache.Received(1).SetForecastAsync(
            Arg.Any<UserId>(), Arg.Any<TenantId>(),
            Arg.Any<string>(), Arg.Any<TimeSpan>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMLClientFails_ShouldMarkPredictionFailed()
    {
        var command = new GenerateForecastCommand(Guid.NewGuid(), Guid.NewGuid(), 3);

        _mlClient
            .GenerateForecastAsync(
                Arg.Any<UserId>(), Arg.Any<List<TimeSeriesInput>>(),
                Arg.Any<List<TimeSeriesInput>>(), Arg.Any<List<TimeSeriesInput>>(),
                Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns<List<ForecastDay>>(x => throw new InvalidOperationException("ML service unavailable"));

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await _repository.Received(1).UpdateAsync(
            Arg.Is<Prediction>(p => p.Status == PredictionStatus.Failed),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCacheForecastInRedis()
    {
        var command = new GenerateForecastCommand(Guid.NewGuid(), Guid.NewGuid(), 3);

        _mlClient
            .GenerateForecastAsync(
                Arg.Any<UserId>(), Arg.Any<List<TimeSeriesInput>>(),
                Arg.Any<List<TimeSeriesInput>>(), Arg.Any<List<TimeSeriesInput>>(),
                Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<ForecastDay>
            {
                new(DateOnly.FromDateTime(DateTime.UtcNow), 0.5, RiskLevel.Low, "Sleep", 0.3, 0.7)
            });

        _mlClient
            .DetectFlareUpAsync(
                Arg.Any<UserId>(), Arg.Any<List<TimeSeriesInput>>(),
                Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(new FlareUpAlert(0.2, [], [], RiskLevel.Low));

        _mlClient
            .ModelTrajectoryAsync(
                Arg.Any<UserId>(), Arg.Any<List<TimeSeriesInput>>(),
                Arg.Any<List<string>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new TrajectoryModel(6, [], []));

        await _handler.Handle(command, CancellationToken.None);

        await _cache.Received(1).SetForecastAsync(
            Arg.Any<UserId>(),
            Arg.Any<TenantId>(),
            Arg.Is<string>(s => s.Contains("predictedScore")),
            TimeSpan.FromHours(1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithHighFlareUp_ShouldPublishWarningEvent()
    {
        var command = new GenerateForecastCommand(Guid.NewGuid(), Guid.NewGuid(), 3);

        _mlClient
            .GenerateForecastAsync(
                Arg.Any<UserId>(), Arg.Any<List<TimeSeriesInput>>(),
                Arg.Any<List<TimeSeriesInput>>(), Arg.Any<List<TimeSeriesInput>>(),
                Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<ForecastDay>
            {
                new(DateOnly.FromDateTime(DateTime.UtcNow), 0.9, RiskLevel.Critical, "AQI", 0.8, 1.0)
            });

        _mlClient
            .DetectFlareUpAsync(
                Arg.Any<UserId>(), Arg.Any<List<TimeSeriesInput>>(),
                Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(new FlareUpAlert(0.9, ["AQI", "Pollen"], ["Stay indoors"], RiskLevel.Critical));

        _mlClient
            .ModelTrajectoryAsync(
                Arg.Any<UserId>(), Arg.Any<List<TimeSeriesInput>>(),
                Arg.Any<List<string>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new TrajectoryModel(6, [], []));

        await _handler.Handle(command, CancellationToken.None);

        await _mediator.Received().Publish(
            Arg.Any<Domain.Events.FlareUpWarningEvent>(),
            Arg.Any<CancellationToken>());
    }
}
