using MediatR;

namespace ClearEyeQ.Clinical.Application.Commands.RejectTreatmentPlan;

public sealed record RejectTreatmentPlanCommand(
    Guid TenantId,
    Guid TreatmentReviewId,
    string ClinicianId,
    string Rationale) : IRequest<Unit>;
