using ClearEyeQ.Identity.Domain.Enums;

namespace ClearEyeQ.Identity.Domain.Entities;

public sealed class Consent
{
    public Guid ConsentId { get; private set; } = Guid.NewGuid();
    public ConsentType ConsentType { get; private set; }
    public DateTimeOffset GrantedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public bool IsActive => RevokedAt is null;

    private Consent() { }

    public static Consent Grant(ConsentType consentType)
    {
        return new Consent
        {
            ConsentId = Guid.NewGuid(),
            ConsentType = consentType,
            GrantedAt = DateTimeOffset.UtcNow
        };
    }

    public void Revoke()
    {
        if (RevokedAt is not null)
        {
            throw new InvalidOperationException("Consent has already been revoked.");
        }

        RevokedAt = DateTimeOffset.UtcNow;
    }
}
