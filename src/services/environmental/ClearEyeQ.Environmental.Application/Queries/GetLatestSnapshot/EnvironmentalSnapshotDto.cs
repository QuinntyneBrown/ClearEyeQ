namespace ClearEyeQ.Environmental.Application.Queries.GetLatestSnapshot;

public sealed record EnvironmentalSnapshotDto(
    Guid SnapshotId,
    DateTimeOffset CapturedAt,
    AirQualityDto? AirQuality,
    PollenDto? Pollen,
    UvDto? Uv,
    HumidityDto? Humidity,
    ScreenTimeDto? ScreenTime,
    GeoLocationDto? Location);

public sealed record AirQualityDto(int Aqi, double Pm25, double Pm10, string Level);
public sealed record PollenDto(int Tree, int Grass, int Weed, string OverallLevel);
public sealed record UvDto(double Value, string RiskCategory);
public sealed record HumidityDto(double Percentage, bool IsComfortableForEyes);
public sealed record ScreenTimeDto(TimeSpan TotalDuration, double TotalHours, Dictionary<string, TimeSpan> AppBreakdown);
public sealed record GeoLocationDto(double Latitude, double Longitude);
