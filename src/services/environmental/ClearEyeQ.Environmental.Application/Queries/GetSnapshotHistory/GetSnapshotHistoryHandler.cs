using ClearEyeQ.Environmental.Application.Interfaces;
using ClearEyeQ.Environmental.Application.Queries.GetLatestSnapshot;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Environmental.Application.Queries.GetSnapshotHistory;

public sealed class GetSnapshotHistoryHandler(IEnvironmentalSnapshotRepository repository)
    : IRequestHandler<GetSnapshotHistoryQuery, SnapshotHistoryDto>
{
    public async Task<SnapshotHistoryDto> Handle(
        GetSnapshotHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var tenantId = new TenantId(request.TenantId);

        var snapshots = await repository.GetHistoryAsync(
            userId, tenantId, request.From, request.To, cancellationToken);

        var dtos = snapshots
            .Select(GetLatestSnapshotHandler.MapToDto)
            .ToList()
            .AsReadOnly();

        return new SnapshotHistoryDto(dtos.Count, dtos);
    }
}
