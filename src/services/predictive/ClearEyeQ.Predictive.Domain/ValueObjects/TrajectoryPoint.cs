namespace ClearEyeQ.Predictive.Domain.ValueObjects;

public sealed record TrajectoryPoint(
    DateOnly Date,
    double ProjectedScore,
    double ConfidenceLower,
    double ConfidenceUpper);
