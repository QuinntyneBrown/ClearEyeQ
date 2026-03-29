namespace ClearEyeQ.Scan.Application.Queries.CompareScan;

public sealed record ScanComparisonDto(
    Guid ScanId,
    Guid BaselineScanId,
    double RednessDelta,
    double TearFilmDelta,
    double CurrentRednessOverall,
    double BaselineRednessOverall);
