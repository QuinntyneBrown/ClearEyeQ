using MediatR;

namespace ClearEyeQ.Environmental.Application.Commands.IngestScreenTime;

public sealed record IngestScreenTimeCommand(
    Guid UserId,
    Guid TenantId,
    TimeSpan TotalDuration,
    Dictionary<string, TimeSpan> AppBreakdown) : IRequest<Guid>;
