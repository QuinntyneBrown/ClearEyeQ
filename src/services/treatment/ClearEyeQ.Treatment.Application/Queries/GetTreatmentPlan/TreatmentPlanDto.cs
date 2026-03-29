using ClearEyeQ.Treatment.Domain.Enums;

namespace ClearEyeQ.Treatment.Application.Queries.GetTreatmentPlan;

public sealed record TreatmentPlanDto(
    Guid PlanId,
    Guid UserId,
    Guid TenantId,
    Guid DiagnosisId,
    TreatmentStatus Status,
    DateTimeOffset? ActivatedAt,
    string? RejectionReason,
    int PhaseCount,
    int EfficacyMeasurementCount,
    double? LatestDeltaPercent,
    EscalationRuleDto? EscalationRule);

public sealed record EscalationRuleDto(
    int DaysThreshold,
    double MinImprovementPercent,
    string Action);
