namespace ClearEyeQ.Environmental.Domain.ValueObjects;

public sealed record UvIndex(double Value, string RiskCategory)
{
    public static string ClassifyRisk(double uvValue) => uvValue switch
    {
        < 3 => "Low",
        < 6 => "Moderate",
        < 8 => "High",
        < 11 => "Very High",
        _ => "Extreme"
    };
}
