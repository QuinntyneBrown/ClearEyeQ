using ClearEyeQ.Treatment.Domain.Enums;

namespace ClearEyeQ.Treatment.Domain.Entities;

public sealed class EnvironmentalIntervention : Intervention
{
    public EnvironmentalTarget Target { get; private set; }
    public string Recommendation { get; private set; } = default!;

    private EnvironmentalIntervention() { }

    public static EnvironmentalIntervention Create(
        EnvironmentalTarget target,
        string recommendation,
        string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recommendation);

        return new EnvironmentalIntervention
        {
            InterventionType = InterventionType.Environmental,
            Target = target,
            Recommendation = recommendation,
            Description = description
        };
    }
}
