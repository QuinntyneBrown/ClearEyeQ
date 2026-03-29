using ClearEyeQ.Monitoring.Application.Interfaces;
using ClearEyeQ.Monitoring.Domain.Aggregates;
using ClearEyeQ.Monitoring.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Monitoring.Application.Commands.RecordSleep;

public sealed class RecordSleepHandler(
    IMonitoringRepository repository,
    ISleepScorer sleepScorer)
    : IRequestHandler<RecordSleepCommand, Guid>
{
    public async Task<Guid> Handle(RecordSleepCommand request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var tenantId = new TenantId(request.TenantId);

        var stages = new SleepStages(request.DeepSleep, request.LightSleep, request.RemSleep, request.AwakeTime);
        var qualityScore = sleepScorer.CalculateQualityScore(request.Duration, stages);

        var session = await repository.GetActiveSessionAsync(userId, tenantId, cancellationToken);

        if (session is null)
        {
            session = MonitoringSession.Create(userId, tenantId);
            session.RecordSleep(request.Date, request.Duration, stages, qualityScore);
            await repository.AddAsync(session, cancellationToken);
        }
        else
        {
            session.RecordSleep(request.Date, request.Duration, stages, qualityScore);
            await repository.UpdateAsync(session, cancellationToken);
        }

        return session.SessionId;
    }
}
