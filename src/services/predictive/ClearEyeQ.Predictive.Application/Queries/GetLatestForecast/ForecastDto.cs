namespace ClearEyeQ.Predictive.Application.Queries.GetLatestForecast;

public sealed record ForecastDto(
    Guid PredictionId,
    Guid UserId,
    DateTimeOffset GeneratedAt,
    string Status,
    List<ForecastDayDto> Days,
    FlareUpAlertDto? FlareUpAlert);

public sealed record ForecastDayDto(
    string Date,
    double PredictedScore,
    string RiskLevel,
    string PrimaryFactor,
    double ConfidenceLower,
    double ConfidenceUpper);

public sealed record FlareUpAlertDto(
    double Probability,
    string Level,
    List<string> TriggerFactors,
    List<string> PreventiveActions);
