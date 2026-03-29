using MediatR;

namespace ClearEyeQ.Monitoring.Application.Commands.RecordSleep;

public sealed record RecordSleepCommand(
    Guid UserId,
    Guid TenantId,
    DateOnly Date,
    TimeSpan Duration,
    TimeSpan DeepSleep,
    TimeSpan LightSleep,
    TimeSpan RemSleep,
    TimeSpan AwakeTime) : IRequest<Guid>;
