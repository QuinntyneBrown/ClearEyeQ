using ClearEyeQ.Environmental.Application.Queries.GetLatestSnapshot;
using MediatR;

namespace ClearEyeQ.Environmental.Application.Queries.GetSnapshotHistory;

public sealed record GetSnapshotHistoryQuery(
    Guid UserId,
    Guid TenantId,
    DateTimeOffset From,
    DateTimeOffset To) : IRequest<SnapshotHistoryDto>;
