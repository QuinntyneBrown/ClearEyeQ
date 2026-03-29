using ClearEyeQ.Notifications.Domain.Entities;
using ClearEyeQ.Notifications.Domain.Enums;
using ClearEyeQ.Notifications.Domain.Events;
using ClearEyeQ.Notifications.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Notifications.Domain.Aggregates;

public sealed class Notification : AggregateRoot
{
    private readonly List<DeliveryAttempt> _deliveryAttempts = [];

    public Guid NotificationId => Id;
    private TenantId _tenantId;
    public override TenantId TenantId => _tenantId;
    public override PartitionKey PartitionKey => PartitionKey.ForUserInTenant(_tenantId, UserId);
    public UserId UserId { get; private set; }
    public NotificationCategory Category { get; private set; }
    public NotificationContent Content { get; private set; } = default!;
    public NotificationChannel Channel { get; private set; }
    public DeliveryStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyList<DeliveryAttempt> DeliveryAttempts => _deliveryAttempts.AsReadOnly();

    private Notification() { }

    public static Notification Create(
        UserId userId,
        TenantId tenantId,
        NotificationCategory category,
        NotificationContent content,
        NotificationChannel channel)
    {
        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            _tenantId = tenantId,
            Category = category,
            Content = content,
            Channel = channel,
            Status = DeliveryStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            Audit = AuditMetadata.Create("system")
        };
    }

    public DeliveryAttempt RecordDeliveryAttempt()
    {
        var attempt = DeliveryAttempt.Create(Channel);
        _deliveryAttempts.Add(attempt);
        return attempt;
    }

    public void MarkDelivered(DeliveryAttempt attempt)
    {
        attempt.MarkDelivered();
        Status = DeliveryStatus.Delivered;
        Audit = Audit.WithModification("system");

        AddDomainEvent(new NotificationDeliveredEvent
        {
            NotificationId = Id,
            UserId = UserId,
            TenantId = _tenantId,
            Channel = Channel
        });
    }

    public void MarkSent(DeliveryAttempt attempt)
    {
        attempt.MarkSent();
        Status = DeliveryStatus.Sent;
        Audit = Audit.WithModification("system");
    }

    public void MarkFailed(DeliveryAttempt attempt, string errorMessage)
    {
        attempt.MarkFailed(errorMessage);

        if (_deliveryAttempts.Count >= 3)
        {
            Status = DeliveryStatus.Failed;
            Audit = Audit.WithModification("system");

            AddDomainEvent(new NotificationFailedEvent
            {
                NotificationId = Id,
                UserId = UserId,
                TenantId = _tenantId,
                Reason = errorMessage
            });
        }
    }
}
