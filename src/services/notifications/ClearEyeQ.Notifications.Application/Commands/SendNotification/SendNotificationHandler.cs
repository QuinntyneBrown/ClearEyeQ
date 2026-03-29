using ClearEyeQ.Notifications.Application.Interfaces;
using ClearEyeQ.Notifications.Domain.Aggregates;
using ClearEyeQ.Notifications.Domain.Enums;
using ClearEyeQ.Notifications.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Notifications.Application.Commands.SendNotification;

public sealed class SendNotificationHandler : IRequestHandler<SendNotificationCommand, Guid>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IPreferenceRepository _preferenceRepository;
    private readonly IEnumerable<IChannelSender> _channelSenders;
    private readonly ILogger<SendNotificationHandler> _logger;

    public SendNotificationHandler(
        INotificationRepository notificationRepository,
        IPreferenceRepository preferenceRepository,
        IEnumerable<IChannelSender> channelSenders,
        ILogger<SendNotificationHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _preferenceRepository = preferenceRepository;
        _channelSenders = channelSenders;
        _logger = logger;
    }

    public async Task<Guid> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var tenantId = new TenantId(request.TenantId);
        var content = NotificationContent.Create(request.Title, request.Body, request.ActionUrl);
        var now = DateTimeOffset.UtcNow;

        var channel = request.PreferredChannel ?? NotificationChannel.InApp;

        // Check user preferences
        var preference = await _preferenceRepository.GetAsync(userId, tenantId, channel, cancellationToken);
        if (preference is not null && !preference.ShouldDeliver(now))
        {
            _logger.LogInformation(
                "Notification suppressed for user {UserId} on channel {Channel} due to preferences/quiet hours",
                userId, channel);

            // Fall back to InApp if preferred channel is suppressed
            if (channel != NotificationChannel.InApp)
            {
                var inAppPreference = await _preferenceRepository.GetAsync(
                    userId, tenantId, NotificationChannel.InApp, cancellationToken);

                if (inAppPreference is null || inAppPreference.ShouldDeliver(now))
                {
                    channel = NotificationChannel.InApp;
                }
                else
                {
                    _logger.LogInformation(
                        "All channels suppressed for user {UserId}, creating notification as pending",
                        userId);
                }
            }
        }

        var notification = Notification.Create(userId, tenantId, request.Category, content, channel);
        await _notificationRepository.AddAsync(notification, cancellationToken);

        // Attempt delivery
        var sender = _channelSenders.FirstOrDefault(s => s.Channel == channel);
        if (sender is not null)
        {
            var attempt = notification.RecordDeliveryAttempt();

            try
            {
                var success = await sender.SendAsync(userId, content, cancellationToken);

                if (success)
                {
                    notification.MarkSent(attempt);
                    _logger.LogInformation(
                        "Notification {NotificationId} sent via {Channel} to user {UserId}",
                        notification.NotificationId, channel, userId);
                }
                else
                {
                    notification.MarkFailed(attempt, "Channel sender returned failure");
                    _logger.LogWarning(
                        "Notification {NotificationId} failed to send via {Channel} to user {UserId}",
                        notification.NotificationId, channel, userId);
                }
            }
            catch (Exception ex)
            {
                notification.MarkFailed(attempt, ex.Message);
                _logger.LogError(ex,
                    "Exception sending notification {NotificationId} via {Channel} to user {UserId}",
                    notification.NotificationId, channel, userId);
            }
        }

        await _notificationRepository.UpdateAsync(notification, cancellationToken);

        return notification.NotificationId;
    }
}
