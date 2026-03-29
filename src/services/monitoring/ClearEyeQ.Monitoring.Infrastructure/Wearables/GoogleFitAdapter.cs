using System.Net.Http.Json;
using System.Text.Json;
using ClearEyeQ.Monitoring.Application.Interfaces;
using ClearEyeQ.Monitoring.Domain.Enums;

namespace ClearEyeQ.Monitoring.Infrastructure.Wearables;

public sealed class GoogleFitAdapter(HttpClient httpClient) : IWearableAdapter
{
    public WearableSource Source => WearableSource.GoogleFit;

    public async Task<IReadOnlyList<WearableMetric>> FetchLatestMetricsAsync(
        string accessToken, DateTimeOffset since, CancellationToken ct)
    {
        var sinceMillis = since.ToUnixTimeMilliseconds();

        using var request = new HttpRequestMessage(HttpMethod.Get,
            $"https://www.googleapis.com/fitness/v1/users/me/dataSources?startTimeMillis={sinceMillis}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<GoogleFitResponse>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);

        if (payload?.DataPoints is null)
            return [];

        var metrics = new List<WearableMetric>();

        foreach (var point in payload.DataPoints)
        {
            var metricType = MapDataSourceType(point.DataSourceType);
            if (metricType.HasValue)
            {
                var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(point.TimestampMillis);
                metrics.Add(new WearableMetric(metricType.Value, point.Value, timestamp));
            }
        }

        return metrics.AsReadOnly();
    }

    private static MetricType? MapDataSourceType(string dataSourceType) => dataSourceType switch
    {
        "com.google.heart_rate.bpm" => MetricType.HeartRate,
        "com.google.heart_rate.variability" => MetricType.HeartRateVariability,
        "com.google.oxygen_saturation" => MetricType.SpO2,
        "com.google.step_count.delta" => MetricType.Steps,
        "com.google.sleep.segment" => MetricType.SleepDuration,
        _ => null
    };

    private sealed record GoogleFitResponse(List<GoogleFitDataPoint>? DataPoints);
    private sealed record GoogleFitDataPoint(string DataSourceType, double Value, long TimestampMillis);
}
