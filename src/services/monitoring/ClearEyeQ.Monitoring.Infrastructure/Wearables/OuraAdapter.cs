using System.Net.Http.Json;
using System.Text.Json;
using ClearEyeQ.Monitoring.Application.Interfaces;
using ClearEyeQ.Monitoring.Domain.Enums;

namespace ClearEyeQ.Monitoring.Infrastructure.Wearables;

public sealed class OuraAdapter(HttpClient httpClient) : IWearableAdapter
{
    public WearableSource Source => WearableSource.Oura;

    public async Task<IReadOnlyList<WearableMetric>> FetchLatestMetricsAsync(
        string accessToken, DateTimeOffset since, CancellationToken ct)
    {
        var startDate = since.ToString("yyyy-MM-dd");

        using var request = new HttpRequestMessage(HttpMethod.Get,
            $"https://api.ouraring.com/v2/usercollection/heartrate?start_date={startDate}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<OuraHeartRateResponse>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);

        var metrics = new List<WearableMetric>();

        if (payload?.Data is not null)
        {
            foreach (var entry in payload.Data)
            {
                metrics.Add(new WearableMetric(MetricType.HeartRate, entry.Bpm, entry.Timestamp));
            }
        }

        // Also fetch sleep data
        using var sleepRequest = new HttpRequestMessage(HttpMethod.Get,
            $"https://api.ouraring.com/v2/usercollection/sleep?start_date={startDate}");
        sleepRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var sleepResponse = await httpClient.SendAsync(sleepRequest, ct);
        sleepResponse.EnsureSuccessStatusCode();

        var sleepPayload = await sleepResponse.Content.ReadFromJsonAsync<OuraSleepResponse>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);

        if (sleepPayload?.Data is not null)
        {
            foreach (var sleep in sleepPayload.Data)
            {
                var timestamp = DateTimeOffset.Parse(sleep.BedtimeEnd);
                metrics.Add(new WearableMetric(MetricType.SleepDuration, sleep.TotalSleepDuration, timestamp));

                if (sleep.AverageHrv > 0)
                {
                    metrics.Add(new WearableMetric(MetricType.HeartRateVariability, sleep.AverageHrv, timestamp));
                }

                if (sleep.LowestHeartRate > 0)
                {
                    metrics.Add(new WearableMetric(MetricType.HeartRate, sleep.LowestHeartRate, timestamp));
                }
            }
        }

        return metrics.AsReadOnly();
    }

    private sealed record OuraHeartRateResponse(List<OuraHeartRateEntry>? Data);
    private sealed record OuraHeartRateEntry(double Bpm, DateTimeOffset Timestamp);

    private sealed record OuraSleepResponse(List<OuraSleepEntry>? Data);
    private sealed record OuraSleepEntry(
        double TotalSleepDuration,
        double AverageHrv,
        double LowestHeartRate,
        string BedtimeEnd);
}
