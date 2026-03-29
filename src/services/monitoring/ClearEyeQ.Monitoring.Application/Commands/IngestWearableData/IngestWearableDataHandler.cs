using ClearEyeQ.Monitoring.Application.Interfaces;
using ClearEyeQ.Monitoring.Domain.Aggregates;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Monitoring.Application.Commands.IngestWearableData;

public sealed class IngestWearableDataHandler(IMonitoringRepository repository)
    : IRequestHandler<IngestWearableDataCommand, Guid>
{
    public async Task<Guid> Handle(IngestWearableDataCommand request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var tenantId = new TenantId(request.TenantId);

        var session = await repository.GetActiveSessionAsync(userId, tenantId, cancellationToken);

        if (session is null)
        {
            session = MonitoringSession.Create(userId, tenantId);
            session.AddWearableData(request.Source, request.MetricType, request.Value, request.Timestamp);
            await repository.AddAsync(session, cancellationToken);
        }
        else
        {
            session.AddWearableData(request.Source, request.MetricType, request.Value, request.Timestamp);
            await repository.UpdateAsync(session, cancellationToken);
        }

        return session.SessionId;
    }
}
