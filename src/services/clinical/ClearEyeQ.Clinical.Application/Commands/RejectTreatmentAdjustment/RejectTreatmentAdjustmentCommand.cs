using MediatR;

namespace ClearEyeQ.Clinical.Application.Commands.RejectTreatmentAdjustment;

public sealed record RejectTreatmentAdjustmentCommand(
    Guid TenantId,
    Guid TreatmentReviewId,
    string ClinicianId,
    string Rationale) : IRequest<Unit>;
