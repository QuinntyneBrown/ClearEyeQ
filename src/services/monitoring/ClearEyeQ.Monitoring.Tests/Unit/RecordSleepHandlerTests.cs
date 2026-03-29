using Xunit;
using ClearEyeQ.Monitoring.Application.Commands.RecordSleep;
using ClearEyeQ.Monitoring.Application.Interfaces;
using ClearEyeQ.Monitoring.Domain.Aggregates;
using ClearEyeQ.Monitoring.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace ClearEyeQ.Monitoring.Tests.Unit;

public sealed class RecordSleepHandlerTests
{
    private readonly IMonitoringRepository _repository = Substitute.For<IMonitoringRepository>();
    private readonly ISleepScorer _sleepScorer = Substitute.For<ISleepScorer>();
    private readonly RecordSleepHandler _handler;

    public RecordSleepHandlerTests()
    {
        _handler = new RecordSleepHandler(_repository, _sleepScorer);
    }

    [Fact]
    public async Task Handle_WhenNoActiveSession_ShouldCreateNewSessionWithSleep()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var command = new RecordSleepCommand(
            UserId: userId,
            TenantId: tenantId,
            Date: DateOnly.FromDateTime(DateTime.Today),
            Duration: TimeSpan.FromHours(7.5),
            DeepSleep: TimeSpan.FromHours(1.5),
            LightSleep: TimeSpan.FromHours(3),
            RemSleep: TimeSpan.FromHours(1.5),
            AwakeTime: TimeSpan.FromMinutes(30));

        _repository.GetActiveSessionAsync(
            Arg.Any<UserId>(),
            Arg.Any<TenantId>(),
            Arg.Any<CancellationToken>())
            .Returns((MonitoringSession?)null);

        _sleepScorer.CalculateQualityScore(
            Arg.Any<TimeSpan>(),
            Arg.Any<SleepStages>())
            .Returns(0.82);

        // Act
        var sessionId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        sessionId.Should().NotBeEmpty();
        await _repository.Received(1).AddAsync(
            Arg.Is<MonitoringSession>(s =>
                s.SleepRecord != null &&
                s.SleepRecord.QualityScore == 0.82 &&
                s.SleepRecord.Date == DateOnly.FromDateTime(DateTime.Today)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenActiveSessionExists_ShouldUpdateExistingSession()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var existingSession = MonitoringSession.Create(new UserId(userId), new TenantId(tenantId));

        var command = new RecordSleepCommand(
            UserId: userId,
            TenantId: tenantId,
            Date: DateOnly.FromDateTime(DateTime.Today),
            Duration: TimeSpan.FromHours(8),
            DeepSleep: TimeSpan.FromHours(2),
            LightSleep: TimeSpan.FromHours(3),
            RemSleep: TimeSpan.FromHours(2),
            AwakeTime: TimeSpan.FromMinutes(20));

        _repository.GetActiveSessionAsync(
            Arg.Any<UserId>(),
            Arg.Any<TenantId>(),
            Arg.Any<CancellationToken>())
            .Returns(existingSession);

        _sleepScorer.CalculateQualityScore(
            Arg.Any<TimeSpan>(),
            Arg.Any<SleepStages>())
            .Returns(0.91);

        // Act
        var sessionId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        sessionId.Should().Be(existingSession.SessionId);
        await _repository.Received(1).UpdateAsync(
            Arg.Is<MonitoringSession>(s => s.SleepRecord != null && s.SleepRecord.QualityScore == 0.91),
            Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().AddAsync(Arg.Any<MonitoringSession>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUseSleepScorerToCalculateQuality()
    {
        // Arrange
        var command = new RecordSleepCommand(
            UserId: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            Date: DateOnly.FromDateTime(DateTime.Today),
            Duration: TimeSpan.FromHours(6),
            DeepSleep: TimeSpan.FromHours(1),
            LightSleep: TimeSpan.FromHours(2.5),
            RemSleep: TimeSpan.FromHours(1),
            AwakeTime: TimeSpan.FromMinutes(45));

        _repository.GetActiveSessionAsync(
            Arg.Any<UserId>(),
            Arg.Any<TenantId>(),
            Arg.Any<CancellationToken>())
            .Returns((MonitoringSession?)null);

        _sleepScorer.CalculateQualityScore(
            Arg.Is<TimeSpan>(d => d == TimeSpan.FromHours(6)),
            Arg.Is<SleepStages>(s =>
                s.Deep == TimeSpan.FromHours(1) &&
                s.Light == TimeSpan.FromHours(2.5) &&
                s.Rem == TimeSpan.FromHours(1) &&
                s.Awake == TimeSpan.FromMinutes(45)))
            .Returns(0.65);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _sleepScorer.Received(1).CalculateQualityScore(
            TimeSpan.FromHours(6),
            Arg.Any<SleepStages>());
    }
}
