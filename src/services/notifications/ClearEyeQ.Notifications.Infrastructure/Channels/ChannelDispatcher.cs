using ClearEyeQ.Notifications.Application.Interfaces;
using ClearEyeQ.Notifications.Domain.Enums;
using ClearEyeQ.Notifications.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Notifications.Infrastructure.Channels;

public sealed class ChannelDispatcher
{
    private readonly IEnumerable<IChannelSender> _senders;
    private readonly ILogger<ChannelDispatcher> _logger;

    public ChannelDispatcher(
        IEnumerable<IChannelSender> senders,
        ILogger<ChannelDispatcher> logger)
    {
        _senders = senders;
        _logger = logger;
    }

    public IChannelSender? GetSender(NotificationChannel channel)
    {
        var sender = _senders.FirstOrDefault(s => s.Channel == channel);
        if (sender is null)
        {
            _logger.LogWarning("No sender registered for channel {Channel}", channel);
        }
        return sender;
    }

    public async Task<bool> DispatchAsync(
        NotificationChannel channel,
        UserId userId,
        NotificationContent content,
        CancellationToken ct)
    {
        var sender = GetSender(channel);
        if (sender is null)
            return false;

        return await sender.SendAsync(userId, content, ct);
    }
}
