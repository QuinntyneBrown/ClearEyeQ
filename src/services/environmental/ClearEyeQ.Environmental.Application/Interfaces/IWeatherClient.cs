namespace ClearEyeQ.Environmental.Application.Interfaces;

public record WeatherData(double UvIndex, double HumidityPercentage);

public interface IWeatherClient
{
    Task<WeatherData?> GetWeatherDataAsync(double latitude, double longitude, CancellationToken ct);
}
