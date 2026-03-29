using MediatR;

namespace ClearEyeQ.Clinical.Application.Commands.ApproveTreatmentPlan;

public sealed record ApproveTreatmentPlanCommand(
    Guid TenantId,
    Guid TreatmentReviewId,
    string ClinicianId,
    string Rationale) : IRequest<Unit>;
