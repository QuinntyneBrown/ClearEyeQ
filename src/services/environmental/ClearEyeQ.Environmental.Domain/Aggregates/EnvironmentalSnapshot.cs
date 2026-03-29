using ClearEyeQ.Environmental.Domain.Events;
using ClearEyeQ.Environmental.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using PartitionKey = ClearEyeQ.SharedKernel.Domain.ValueObjects.PartitionKey;

namespace ClearEyeQ.Environmental.Domain.Aggregates;

public sealed class EnvironmentalSnapshot : AggregateRoot
{
    public Guid SnapshotId => Id;
    public UserId UserId { get; private set; }
    private TenantId _tenantId;
    public override TenantId TenantId => _tenantId;
    public override PartitionKey PartitionKey => PartitionKey.ForUserInTenant(_tenantId, UserId);

    public DateTimeOffset CapturedAt { get; private set; }
    public AirQualityReading? AirQualityReading { get; private set; }
    public PollenCount? PollenCount { get; private set; }
    public UvIndex? UvIndex { get; private set; }
    public HumidityReading? HumidityReading { get; private set; }
    public ScreenTimeRecord? ScreenTimeRecord { get; private set; }
    public GeoLocation? GeoLocation { get; private set; }

    private EnvironmentalSnapshot() { }

    public static EnvironmentalSnapshot Create(UserId userId, TenantId tenantId, GeoLocation? location = null)
    {
        var snapshot = new EnvironmentalSnapshot
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            _tenantId = tenantId,
            CapturedAt = DateTimeOffset.UtcNow,
            GeoLocation = location,
            Audit = AuditMetadata.Create(userId.Value.ToString())
        };

        snapshot.AddDomainEvent(new EnvironmentalSnapshotCapturedEvent(
            snapshot.SnapshotId, userId, tenantId, snapshot.CapturedAt));

        return snapshot;
    }

    public void SetAirQuality(int aqi, double pm25, double pm10)
    {
        var level = AirQualityReading.ClassifyAqi(aqi);
        AirQualityReading = new AirQualityReading(aqi, pm25, pm10, level);
    }

    public void SetPollen(int tree, int grass, int weed)
    {
        var overallLevel = PollenCount.ClassifyOverallLevel(tree, grass, weed);
        PollenCount = new PollenCount(tree, grass, weed, overallLevel);
    }

    public void SetUv(double value)
    {
        var riskCategory = UvIndex.ClassifyRisk(value);
        UvIndex = new UvIndex(value, riskCategory);
    }

    public void SetHumidity(double percentage)
    {
        if (percentage is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(percentage), "Humidity must be between 0 and 100.");

        HumidityReading = new HumidityReading(percentage);
    }

    public void SetScreenTime(TimeSpan totalDuration, Dictionary<string, TimeSpan> appBreakdown)
    {
        if (totalDuration < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(totalDuration), "Screen time duration cannot be negative.");

        ScreenTimeRecord = new ScreenTimeRecord(totalDuration, appBreakdown);
    }
}
