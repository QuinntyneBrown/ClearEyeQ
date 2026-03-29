using ClearEyeQ.Identity.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Identity.Application.Commands.GrantConsent;

public sealed class GrantConsentHandler : IRequestHandler<GrantConsentCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogger _auditLogger;

    private static readonly TenantId DefaultTenantId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    public GrantConsentHandler(IUserRepository userRepository, IAuditLogger auditLogger)
    {
        _userRepository = userRepository;
        _auditLogger = auditLogger;
    }

    public async Task Handle(GrantConsentCommand request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, DefaultTenantId, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException($"User '{request.UserId}' not found.");
        }

        user.AddConsent(request.ConsentType);
        await _userRepository.SaveAsync(user, cancellationToken);

        await _auditLogger.LogAsync(
            "ConsentGranted",
            userId,
            $"Consent granted: {request.ConsentType}",
            cancellationToken);
    }
}
