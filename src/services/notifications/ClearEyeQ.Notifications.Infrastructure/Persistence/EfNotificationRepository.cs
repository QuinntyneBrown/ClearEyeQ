using ClearEyeQ.Notifications.Application.Interfaces;
using ClearEyeQ.Notifications.Domain.Aggregates;
using ClearEyeQ.Notifications.Domain.Entities;
using ClearEyeQ.Notifications.Domain.Enums;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Notifications.Infrastructure.Persistence;

public sealed class EfNotificationRepository : INotificationRepository, IPreferenceRepository
{
    private readonly NotificationDbContext _context;
    private readonly ILogger<EfNotificationRepository> _logger;

    public EfNotificationRepository(
        NotificationDbContext context,
        ILogger<EfNotificationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Notification?> GetByIdAsync(Guid notificationId, CancellationToken ct)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId, ct);
    }

    public async Task<IReadOnlyList<Notification>> GetByUserAsync(
        UserId userId, TenantId tenantId, int limit, CancellationToken ct)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && n.TenantId == tenantId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Notification notification, CancellationToken ct)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Created notification {NotificationId} for user {UserId}",
            notification.NotificationId, notification.UserId);
    }

    public async Task UpdateAsync(Notification notification, CancellationToken ct)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<NotificationPreference>> GetByUserAsync(
        UserId userId, TenantId tenantId, CancellationToken ct)
    {
        return await _context.Preferences
            .Where(p => p.UserId == userId && p.TenantId == tenantId)
            .ToListAsync(ct);
    }

    public async Task<NotificationPreference?> GetAsync(
        UserId userId, TenantId tenantId, NotificationChannel channel, CancellationToken ct)
    {
        return await _context.Preferences
            .FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.TenantId == tenantId &&
                p.Channel == channel, ct);
    }

    public async Task SaveAsync(NotificationPreference preference, CancellationToken ct)
    {
        var existing = await _context.Preferences
            .FirstOrDefaultAsync(p => p.PreferenceId == preference.PreferenceId, ct);

        if (existing is null)
        {
            _context.Preferences.Add(preference);
        }
        else
        {
            _context.Entry(existing).CurrentValues.SetValues(preference);
        }

        await _context.SaveChangesAsync(ct);
    }
}
