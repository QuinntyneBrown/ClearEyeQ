using System.Net.Http.Json;
using System.Text.Json;
using ClearEyeQ.Environmental.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Environmental.Infrastructure.ExternalApis;

/// <summary>
/// Fetches pollen data from the Ambee Pollen API.
/// </summary>
public sealed class AmbeePollenClient(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<AmbeePollenClient> logger) : IPollenClient
{
    private string ApiKey => configuration["Ambee:ApiKey"]
        ?? throw new InvalidOperationException("Ambee API key is not configured.");

    private const string BaseUrl = "https://api.ambeedata.com";

    public async Task<PollenData?> GetPollenCountAsync(double latitude, double longitude, CancellationToken ct)
    {
        var url = $"{BaseUrl}/latest/pollen/by-lat-lng?lat={latitude}&lng={longitude}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("x-api-key", ApiKey);
        request.Headers.Add("Accept", "application/json");

        var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<AmbeePollenResponse>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);

        if (payload?.Data is null || payload.Data.Count == 0)
        {
            logger.LogWarning("Ambee returned empty pollen data for ({Lat}, {Lon}).", latitude, longitude);
            return null;
        }

        var current = payload.Data[0];
        var treeCount = current.Count.TreePollen;
        var grassCount = current.Count.GrassPollen;
        var weedCount = current.Count.WeedPollen;

        return new PollenData(treeCount, grassCount, weedCount);
    }

    private sealed record AmbeePollenResponse(List<AmbeePollenEntry>? Data);
    private sealed record AmbeePollenEntry(AmbeePollenCount Count);
    private sealed record AmbeePollenCount(
        int TreePollen,
        int GrassPollen,
        int WeedPollen);
}
