using ClearEyeQ.Environmental.Application.Interfaces;
using ClearEyeQ.Environmental.Domain.Aggregates;
using ClearEyeQ.Environmental.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Environmental.Application.Commands.CaptureSnapshot;

public sealed class CaptureSnapshotHandler(
    IEnvironmentalSnapshotRepository repository,
    IAirQualityClient airQualityClient,
    IPollenClient pollenClient,
    IWeatherClient weatherClient,
    ILogger<CaptureSnapshotHandler> logger)
    : IRequestHandler<CaptureSnapshotCommand, Guid>
{
    public async Task<Guid> Handle(CaptureSnapshotCommand request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var tenantId = new TenantId(request.TenantId);
        var location = new GeoLocation(request.Latitude, request.Longitude);
        location.Validate();

        var snapshot = EnvironmentalSnapshot.Create(userId, tenantId, location);

        // Fetch environmental data in parallel
        var airQualityTask = SafeFetchAsync(
            () => airQualityClient.GetAirQualityAsync(request.Latitude, request.Longitude, cancellationToken),
            "AirQuality");

        var pollenTask = SafeFetchAsync(
            () => pollenClient.GetPollenCountAsync(request.Latitude, request.Longitude, cancellationToken),
            "Pollen");

        var weatherTask = SafeFetchAsync(
            () => weatherClient.GetWeatherDataAsync(request.Latitude, request.Longitude, cancellationToken),
            "Weather");

        await Task.WhenAll(airQualityTask, pollenTask, weatherTask);

        var airQuality = await airQualityTask;
        var pollen = await pollenTask;
        var weather = await weatherTask;

        if (airQuality is not null)
        {
            snapshot.SetAirQuality(airQuality.Aqi, airQuality.Pm25, airQuality.Pm10);
        }

        if (pollen is not null)
        {
            snapshot.SetPollen(pollen.TreeCount, pollen.GrassCount, pollen.WeedCount);
        }

        if (weather is not null)
        {
            snapshot.SetUv(weather.UvIndex);
            snapshot.SetHumidity(weather.HumidityPercentage);
        }

        await repository.AddAsync(snapshot, cancellationToken);

        return snapshot.SnapshotId;
    }

    private async Task<T?> SafeFetchAsync<T>(Func<Task<T?>> fetch, string source) where T : class
    {
        try
        {
            return await fetch();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch {Source} data. Snapshot will be created without it.", source);
            return null;
        }
    }
}
