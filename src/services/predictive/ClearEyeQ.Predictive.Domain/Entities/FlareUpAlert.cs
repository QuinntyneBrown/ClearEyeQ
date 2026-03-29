using ClearEyeQ.Predictive.Domain.Enums;

namespace ClearEyeQ.Predictive.Domain.Entities;

public sealed class FlareUpAlert
{
    public double Probability { get; private set; }
    public List<string> TriggerFactors { get; private set; }
    public List<string> PreventiveActions { get; private set; }
    public RiskLevel Level { get; private set; }

    private FlareUpAlert()
    {
        TriggerFactors = [];
        PreventiveActions = [];
    }

    public FlareUpAlert(
        double probability,
        List<string> triggerFactors,
        List<string> preventiveActions,
        RiskLevel level)
    {
        if (probability < 0.0 || probability > 1.0)
            throw new ArgumentOutOfRangeException(nameof(probability), "Probability must be between 0.0 and 1.0.");

        Probability = probability;
        TriggerFactors = triggerFactors ?? [];
        PreventiveActions = preventiveActions ?? [];
        Level = level;
    }
}
