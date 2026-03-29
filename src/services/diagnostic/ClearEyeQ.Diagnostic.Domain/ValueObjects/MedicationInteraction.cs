using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Diagnostic.Domain.ValueObjects;

public sealed record MedicationInteraction(
    string Medication,
    string InteractionType,
    Severity Severity,
    string Description);
