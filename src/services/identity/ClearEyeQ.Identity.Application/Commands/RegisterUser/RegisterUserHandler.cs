using ClearEyeQ.Identity.Application.Interfaces;
using ClearEyeQ.Identity.Domain.Aggregates;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Identity.Application.Commands.RegisterUser;

public sealed class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditLogger _auditLogger;

    public RegisterUserHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAuditLogger auditLogger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _auditLogger = auditLogger;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // For now, use a default tenant. In production, tenant resolution would happen upstream.
        var tenantId = TenantId.New();

        var exists = await _userRepository.ExistsAsync(request.Email, tenantId, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"A user with email '{request.Email}' already exists.");
        }

        var (hash, salt) = _passwordHasher.HashPassword(request.Password);

        var user = User.Register(
            email: request.Email,
            passwordHash: hash,
            salt: salt,
            displayName: request.DisplayName,
            role: request.Role,
            tenantId: tenantId);

        await _userRepository.SaveAsync(user, cancellationToken);

        await _auditLogger.LogAsync(
            "UserRegistered",
            new UserId(user.Id),
            $"User registered with email {request.Email}",
            cancellationToken);

        return user.Id;
    }
}
