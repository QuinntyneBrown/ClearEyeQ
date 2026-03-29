using ClearEyeQ.Notifications.Domain.Enums;
using ClearEyeQ.Notifications.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Notifications.Domain.Entities;

public sealed class NotificationPreference
{
    public Guid PreferenceId { get; private set; } = Guid.NewGuid();
    public UserId UserId { get; private set; }
    public TenantId TenantId { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public bool Enabled { get; private set; }
    public QuietHoursPolicy? QuietHoursPolicy { get; private set; }

    private NotificationPreference() { }

    public static NotificationPreference Create(
        UserId userId,
        TenantId tenantId,
        NotificationChannel channel,
        bool enabled = true,
        QuietHoursPolicy? quietHoursPolicy = null)
    {
        return new NotificationPreference
        {
            UserId = userId,
            TenantId = tenantId,
            Channel = channel,
            Enabled = enabled,
            QuietHoursPolicy = quietHoursPolicy
        };
    }

    public void Enable()
    {
        Enabled = true;
    }

    public void Disable()
    {
        Enabled = false;
    }

    public void SetQuietHours(QuietHoursPolicy policy)
    {
        QuietHoursPolicy = policy;
    }

    public void ClearQuietHours()
    {
        QuietHoursPolicy = null;
    }

    public bool ShouldDeliver(DateTimeOffset utcNow)
    {
        if (!Enabled)
            return false;

        if (QuietHoursPolicy is not null && QuietHoursPolicy.IsInQuietHours(utcNow))
            return false;

        return true;
    }
}
