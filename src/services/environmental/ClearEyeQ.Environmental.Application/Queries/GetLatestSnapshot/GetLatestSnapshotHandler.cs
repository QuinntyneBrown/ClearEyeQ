using ClearEyeQ.Environmental.Application.Interfaces;
using ClearEyeQ.Environmental.Domain.Aggregates;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Environmental.Application.Queries.GetLatestSnapshot;

public sealed class GetLatestSnapshotHandler(IEnvironmentalSnapshotRepository repository)
    : IRequestHandler<GetLatestSnapshotQuery, EnvironmentalSnapshotDto?>
{
    public async Task<EnvironmentalSnapshotDto?> Handle(
        GetLatestSnapshotQuery request,
        CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var tenantId = new TenantId(request.TenantId);

        var snapshot = await repository.GetLatestAsync(userId, tenantId, cancellationToken);

        return snapshot is null ? null : MapToDto(snapshot);
    }

    internal static EnvironmentalSnapshotDto MapToDto(EnvironmentalSnapshot snapshot) => new(
        SnapshotId: snapshot.SnapshotId,
        CapturedAt: snapshot.CapturedAt,
        AirQuality: snapshot.AirQualityReading is not null
            ? new AirQualityDto(
                snapshot.AirQualityReading.Aqi,
                snapshot.AirQualityReading.Pm25,
                snapshot.AirQualityReading.Pm10,
                snapshot.AirQualityReading.Level.ToString())
            : null,
        Pollen: snapshot.PollenCount is not null
            ? new PollenDto(
                snapshot.PollenCount.Tree,
                snapshot.PollenCount.Grass,
                snapshot.PollenCount.Weed,
                snapshot.PollenCount.OverallLevel)
            : null,
        Uv: snapshot.UvIndex is not null
            ? new UvDto(snapshot.UvIndex.Value, snapshot.UvIndex.RiskCategory)
            : null,
        Humidity: snapshot.HumidityReading is not null
            ? new HumidityDto(
                snapshot.HumidityReading.Percentage,
                snapshot.HumidityReading.IsComfortableForEyes)
            : null,
        ScreenTime: snapshot.ScreenTimeRecord is not null
            ? new ScreenTimeDto(
                snapshot.ScreenTimeRecord.TotalDuration,
                snapshot.ScreenTimeRecord.TotalHours,
                snapshot.ScreenTimeRecord.AppBreakdown)
            : null,
        Location: snapshot.GeoLocation is not null
            ? new GeoLocationDto(snapshot.GeoLocation.Latitude, snapshot.GeoLocation.Longitude)
            : null);
}
