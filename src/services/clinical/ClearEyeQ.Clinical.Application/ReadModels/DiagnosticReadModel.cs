namespace ClearEyeQ.Clinical.Application.ReadModels;

/// <summary>
/// Projected read model for diagnostic results, sourced from the Diagnostic bounded context.
/// </summary>
public sealed class DiagnosticReadModel
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DiagnosticSessionId { get; set; }
    public string PrimaryDiagnosis { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public string CausalFactors { get; set; } = string.Empty;
    public DateTimeOffset CompletedAtUtc { get; set; }
}
