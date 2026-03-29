using ClearEyeQ.Identity.Domain.Enums;
using MediatR;

namespace ClearEyeQ.Identity.Application.Commands.GrantConsent;

public sealed record GrantConsentCommand(
    Guid UserId,
    ConsentType ConsentType) : IRequest;
