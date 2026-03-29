namespace ClearEyeQ.Clinical.Application.Queries.GetPatientList;

public sealed record PatientSummaryDto(
    Guid PatientId,
    string Name,
    DateTimeOffset? LastScanDate,
    double? RednessScore,
    string Status);
