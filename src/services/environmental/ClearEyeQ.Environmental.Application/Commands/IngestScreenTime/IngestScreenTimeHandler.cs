using ClearEyeQ.Environmental.Application.Interfaces;
using ClearEyeQ.Environmental.Domain.Aggregates;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Environmental.Application.Commands.IngestScreenTime;

public sealed class IngestScreenTimeHandler(IEnvironmentalSnapshotRepository repository)
    : IRequestHandler<IngestScreenTimeCommand, Guid>
{
    public async Task<Guid> Handle(IngestScreenTimeCommand request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var tenantId = new TenantId(request.TenantId);

        // Get the latest snapshot to attach screen time, or create a minimal one
        var snapshot = await repository.GetLatestAsync(userId, tenantId, cancellationToken);

        if (snapshot is null || snapshot.ScreenTimeRecord is not null)
        {
            // Create a new snapshot for screen time data
            snapshot = EnvironmentalSnapshot.Create(userId, tenantId);
        }

        snapshot.SetScreenTime(request.TotalDuration, request.AppBreakdown);
        await repository.AddAsync(snapshot, cancellationToken);

        return snapshot.SnapshotId;
    }
}
