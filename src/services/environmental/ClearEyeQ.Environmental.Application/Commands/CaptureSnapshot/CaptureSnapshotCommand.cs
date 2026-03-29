using MediatR;

namespace ClearEyeQ.Environmental.Application.Commands.CaptureSnapshot;

public sealed record CaptureSnapshotCommand(
    Guid UserId,
    Guid TenantId,
    double Latitude,
    double Longitude) : IRequest<Guid>;
