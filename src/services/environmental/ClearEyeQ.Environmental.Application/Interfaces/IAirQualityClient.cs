namespace ClearEyeQ.Environmental.Application.Interfaces;

public record AirQualityData(int Aqi, double Pm25, double Pm10);

public interface IAirQualityClient
{
    Task<AirQualityData?> GetAirQualityAsync(double latitude, double longitude, CancellationToken ct);
}
