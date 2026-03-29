using ClearEyeQ.Identity.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Identity.Application.Commands.Authenticate;

public sealed class AuthenticateHandler : IRequestHandler<AuthenticateCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenProvider _tokenProvider;
    private readonly IAuditLogger _auditLogger;

    // Default tenant for simplified auth flow. In production, resolved from request context.
    private static readonly TenantId DefaultTenantId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    public AuthenticateHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenProvider tokenProvider,
        IAuditLogger auditLogger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenProvider = tokenProvider;
        _auditLogger = auditLogger;
    }

    public async Task<AuthResult> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, DefaultTenantId, cancellationToken);
        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var passwordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);

        if (!user.Authenticate(passwordValid ? user.PasswordHash : "invalid", request.DeviceFingerprint))
        {
            await _userRepository.SaveAsync(user, cancellationToken);

            await _auditLogger.LogAsync(
                "AuthenticationFailed",
                new UserId(user.Id),
                $"Failed login attempt from device {request.DeviceFingerprint}",
                cancellationToken);

            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var accessToken = _tokenProvider.GenerateAccessToken(user);
        var refreshTokenRaw = _tokenProvider.GenerateRefreshToken();
        var refreshTokenHash = _tokenProvider.HashToken(refreshTokenRaw);

        user.IssueRefreshToken(refreshTokenHash, request.DeviceFingerprint);

        await _userRepository.SaveAsync(user, cancellationToken);

        await _auditLogger.LogAsync(
            "UserAuthenticated",
            new UserId(user.Id),
            $"Successful login from device {request.DeviceFingerprint}",
            cancellationToken);

        return new AuthResult(
            AccessToken: accessToken,
            RefreshToken: refreshTokenRaw,
            ExpiresAt: DateTimeOffset.UtcNow.AddMinutes(15));
    }
}
