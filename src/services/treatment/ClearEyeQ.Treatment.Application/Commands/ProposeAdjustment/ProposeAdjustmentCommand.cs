using MediatR;

namespace ClearEyeQ.Treatment.Application.Commands.ProposeAdjustment;

public sealed record ProposeAdjustmentCommand(
    Guid PlanId,
    Guid TenantId,
    string Reason) : IRequest;
