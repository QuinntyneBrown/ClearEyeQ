namespace ClearEyeQ.Treatment.Domain.ValueObjects;

public sealed record EscalationRule(
    int DaysThreshold,
    double MinImprovementPercent,
    string Action);
