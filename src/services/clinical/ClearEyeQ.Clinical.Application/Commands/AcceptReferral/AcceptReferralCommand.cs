using MediatR;

namespace ClearEyeQ.Clinical.Application.Commands.AcceptReferral;

public sealed record AcceptReferralCommand(
    Guid TenantId,
    Guid ReferralId,
    string ClinicianId,
    string Rationale) : IRequest<Unit>;
