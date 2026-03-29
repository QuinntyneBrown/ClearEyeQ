namespace ClearEyeQ.Fhir.Application.Interfaces;

/// <summary>
/// Gathers patient data from upstream bounded contexts for FHIR export.
/// </summary>
public interface IContextDataGatherer
{
    Task<PatientData> GatherAsync(Guid tenantId, Guid patientId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Aggregated patient data from all upstream contexts.
/// </summary>
public sealed record PatientData(
    Guid PatientId,
    string GivenName,
    string FamilyName,
    DateTimeOffset? BirthDate,
    string? Gender,
    IReadOnlyList<ScanData> Scans,
    IReadOnlyList<DiagnosticData> Diagnostics,
    IReadOnlyList<TreatmentData> Treatments);

public sealed record ScanData(
    Guid ScanId,
    string EyeSide,
    double RednessScore,
    double TearFilmStability,
    double ConfidenceScore,
    DateTimeOffset CompletedAtUtc);

public sealed record DiagnosticData(
    Guid DiagnosticSessionId,
    string PrimaryDiagnosis,
    string Severity,
    double ConfidenceScore,
    string CausalFactors,
    DateTimeOffset CompletedAtUtc);

public sealed record TreatmentData(
    Guid TreatmentPlanId,
    string Status,
    string InterventionSummary,
    DateTimeOffset ProposedAtUtc);
