using MediatR;

namespace ClearEyeQ.Monitoring.Application.Commands.RecordBlinkRate;

public sealed record RecordBlinkRateCommand(
    Guid UserId,
    Guid TenantId,
    double BlinksPerMinute,
    double FatigueScore,
    DateTimeOffset MeasuredAt) : IRequest<Guid>;
