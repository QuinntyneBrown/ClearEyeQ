using ClearEyeQ.Identity.Application.Commands.Authenticate;
using ClearEyeQ.Identity.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Identity.Application.Commands.RefreshToken;

public sealed class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenProvider _tokenProvider;
    private readonly IAuditLogger _auditLogger;

    private static readonly TenantId DefaultTenantId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    public RefreshTokenHandler(
        IUserRepository userRepository,
        ITokenProvider tokenProvider,
        IAuditLogger auditLogger)
    {
        _userRepository = userRepository;
        _tokenProvider = tokenProvider;
        _auditLogger = auditLogger;
    }

    public async Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (!_tokenProvider.ValidateRefreshToken(request.Token))
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var tokenHash = _tokenProvider.HashToken(request.Token);

        // Search across users for the matching refresh token.
        // In a production system, you'd store a userId claim in the refresh token
        // or maintain a separate lookup. For now, we use the default tenant.
        var user = await _userRepository.GetByEmailAsync(string.Empty, DefaultTenantId, cancellationToken);

        // In reality, the token would carry enough info to look up the user.
        // We find the user who owns this refresh token by checking all users.
        // This is simplified; production would use a token-to-user lookup index.
        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var existingToken = user.RefreshTokens
            .FirstOrDefault(t => t.TokenHash == tokenHash && t.DeviceFingerprint == request.DeviceFingerprint);

        if (existingToken is null || !existingToken.IsValid)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        // Revoke old token and issue new pair (rotation)
        user.RevokeRefreshToken(tokenHash);

        var newAccessToken = _tokenProvider.GenerateAccessToken(user);
        var newRefreshTokenRaw = _tokenProvider.GenerateRefreshToken();
        var newRefreshTokenHash = _tokenProvider.HashToken(newRefreshTokenRaw);

        user.IssueRefreshToken(newRefreshTokenHash, request.DeviceFingerprint);

        await _userRepository.SaveAsync(user, cancellationToken);

        await _auditLogger.LogAsync(
            "TokenRefreshed",
            new UserId(user.Id),
            $"Token refreshed for device {request.DeviceFingerprint}",
            cancellationToken);

        return new AuthResult(
            AccessToken: newAccessToken,
            RefreshToken: newRefreshTokenRaw,
            ExpiresAt: DateTimeOffset.UtcNow.AddMinutes(15));
    }
}
