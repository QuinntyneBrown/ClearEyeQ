using ClearEyeQ.Treatment.Domain.Enums;

namespace ClearEyeQ.Treatment.Domain.Entities;

public sealed class BehavioralIntervention : Intervention
{
    public BehavioralType BehavioralType { get; private set; }
    public string Schedule { get; private set; } = default!;

    private BehavioralIntervention() { }

    public static BehavioralIntervention Create(
        BehavioralType behavioralType,
        string schedule,
        string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schedule);

        return new BehavioralIntervention
        {
            InterventionType = InterventionType.Behavioral,
            BehavioralType = behavioralType,
            Schedule = schedule,
            Description = description
        };
    }
}
