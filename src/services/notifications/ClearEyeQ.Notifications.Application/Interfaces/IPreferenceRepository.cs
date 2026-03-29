using ClearEyeQ.Notifications.Domain.Entities;
using ClearEyeQ.Notifications.Domain.Enums;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Notifications.Application.Interfaces;

public interface IPreferenceRepository
{
    Task<IReadOnlyList<NotificationPreference>> GetByUserAsync(UserId userId, TenantId tenantId, CancellationToken ct);
    Task<NotificationPreference?> GetAsync(UserId userId, TenantId tenantId, NotificationChannel channel, CancellationToken ct);
    Task SaveAsync(NotificationPreference preference, CancellationToken ct);
}
