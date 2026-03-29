using Xunit;
using ClearEyeQ.Monitoring.Domain.Aggregates;
using ClearEyeQ.Monitoring.Domain.Enums;
using ClearEyeQ.Monitoring.Domain.Events;
using ClearEyeQ.Monitoring.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using FluentAssertions;

namespace ClearEyeQ.Monitoring.Tests.Unit;

public sealed class MonitoringSessionTests
{
    private readonly UserId _userId = UserId.New();
    private readonly TenantId _tenantId = TenantId.New();

    [Fact]
    public void Create_ShouldReturnActiveSession()
    {
        var session = MonitoringSession.Create(_userId, _tenantId);

        session.SessionId.Should().NotBeEmpty();
        session.UserId.Should().Be(_userId);
        session.TenantId.Should().Be(_tenantId);
        session.Status.Should().Be(MonitoringSessionStatus.Active);
        session.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        session.WearableDataPoints.Should().BeEmpty();
        session.SleepRecord.Should().BeNull();
        session.BlinkRateSample.Should().BeNull();
    }

    [Fact]
    public void AddWearableData_ShouldAddDataPointAndRaiseEvent()
    {
        var session = MonitoringSession.Create(_userId, _tenantId);
        var timestamp = DateTimeOffset.UtcNow;

        session.AddWearableData(WearableSource.Oura, MetricType.HeartRate, 72.0, timestamp);

        session.WearableDataPoints.Should().HaveCount(1);
        var dp = session.WearableDataPoints[0];
        dp.Source.Should().Be(WearableSource.Oura);
        dp.MetricType.Should().Be(MetricType.HeartRate);
        dp.Value.Should().Be(72.0);
        dp.Timestamp.Should().Be(timestamp);

        session.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<MonitoringDataReceivedEvent>();
    }

    [Fact]
    public void RecordBlinkRate_ShouldSetSampleAndRaiseEvent()
    {
        var session = MonitoringSession.Create(_userId, _tenantId);
        var measuredAt = DateTimeOffset.UtcNow;

        session.RecordBlinkRate(15.0, 0.3, measuredAt);

        session.BlinkRateSample.Should().NotBeNull();
        session.BlinkRateSample!.BlinksPerMinute.Should().Be(15.0);
        session.BlinkRateSample.FatigueScore.Should().Be(0.3);
        session.BlinkRateSample.MeasuredAt.Should().Be(measuredAt);

        session.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<MonitoringDataReceivedEvent>();
    }

    [Fact]
    public void RecordSleep_ShouldSetRecordAndRaiseEvent()
    {
        var session = MonitoringSession.Create(_userId, _tenantId);
        var stages = new SleepStages(
            Deep: TimeSpan.FromHours(1.5),
            Light: TimeSpan.FromHours(3),
            Rem: TimeSpan.FromHours(1.5),
            Awake: TimeSpan.FromMinutes(30));

        session.RecordSleep(DateOnly.FromDateTime(DateTime.Today), TimeSpan.FromHours(7), stages, 0.85);

        session.SleepRecord.Should().NotBeNull();
        session.SleepRecord!.Date.Should().Be(DateOnly.FromDateTime(DateTime.Today));
        session.SleepRecord.Duration.Should().Be(TimeSpan.FromHours(7));
        session.SleepRecord.QualityScore.Should().Be(0.85);

        session.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<MonitoringDataReceivedEvent>();
    }

    [Fact]
    public void Close_ShouldSetStatusToClosed()
    {
        var session = MonitoringSession.Create(_userId, _tenantId);

        session.Close();

        session.Status.Should().Be(MonitoringSessionStatus.Closed);
    }

    [Fact]
    public void AddWearableData_WhenClosed_ShouldThrow()
    {
        var session = MonitoringSession.Create(_userId, _tenantId);
        session.Close();

        var act = () => session.AddWearableData(WearableSource.Oura, MetricType.HeartRate, 72.0, DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*closed*");
    }

    [Fact]
    public void RecordBlinkRate_WhenClosed_ShouldThrow()
    {
        var session = MonitoringSession.Create(_userId, _tenantId);
        session.Close();

        var act = () => session.RecordBlinkRate(15.0, 0.3, DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*closed*");
    }

    [Fact]
    public void RecordSleep_WhenClosed_ShouldThrow()
    {
        var session = MonitoringSession.Create(_userId, _tenantId);
        session.Close();

        var stages = new SleepStages(
            Deep: TimeSpan.FromHours(1),
            Light: TimeSpan.FromHours(3),
            Rem: TimeSpan.FromHours(1),
            Awake: TimeSpan.FromMinutes(30));

        var act = () => session.RecordSleep(DateOnly.FromDateTime(DateTime.Today), TimeSpan.FromHours(6), stages, 0.7);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*closed*");
    }

    [Fact]
    public void PartitionKey_ShouldBeTenantUserComposite()
    {
        var session = MonitoringSession.Create(_userId, _tenantId);

        session.PartitionKey.Value.Should().Be($"{_tenantId.Value}|{_userId.Value}");
    }

    [Fact]
    public void AddMultipleWearableDataPoints_ShouldAccumulate()
    {
        var session = MonitoringSession.Create(_userId, _tenantId);

        session.AddWearableData(WearableSource.Oura, MetricType.HeartRate, 72.0, DateTimeOffset.UtcNow);
        session.AddWearableData(WearableSource.Oura, MetricType.SpO2, 98.0, DateTimeOffset.UtcNow);
        session.AddWearableData(WearableSource.AppleHealth, MetricType.Steps, 5000, DateTimeOffset.UtcNow);

        session.WearableDataPoints.Should().HaveCount(3);
        session.DomainEvents.Should().HaveCount(3);
    }
}
