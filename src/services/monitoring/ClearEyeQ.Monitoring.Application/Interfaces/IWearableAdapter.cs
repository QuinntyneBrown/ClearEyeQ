using ClearEyeQ.Monitoring.Domain.Enums;

namespace ClearEyeQ.Monitoring.Application.Interfaces;

public record WearableMetric(MetricType MetricType, double Value, DateTimeOffset Timestamp);

public interface IWearableAdapter
{
    WearableSource Source { get; }
    Task<IReadOnlyList<WearableMetric>> FetchLatestMetricsAsync(string accessToken, DateTimeOffset since, CancellationToken ct);
}
