using MediatR;

namespace ClearEyeQ.Notifications.Application.Queries.GetNotifications;

public sealed record GetNotificationsQuery(
    Guid UserId,
    Guid TenantId,
    int Limit = 50) : IRequest<IReadOnlyList<NotificationDto>>;
