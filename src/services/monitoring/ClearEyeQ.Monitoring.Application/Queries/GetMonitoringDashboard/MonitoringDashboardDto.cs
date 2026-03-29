namespace ClearEyeQ.Monitoring.Application.Queries.GetMonitoringDashboard;

public sealed record MonitoringDashboardDto(
    Guid? ActiveSessionId,
    int TotalDataPointsToday,
    double? LatestBlinkRate,
    double? LatestFatigueScore,
    SleepSummaryDto? LatestSleep,
    IReadOnlyList<WearableDataPointDto> RecentMetrics);

public sealed record SleepSummaryDto(
    DateOnly Date,
    TimeSpan Duration,
    double QualityScore,
    double SleepEfficiency);

public sealed record WearableDataPointDto(
    string Source,
    string MetricType,
    double Value,
    DateTimeOffset Timestamp);
