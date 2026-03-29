using MediatR;

namespace ClearEyeQ.Predictive.Application.Queries.GetTrajectory;

public sealed record GetTrajectoryQuery(Guid UserId, Guid TenantId) : IRequest<TrajectoryDto?>;
