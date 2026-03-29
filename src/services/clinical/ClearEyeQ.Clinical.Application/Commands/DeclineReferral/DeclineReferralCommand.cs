using MediatR;

namespace ClearEyeQ.Clinical.Application.Commands.DeclineReferral;

public sealed record DeclineReferralCommand(
    Guid TenantId,
    Guid ReferralId,
    string ClinicianId,
    string Rationale) : IRequest<Unit>;
