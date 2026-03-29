namespace ClearEyeQ.Billing.Domain.ValueObjects;

public sealed record FeatureSet(
    bool PredictiveAccess,
    bool AutonomousTreatment,
    int MaxScansPerMonth,
    bool PrioritySupport)
{
    public static FeatureSet ForFree() => new(
        PredictiveAccess: false,
        AutonomousTreatment: false,
        MaxScansPerMonth: 1,
        PrioritySupport: false);

    public static FeatureSet ForPro() => new(
        PredictiveAccess: false,
        AutonomousTreatment: false,
        MaxScansPerMonth: 10,
        PrioritySupport: false);

    public static FeatureSet ForPremium() => new(
        PredictiveAccess: true,
        AutonomousTreatment: false,
        MaxScansPerMonth: int.MaxValue,
        PrioritySupport: true);

    public static FeatureSet ForAutonomous() => new(
        PredictiveAccess: true,
        AutonomousTreatment: true,
        MaxScansPerMonth: int.MaxValue,
        PrioritySupport: true);
}
