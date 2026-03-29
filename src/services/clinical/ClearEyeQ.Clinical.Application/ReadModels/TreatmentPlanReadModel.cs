namespace ClearEyeQ.Clinical.Application.ReadModels;

/// <summary>
/// Projected read model for treatment plans, sourced from the Treatment bounded context.
/// </summary>
public sealed class TreatmentPlanReadModel
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PatientId { get; set; }
    public Guid TreatmentPlanId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string InterventionSummary { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public DateTimeOffset ProposedAtUtc { get; set; }
    public DateTimeOffset? ActivatedAtUtc { get; set; }
}
