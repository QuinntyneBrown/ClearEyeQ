namespace ClearEyeQ.Identity.Domain.Entities;

public sealed class RefreshToken
{
    public string TokenHash { get; private set; } = default!;
    public string DeviceFingerprint { get; private set; } = default!;
    public DateTimeOffset IssuedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }

    private RefreshToken() { }

    public static RefreshToken Create(string tokenHash, string deviceFingerprint, TimeSpan lifetime)
    {
        var now = DateTimeOffset.UtcNow;
        return new RefreshToken
        {
            TokenHash = tokenHash,
            DeviceFingerprint = deviceFingerprint,
            IssuedAt = now,
            ExpiresAt = now.Add(lifetime)
        };
    }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;

    public bool IsValid => !IsRevoked && !IsExpired;

    public void Revoke()
    {
        IsRevoked = true;
    }
}
