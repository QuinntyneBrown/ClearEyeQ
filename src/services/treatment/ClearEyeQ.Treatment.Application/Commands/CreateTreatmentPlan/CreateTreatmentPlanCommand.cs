using MediatR;

namespace ClearEyeQ.Treatment.Application.Commands.CreateTreatmentPlan;

public sealed record CreateTreatmentPlanCommand(
    Guid UserId,
    Guid TenantId,
    Guid DiagnosisId,
    int? EscalationDaysThreshold,
    double? EscalationMinImprovementPercent,
    string? EscalationAction) : IRequest<Guid>;
