using ClearEyeQ.Identity.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Identity.Application.Commands.RevokeConsent;

public sealed class RevokeConsentHandler : IRequestHandler<RevokeConsentCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogger _auditLogger;

    private static readonly TenantId DefaultTenantId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    public RevokeConsentHandler(IUserRepository userRepository, IAuditLogger auditLogger)
    {
        _userRepository = userRepository;
        _auditLogger = auditLogger;
    }

    public async Task Handle(RevokeConsentCommand request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, DefaultTenantId, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException($"User '{request.UserId}' not found.");
        }

        user.RevokeConsent(request.ConsentType);
        await _userRepository.SaveAsync(user, cancellationToken);

        await _auditLogger.LogAsync(
            "ConsentRevoked",
            userId,
            $"Consent revoked: {request.ConsentType}",
            cancellationToken);
    }
}
