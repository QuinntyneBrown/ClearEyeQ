using ClearEyeQ.Identity.Domain.Entities;
using ClearEyeQ.Identity.Domain.Enums;
using ClearEyeQ.Identity.Domain.Events;
using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Identity.Domain.Aggregates;

public sealed class User : AggregateRoot
{
    private readonly List<Consent> _consents = [];
    private readonly List<RefreshToken> _refreshTokens = [];

    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string Salt { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public Role Role { get; private set; }
    public AccountStatus AccountStatus { get; private set; }
    public IReadOnlyList<Consent> Consents => _consents.AsReadOnly();
    public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
    public bool MfaEnabled { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTimeOffset? LockedUntil { get; private set; }

    private TenantId _tenantId;
    public override TenantId TenantId => _tenantId;
    public override PartitionKey PartitionKey => PartitionKey.ForTenant(_tenantId);

    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    private User() { }

    public static User Register(
        string email,
        string passwordHash,
        string salt,
        string displayName,
        Role role,
        TenantId tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(salt);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Salt = salt,
            DisplayName = displayName,
            Role = role,
            AccountStatus = AccountStatus.PendingVerification,
            _tenantId = tenantId,
            Audit = AuditMetadata.Create(email)
        };

        user.AddDomainEvent(new UserRegisteredEvent
        {
            UserId = new UserId(user.Id),
            Email = user.Email,
            TenantId = tenantId,
            Role = role
        });

        return user;
    }

    public bool Authenticate(string passwordHash, string deviceFingerprint)
    {
        if (IsLockedOut())
        {
            return false;
        }

        if (AccountStatus is AccountStatus.Suspended or AccountStatus.Deleted)
        {
            return false;
        }

        if (PasswordHash != passwordHash)
        {
            RecordFailedLogin();
            return false;
        }

        FailedLoginAttempts = 0;
        LockedUntil = null;

        AddDomainEvent(new UserAuthenticatedEvent
        {
            UserId = new UserId(Id),
            DeviceFingerprint = deviceFingerprint
        });

        return true;
    }

    public Consent AddConsent(ConsentType consentType)
    {
        var existing = _consents.FirstOrDefault(c => c.ConsentType == consentType && c.IsActive);
        if (existing is not null)
        {
            throw new InvalidOperationException($"Active consent of type {consentType} already exists.");
        }

        var consent = Consent.Grant(consentType);
        _consents.Add(consent);

        AddDomainEvent(new ConsentGrantedEvent
        {
            UserId = new UserId(Id),
            ConsentType = consentType
        });

        return consent;
    }

    public void RevokeConsent(ConsentType consentType)
    {
        var consent = _consents.FirstOrDefault(c => c.ConsentType == consentType && c.IsActive);
        if (consent is null)
        {
            throw new InvalidOperationException($"No active consent of type {consentType} found.");
        }

        consent.Revoke();

        AddDomainEvent(new ConsentRevokedEvent
        {
            UserId = new UserId(Id),
            ConsentType = consentType
        });
    }

    public RefreshToken IssueRefreshToken(string tokenHash, string deviceFingerprint)
    {
        // Revoke any existing tokens for the same device
        foreach (var existing in _refreshTokens.Where(t => t.DeviceFingerprint == deviceFingerprint && t.IsValid))
        {
            existing.Revoke();
        }

        var token = RefreshToken.Create(tokenHash, deviceFingerprint, RefreshTokenLifetime);
        _refreshTokens.Add(token);
        return token;
    }

    public void RevokeRefreshToken(string tokenHash)
    {
        var token = _refreshTokens.FirstOrDefault(t => t.TokenHash == tokenHash);
        if (token is null)
        {
            throw new InvalidOperationException("Refresh token not found.");
        }

        token.Revoke();
    }

    public void EnableMfa()
    {
        MfaEnabled = true;
    }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= MaxFailedAttempts)
        {
            LockedUntil = DateTimeOffset.UtcNow.Add(LockoutDuration);
        }
    }

    public void Unlock()
    {
        FailedLoginAttempts = 0;
        LockedUntil = null;
    }

    public bool IsLockedOut()
    {
        return LockedUntil.HasValue && DateTimeOffset.UtcNow < LockedUntil.Value;
    }
}
