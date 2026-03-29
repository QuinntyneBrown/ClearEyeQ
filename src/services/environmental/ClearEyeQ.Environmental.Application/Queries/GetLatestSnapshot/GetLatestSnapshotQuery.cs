using MediatR;

namespace ClearEyeQ.Environmental.Application.Queries.GetLatestSnapshot;

public sealed record GetLatestSnapshotQuery(
    Guid UserId,
    Guid TenantId) : IRequest<EnvironmentalSnapshotDto?>;
