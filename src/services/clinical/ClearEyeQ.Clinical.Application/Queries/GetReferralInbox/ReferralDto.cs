namespace ClearEyeQ.Clinical.Application.Queries.GetReferralInbox;

public sealed record ReferralDto(
    Guid ReferralId,
    Guid PatientId,
    string PatientName,
    string Reason,
    string Severity,
    string Status,
    DateTimeOffset CreatedAtUtc);
