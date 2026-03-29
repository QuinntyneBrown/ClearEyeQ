using MediatR;

namespace ClearEyeQ.Treatment.Application.Commands.RejectTreatmentPlan;

public sealed record RejectTreatmentPlanCommand(
    Guid PlanId,
    Guid TenantId,
    string Reason) : IRequest;
