using MediatR;

namespace ClearEyeQ.Treatment.Application.Queries.GetTreatmentPlan;

public sealed record GetTreatmentPlanQuery(
    Guid PlanId,
    Guid TenantId) : IRequest<TreatmentPlanDto?>;
