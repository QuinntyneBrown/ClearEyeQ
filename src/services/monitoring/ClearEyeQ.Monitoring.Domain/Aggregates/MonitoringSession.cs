using ClearEyeQ.Monitoring.Domain.Entities;
using ClearEyeQ.Monitoring.Domain.Enums;
using ClearEyeQ.Monitoring.Domain.Events;
using ClearEyeQ.Monitoring.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using PartitionKey = ClearEyeQ.SharedKernel.Domain.ValueObjects.PartitionKey;

namespace ClearEyeQ.Monitoring.Domain.Aggregates;

public sealed class MonitoringSession : AggregateRoot
{
    private readonly List<WearableDataPoint> _wearableDataPoints = [];

    public Guid SessionId => Id;
    public UserId UserId { get; private set; }
    private TenantId _tenantId;
    public override TenantId TenantId => _tenantId;
    public override PartitionKey PartitionKey => PartitionKey.ForUserInTenant(_tenantId, UserId);

    public IReadOnlyList<WearableDataPoint> WearableDataPoints => _wearableDataPoints.AsReadOnly();
    public SleepRecord? SleepRecord { get; private set; }
    public BlinkRateSample? BlinkRateSample { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public MonitoringSessionStatus Status { get; private set; }

    private MonitoringSession() { }

    public static MonitoringSession Create(UserId userId, TenantId tenantId)
    {
        var session = new MonitoringSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            _tenantId = tenantId,
            CreatedAt = DateTimeOffset.UtcNow,
            Status = MonitoringSessionStatus.Active,
            Audit = AuditMetadata.Create(userId.Value.ToString())
        };

        return session;
    }

    public void AddWearableData(WearableSource source, MetricType metricType, double value, DateTimeOffset timestamp)
    {
        EnsureActive();

        var dataPoint = WearableDataPoint.Create(source, metricType, value, timestamp);
        _wearableDataPoints.Add(dataPoint);

        AddDomainEvent(new MonitoringDataReceivedEvent(
            SessionId, UserId, TenantId, $"Wearable:{metricType}", DateTimeOffset.UtcNow));
    }

    public void RecordBlinkRate(double blinksPerMinute, double fatigueScore, DateTimeOffset measuredAt)
    {
        EnsureActive();

        BlinkRateSample = BlinkRateSample.Create(blinksPerMinute, fatigueScore, measuredAt);

        AddDomainEvent(new MonitoringDataReceivedEvent(
            SessionId, UserId, TenantId, "BlinkRate", DateTimeOffset.UtcNow));
    }

    public void RecordSleep(DateOnly date, TimeSpan duration, SleepStages stages, double qualityScore)
    {
        EnsureActive();

        SleepRecord = SleepRecord.Create(date, duration, stages, qualityScore);

        AddDomainEvent(new MonitoringDataReceivedEvent(
            SessionId, UserId, TenantId, "Sleep", DateTimeOffset.UtcNow));
    }

    public void Close()
    {
        EnsureActive();
        Status = MonitoringSessionStatus.Closed;
        Audit = Audit.WithModification(UserId.Value.ToString());
    }

    private void EnsureActive()
    {
        if (Status == MonitoringSessionStatus.Closed)
            throw new InvalidOperationException("Cannot modify a closed monitoring session.");
    }
}
