using ClearEyeQ.Environmental.Domain.Enums;

namespace ClearEyeQ.Environmental.Domain.ValueObjects;

public sealed record AirQualityReading(int Aqi, double Pm25, double Pm10, AirQualityLevel Level)
{
    public static AirQualityLevel ClassifyAqi(int aqi) => aqi switch
    {
        <= 50 => AirQualityLevel.Good,
        <= 100 => AirQualityLevel.Moderate,
        <= 150 => AirQualityLevel.UnhealthyForSensitive,
        <= 200 => AirQualityLevel.Unhealthy,
        <= 300 => AirQualityLevel.VeryUnhealthy,
        _ => AirQualityLevel.Hazardous
    };
}
