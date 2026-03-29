namespace ClearEyeQ.Clinical.Application.ReadModels;

/// <summary>
/// Flat projection for the patient list view, aggregated from upstream bounded contexts.
/// </summary>
public sealed class PatientSummaryReadModel
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset? LastScanDate { get; set; }
    public double? RednessScore { get; set; }
    public string Status { get; set; } = "Active";
    public string? LatestDiagnosis { get; set; }
    public string? ActiveTreatment { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
