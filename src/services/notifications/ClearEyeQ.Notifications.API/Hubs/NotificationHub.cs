// The NotificationHub is defined in ClearEyeQ.Notifications.Infrastructure.Channels.InAppNotificationHub
// This file re-exports it for API mapping convenience.
using ClearEyeQ.Notifications.Infrastructure.Channels;

namespace ClearEyeQ.Notifications.API.Hubs;

/// <summary>
/// Re-export alias for InAppNotificationHub used in endpoint mapping.
/// </summary>
public static class NotificationHubMapping
{
    public static void MapNotificationHub(this WebApplication app)
    {
        app.MapHub<InAppNotificationHub>("/hubs/notifications");
    }
}
