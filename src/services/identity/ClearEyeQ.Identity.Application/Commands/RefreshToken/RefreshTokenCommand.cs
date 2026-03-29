using ClearEyeQ.Identity.Application.Commands.Authenticate;
using MediatR;

namespace ClearEyeQ.Identity.Application.Commands.RefreshToken;

public sealed record RefreshTokenCommand(
    string Token,
    string DeviceFingerprint) : IRequest<AuthResult>;
