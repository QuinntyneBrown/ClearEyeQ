using ClearEyeQ.Predictive.Domain.Enums;

namespace ClearEyeQ.Predictive.Domain.Entities;

public sealed class ForecastDay
{
    public DateOnly Date { get; private set; }
    public double PredictedScore { get; private set; }
    public RiskLevel Risk { get; private set; }
    public string PrimaryFactor { get; private set; }
    public double ConfidenceLower { get; private set; }
    public double ConfidenceUpper { get; private set; }

    private ForecastDay() { PrimaryFactor = string.Empty; }

    public ForecastDay(
        DateOnly date,
        double predictedScore,
        RiskLevel risk,
        string primaryFactor,
        double confidenceLower,
        double confidenceUpper)
    {
        Date = date;
        PredictedScore = predictedScore;
        Risk = risk;
        PrimaryFactor = primaryFactor;
        ConfidenceLower = confidenceLower;
        ConfidenceUpper = confidenceUpper;
    }
}
