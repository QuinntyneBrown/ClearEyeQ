using ClearEyeQ.Notifications.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Notifications.Application.Queries.GetNotifications;

public sealed class GetNotificationsHandler : IRequestHandler<GetNotificationsQuery, IReadOnlyList<NotificationDto>>
{
    private readonly INotificationRepository _repository;

    public GetNotificationsHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<NotificationDto>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var tenantId = new TenantId(request.TenantId);

        var notifications = await _repository.GetByUserAsync(
            userId, tenantId, request.Limit, cancellationToken);

        return notifications.Select(n => new NotificationDto(
            NotificationId: n.NotificationId,
            Category: n.Category,
            Title: n.Content.Title,
            Body: n.Content.Body,
            ActionUrl: n.Content.ActionUrl,
            Channel: n.Channel,
            Status: n.Status,
            CreatedAt: n.CreatedAt,
            DeliveryAttemptCount: n.DeliveryAttempts.Count)).ToList().AsReadOnly();
    }
}
