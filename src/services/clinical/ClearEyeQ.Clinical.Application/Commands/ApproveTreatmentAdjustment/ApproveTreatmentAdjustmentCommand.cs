using MediatR;

namespace ClearEyeQ.Clinical.Application.Commands.ApproveTreatmentAdjustment;

public sealed record ApproveTreatmentAdjustmentCommand(
    Guid TenantId,
    Guid TreatmentReviewId,
    string ClinicianId,
    string Rationale) : IRequest<Unit>;
