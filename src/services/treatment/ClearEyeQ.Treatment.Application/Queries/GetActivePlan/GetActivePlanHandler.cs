using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using ClearEyeQ.Treatment.Application.Interfaces;
using ClearEyeQ.Treatment.Application.Queries.GetTreatmentPlan;
using MediatR;

namespace ClearEyeQ.Treatment.Application.Queries.GetActivePlan;

public sealed class GetActivePlanHandler : IRequestHandler<GetActivePlanQuery, TreatmentPlanDto?>
{
    private readonly ITreatmentPlanRepository _repository;

    public GetActivePlanHandler(ITreatmentPlanRepository repository)
    {
        _repository = repository;
    }

    public async Task<TreatmentPlanDto?> Handle(GetActivePlanQuery request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var tenantId = new TenantId(request.TenantId);

        var plan = await _repository.GetActivePlanAsync(userId, tenantId, cancellationToken);

        if (plan is null)
            return null;

        var latestMeasurement = plan.EfficacyMeasurements
            .OrderByDescending(m => m.MeasuredAt)
            .FirstOrDefault();

        EscalationRuleDto? escalationRuleDto = plan.EscalationRule is not null
            ? new EscalationRuleDto(
                plan.EscalationRule.DaysThreshold,
                plan.EscalationRule.MinImprovementPercent,
                plan.EscalationRule.Action)
            : null;

        return new TreatmentPlanDto(
            PlanId: plan.PlanId,
            UserId: plan.UserId.Value,
            TenantId: plan.TenantId.Value,
            DiagnosisId: plan.DiagnosisId,
            Status: plan.Status,
            ActivatedAt: plan.ActivatedAt,
            RejectionReason: plan.RejectionReason,
            PhaseCount: plan.Phases.Count,
            EfficacyMeasurementCount: plan.EfficacyMeasurements.Count,
            LatestDeltaPercent: latestMeasurement?.DeltaPercent,
            EscalationRule: escalationRuleDto);
    }
}
