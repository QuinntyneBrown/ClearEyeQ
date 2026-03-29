using ClearEyeQ.Notifications.Domain.Enums;
using MediatR;

namespace ClearEyeQ.Notifications.Application.Commands.SendNotification;

public sealed record SendNotificationCommand(
    Guid UserId,
    Guid TenantId,
    NotificationCategory Category,
    string Title,
    string Body,
    string? ActionUrl,
    NotificationChannel? PreferredChannel = null) : IRequest<Guid>;
