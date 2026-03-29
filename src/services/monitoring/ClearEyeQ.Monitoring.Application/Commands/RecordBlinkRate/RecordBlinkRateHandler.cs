using ClearEyeQ.Monitoring.Application.Interfaces;
using ClearEyeQ.Monitoring.Domain.Aggregates;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Monitoring.Application.Commands.RecordBlinkRate;

public sealed class RecordBlinkRateHandler(IMonitoringRepository repository)
    : IRequestHandler<RecordBlinkRateCommand, Guid>
{
    public async Task<Guid> Handle(RecordBlinkRateCommand request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var tenantId = new TenantId(request.TenantId);

        var session = await repository.GetActiveSessionAsync(userId, tenantId, cancellationToken);

        if (session is null)
        {
            session = MonitoringSession.Create(userId, tenantId);
            session.RecordBlinkRate(request.BlinksPerMinute, request.FatigueScore, request.MeasuredAt);
            await repository.AddAsync(session, cancellationToken);
        }
        else
        {
            session.RecordBlinkRate(request.BlinksPerMinute, request.FatigueScore, request.MeasuredAt);
            await repository.UpdateAsync(session, cancellationToken);
        }

        return session.SessionId;
    }
}
