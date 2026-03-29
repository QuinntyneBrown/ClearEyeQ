using ClearEyeQ.Notifications.Domain.Enums;

namespace ClearEyeQ.Notifications.Application.Queries.GetNotifications;

public sealed record NotificationDto(
    Guid NotificationId,
    NotificationCategory Category,
    string Title,
    string Body,
    string? ActionUrl,
    NotificationChannel Channel,
    DeliveryStatus Status,
    DateTimeOffset CreatedAt,
    int DeliveryAttemptCount);
