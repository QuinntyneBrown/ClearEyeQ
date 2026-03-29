using MediatR;

namespace ClearEyeQ.Treatment.Application.Commands.EvaluateEscalation;

public sealed record EvaluateEscalationCommand(
    Guid PlanId,
    Guid TenantId) : IRequest<bool>;
