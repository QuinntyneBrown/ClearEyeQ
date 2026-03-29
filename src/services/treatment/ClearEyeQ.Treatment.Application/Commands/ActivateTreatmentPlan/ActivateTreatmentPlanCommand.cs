using MediatR;

namespace ClearEyeQ.Treatment.Application.Commands.ActivateTreatmentPlan;

public sealed record ActivateTreatmentPlanCommand(
    Guid PlanId,
    Guid TenantId) : IRequest;
