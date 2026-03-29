using ClearEyeQ.Scan.Domain.Enums;

namespace ClearEyeQ.Scan.Application.Queries.GetScanResult;

public sealed record ScanResultDto(
    Guid ScanId,
    Guid UserId,
    EyeSide EyeSide,
    ScanStatus Status,
    DateTimeOffset CreatedAt,
    double? RednessOverall,
    double? RednessConfidence,
    IReadOnlyDictionary<string, double>? ZoneScores,
    double? TearFilmBreakUpTime,
    string? TearFilmLipidLayerGrade,
    double? TearFilmCoveragePercentage,
    string? FailureReason);
