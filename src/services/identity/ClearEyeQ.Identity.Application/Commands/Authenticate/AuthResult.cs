namespace ClearEyeQ.Identity.Application.Commands.Authenticate;

public sealed record AuthResult(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt);
