using ClearEyeQ.Identity.Domain.Enums;
using MediatR;

namespace ClearEyeQ.Identity.Application.Commands.RevokeConsent;

public sealed record RevokeConsentCommand(
    Guid UserId,
    ConsentType ConsentType) : IRequest;
