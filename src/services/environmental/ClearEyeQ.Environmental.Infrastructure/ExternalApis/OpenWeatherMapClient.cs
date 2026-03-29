using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClearEyeQ.Environmental.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Environmental.Infrastructure.ExternalApis;

/// <summary>
/// Implements both IAirQualityClient and IWeatherClient using OpenWeatherMap APIs.
/// </summary>
public sealed class OpenWeatherMapClient(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<OpenWeatherMapClient> logger) : IAirQualityClient, IWeatherClient
{
    private string ApiKey => configuration["OpenWeatherMap:ApiKey"]
        ?? throw new InvalidOperationException("OpenWeatherMap API key is not configured.");

    private const string BaseUrl = "https://api.openweathermap.org";

    public async Task<AirQualityData?> GetAirQualityAsync(double latitude, double longitude, CancellationToken ct)
    {
        var url = $"{BaseUrl}/data/2.5/air_pollution?lat={latitude}&lon={longitude}&appid={ApiKey}";

        var response = await httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<OwmAirPollutionResponse>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);

        if (payload?.List is null || payload.List.Count == 0)
        {
            logger.LogWarning("OpenWeatherMap returned empty air pollution data for ({Lat}, {Lon}).", latitude, longitude);
            return null;
        }

        var current = payload.List[0];
        var aqi = current.Main.Aqi;
        var pm25 = current.Components.Pm25;
        var pm10 = current.Components.Pm10;

        // OpenWeatherMap AQI is 1-5 scale; convert to EPA 0-500 scale
        var epaAqi = ConvertOwmAqiToEpa(aqi, pm25);

        return new AirQualityData(epaAqi, pm25, pm10);
    }

    public async Task<WeatherData?> GetWeatherDataAsync(double latitude, double longitude, CancellationToken ct)
    {
        var url = $"{BaseUrl}/data/3.0/onecall?lat={latitude}&lon={longitude}&exclude=minutely,hourly,daily,alerts&appid={ApiKey}";

        var response = await httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<OwmOneCallResponse>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);

        if (payload?.Current is null)
        {
            logger.LogWarning("OpenWeatherMap returned empty weather data for ({Lat}, {Lon}).", latitude, longitude);
            return null;
        }

        return new WeatherData(payload.Current.Uvi, payload.Current.Humidity);
    }

    /// <summary>
    /// Converts OpenWeatherMap's 1-5 AQI scale to EPA's 0-500 scale using PM2.5 as primary indicator.
    /// </summary>
    private static int ConvertOwmAqiToEpa(int owmAqi, double pm25)
    {
        // Use PM2.5 breakpoints for more accurate EPA AQI calculation
        return pm25 switch
        {
            <= 12.0 => (int)(pm25 / 12.0 * 50),
            <= 35.4 => (int)(50 + (pm25 - 12.1) / (35.4 - 12.1) * 50),
            <= 55.4 => (int)(100 + (pm25 - 35.5) / (55.4 - 35.5) * 50),
            <= 150.4 => (int)(150 + (pm25 - 55.5) / (150.4 - 55.5) * 50),
            <= 250.4 => (int)(200 + (pm25 - 150.5) / (250.4 - 150.5) * 100),
            <= 500.4 => (int)(300 + (pm25 - 250.5) / (500.4 - 250.5) * 200),
            _ => 500
        };
    }

    // OpenWeatherMap response models

    private sealed record OwmAirPollutionResponse(List<OwmAirPollutionEntry> List);

    private sealed record OwmAirPollutionEntry(OwmMainAqi Main, OwmComponents Components);

    private sealed record OwmMainAqi(int Aqi);

    private sealed record OwmComponents(
        [property: JsonPropertyName("pm2_5")] double Pm25,
        [property: JsonPropertyName("pm10")] double Pm10);

    private sealed record OwmOneCallResponse(OwmCurrentWeather? Current);

    private sealed record OwmCurrentWeather(double Uvi, double Humidity);
}
