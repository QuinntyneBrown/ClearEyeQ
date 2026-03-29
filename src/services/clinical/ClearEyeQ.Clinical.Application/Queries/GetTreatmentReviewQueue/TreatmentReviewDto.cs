namespace ClearEyeQ.Clinical.Application.Queries.GetTreatmentReviewQueue;

public sealed record TreatmentReviewDto(
    Guid ReviewId,
    Guid PatientId,
    string PatientName,
    string ReviewType,
    string InterventionSummary,
    string Rationale,
    string Status,
    DateTimeOffset ProposedAtUtc);
