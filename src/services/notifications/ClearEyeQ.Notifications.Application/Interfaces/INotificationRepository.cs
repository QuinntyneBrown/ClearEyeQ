using ClearEyeQ.Notifications.Domain.Aggregates;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Notifications.Application.Interfaces;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid notificationId, CancellationToken ct);
    Task<IReadOnlyList<Notification>> GetByUserAsync(UserId userId, TenantId tenantId, int limit, CancellationToken ct);
    Task AddAsync(Notification notification, CancellationToken ct);
    Task UpdateAsync(Notification notification, CancellationToken ct);
}
