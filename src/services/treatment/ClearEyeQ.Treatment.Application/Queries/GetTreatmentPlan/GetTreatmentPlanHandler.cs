using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using ClearEyeQ.Treatment.Application.Interfaces;
using MediatR;

namespace ClearEyeQ.Treatment.Application.Queries.GetTreatmentPlan;

public sealed class GetTreatmentPlanHandler : IRequestHandler<GetTreatmentPlanQuery, TreatmentPlanDto?>
{
    private readonly ITreatmentPlanRepository _repository;

    public GetTreatmentPlanHandler(ITreatmentPlanRepository repository)
    {
        _repository = repository;
    }

    public async Task<TreatmentPlanDto?> Handle(GetTreatmentPlanQuery request, CancellationToken cancellationToken)
    {
        var tenantId = new TenantId(request.TenantId);
        var plan = await _repository.GetByIdAsync(request.PlanId, tenantId, cancellationToken);

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
