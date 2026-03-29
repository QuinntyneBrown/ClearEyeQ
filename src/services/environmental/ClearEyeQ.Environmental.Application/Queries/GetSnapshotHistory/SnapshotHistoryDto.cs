using ClearEyeQ.Environmental.Application.Queries.GetLatestSnapshot;

namespace ClearEyeQ.Environmental.Application.Queries.GetSnapshotHistory;

public sealed record SnapshotHistoryDto(
    int TotalCount,
    IReadOnlyList<EnvironmentalSnapshotDto> Snapshots);
