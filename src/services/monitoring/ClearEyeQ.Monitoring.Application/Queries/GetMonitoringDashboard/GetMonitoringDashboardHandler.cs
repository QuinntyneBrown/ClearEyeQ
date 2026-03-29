using ClearEyeQ.Monitoring.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Monitoring.Application.Queries.GetMonitoringDashboard;

public sealed class GetMonitoringDashboardHandler(IMonitoringRepository repository)
    : IRequestHandler<GetMonitoringDashboardQuery, MonitoringDashboardDto>
{
    public async Task<MonitoringDashboardDto> Handle(
        GetMonitoringDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var tenantId = new TenantId(request.TenantId);

        var activeSession = await repository.GetActiveSessionAsync(userId, tenantId, cancellationToken);
        var recentSessions = await repository.GetRecentSessionsAsync(userId, tenantId, 10, cancellationToken);

        var todayDataPoints = recentSessions
            .SelectMany(s => s.WearableDataPoints)
            .Where(dp => dp.Timestamp.Date == DateTimeOffset.UtcNow.Date)
            .ToList();

        var latestBlinkSample = recentSessions
            .Where(s => s.BlinkRateSample is not null)
            .Select(s => s.BlinkRateSample!)
            .OrderByDescending(b => b.MeasuredAt)
            .FirstOrDefault();

        var latestSleep = recentSessions
            .Where(s => s.SleepRecord is not null)
            .Select(s => s.SleepRecord!)
            .OrderByDescending(s => s.Date)
            .FirstOrDefault();

        SleepSummaryDto? sleepSummary = latestSleep is not null
            ? new SleepSummaryDto(
                latestSleep.Date,
                latestSleep.Duration,
                latestSleep.QualityScore,
                latestSleep.Stages.SleepEfficiency)
            : null;

        var recentMetrics = todayDataPoints
            .OrderByDescending(dp => dp.Timestamp)
            .Take(50)
            .Select(dp => new WearableDataPointDto(
                dp.Source.ToString(),
                dp.MetricType.ToString(),
                dp.Value,
                dp.Timestamp))
            .ToList();

        return new MonitoringDashboardDto(
            ActiveSessionId: activeSession?.SessionId,
            TotalDataPointsToday: todayDataPoints.Count,
            LatestBlinkRate: latestBlinkSample?.BlinksPerMinute,
            LatestFatigueScore: latestBlinkSample?.FatigueScore,
            LatestSleep: sleepSummary,
            RecentMetrics: recentMetrics);
    }
}
