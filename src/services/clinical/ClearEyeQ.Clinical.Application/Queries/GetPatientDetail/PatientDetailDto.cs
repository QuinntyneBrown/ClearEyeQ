namespace ClearEyeQ.Clinical.Application.Queries.GetPatientDetail;

public sealed record PatientDetailDto(
    Guid PatientId,
    string Name,
    string Status,
    IReadOnlyList<ScanDto> Scans,
    IReadOnlyList<DiagnosisDto> Diagnoses,
    IReadOnlyList<TreatmentDto> Treatments);

public sealed record ScanDto(
    Guid ScanId,
    string EyeSide,
    double RednessScore,
    double TearFilmStability,
    double ConfidenceScore,
    DateTimeOffset CompletedAtUtc);

public sealed record DiagnosisDto(
    Guid DiagnosticSessionId,
    string PrimaryDiagnosis,
    string Severity,
    double ConfidenceScore,
    DateTimeOffset CompletedAtUtc);

public sealed record TreatmentDto(
    Guid TreatmentPlanId,
    string Status,
    string InterventionSummary,
    DateTimeOffset ProposedAtUtc,
    DateTimeOffset? ActivatedAtUtc);
