namespace ClearEyeQ.Treatment.Domain.Entities;

public sealed class TreatmentPhase
{
    private readonly List<Intervention> _interventions = [];

    public Guid PhaseId { get; private set; } = Guid.NewGuid();
    public int PhaseNumber { get; private set; }
    public TimeSpan Duration { get; private set; }
    public IReadOnlyList<Intervention> Interventions => _interventions.AsReadOnly();

    private TreatmentPhase() { }

    public static TreatmentPhase Create(int phaseNumber, TimeSpan duration)
    {
        if (phaseNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(phaseNumber), "Phase number must be at least 1.");

        return new TreatmentPhase
        {
            PhaseNumber = phaseNumber,
            Duration = duration
        };
    }

    public void AddIntervention(Intervention intervention)
    {
        ArgumentNullException.ThrowIfNull(intervention);
        _interventions.Add(intervention);
    }

    public void RemoveIntervention(Guid interventionId)
    {
        var intervention = _interventions.FirstOrDefault(i => i.InterventionId == interventionId);
        if (intervention is not null)
        {
            _interventions.Remove(intervention);
        }
    }
}
