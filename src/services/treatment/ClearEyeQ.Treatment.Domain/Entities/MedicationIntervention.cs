using ClearEyeQ.Treatment.Domain.Enums;

namespace ClearEyeQ.Treatment.Domain.Entities;

public sealed class MedicationIntervention : Intervention
{
    public string DrugName { get; private set; } = default!;
    public string Dosage { get; private set; } = default!;
    public string Frequency { get; private set; } = default!;
    public string Route { get; private set; } = default!;

    private MedicationIntervention() { }

    public static MedicationIntervention Create(
        string drugName,
        string dosage,
        string frequency,
        string route,
        string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(drugName);
        ArgumentException.ThrowIfNullOrWhiteSpace(dosage);

        return new MedicationIntervention
        {
            InterventionType = InterventionType.Medication,
            DrugName = drugName,
            Dosage = dosage,
            Frequency = frequency,
            Route = route,
            Description = description
        };
    }
}
