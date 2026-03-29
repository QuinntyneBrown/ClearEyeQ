using MediatR;

namespace ClearEyeQ.Identity.Application.Commands.Authenticate;

public sealed record AuthenticateCommand(
    string Email,
    string Password,
    string DeviceFingerprint) : IRequest<AuthResult>;
