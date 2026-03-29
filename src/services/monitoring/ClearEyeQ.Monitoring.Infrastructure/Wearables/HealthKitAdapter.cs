using System.Net.Http.Json;
using System.Text.Json;
using ClearEyeQ.Monitoring.Application.Interfaces;
using ClearEyeQ.Monitoring.Domain.Enums;

namespace ClearEyeQ.Monitoring.Infrastructure.Wearables;

public sealed class HealthKitAdapter(HttpClient httpClient) : IWearableAdapter
{
    public WearableSource Source => WearableSource.AppleHealth;

    public async Task<IReadOnlyList<WearableMetric>> FetchLatestMetricsAsync(
        string accessToken, DateTimeOffset since, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get,
            $"https://api.apple-health-proxy.cleareyeq.com/v1/metrics?since={since:O}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<HealthKitResponse>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);

        if (payload?.Samples is null)
            return [];

        var metrics = new List<WearableMetric>();

        foreach (var sample in payload.Samples)
        {
            var metricType = MapMetricType(sample.Type);
            if (metricType.HasValue)
            {
                metrics.Add(new WearableMetric(metricType.Value, sample.Value, sample.StartDate));
            }
        }

        return metrics.AsReadOnly();
    }

    private static MetricType? MapMetricType(string healthKitType) => healthKitType switch
    {
        "HKQuantityTypeIdentifierHeartRate" => MetricType.HeartRate,
        "HKQuantityTypeIdentifierHeartRateVariabilitySDNN" => MetricType.HeartRateVariability,
        "HKQuantityTypeIdentifierOxygenSaturation" => MetricType.SpO2,
        "HKQuantityTypeIdentifierStepCount" => MetricType.Steps,
        "HKCategoryTypeIdentifierSleepAnalysis" => MetricType.SleepDuration,
        _ => null
    };

    private sealed record HealthKitResponse(List<HealthKitSample>? Samples);
    private sealed record HealthKitSample(string Type, double Value, DateTimeOffset StartDate);
}
