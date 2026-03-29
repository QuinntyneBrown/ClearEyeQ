using ClearEyeQ.Notifications.Application.Interfaces;
using ClearEyeQ.Notifications.Domain.Enums;
using ClearEyeQ.Notifications.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Notifications.Infrastructure.Channels;

public sealed class SignalRInAppSender : IChannelSender
{
    private readonly IHubContext<InAppNotificationHub> _hubContext;
    private readonly ILogger<SignalRInAppSender> _logger;

    public NotificationChannel Channel => NotificationChannel.InApp;

    public SignalRInAppSender(
        IHubContext<InAppNotificationHub> hubContext,
        ILogger<SignalRInAppSender> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<bool> SendAsync(UserId userId, NotificationContent content, CancellationToken ct)
    {
        try
        {
            await _hubContext.Clients
                .Group(userId.Value.ToString())
                .SendAsync("ReceiveNotification", new
                {
                    content.Title,
                    content.Body,
                    content.ActionUrl,
                    Timestamp = DateTimeOffset.UtcNow
                }, ct);

            _logger.LogInformation("SignalR in-app notification sent to user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SignalR in-app notification failed for user {UserId}", userId);
            return false;
        }
    }
}

/// <summary>
/// SignalR hub for in-app notifications. Mapped in API Program.cs.
/// </summary>
public sealed class InAppNotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}
