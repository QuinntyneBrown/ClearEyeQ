using ClearEyeQ.Treatment.Application.Queries.GetTreatmentPlan;
using MediatR;

namespace ClearEyeQ.Treatment.Application.Queries.GetActivePlan;

public sealed record GetActivePlanQuery(
    Guid UserId,
    Guid TenantId) : IRequest<TreatmentPlanDto?>;
