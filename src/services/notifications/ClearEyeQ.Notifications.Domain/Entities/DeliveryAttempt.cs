using ClearEyeQ.Notifications.Domain.Enums;

namespace ClearEyeQ.Notifications.Domain.Entities;

public sealed class DeliveryAttempt
{
    public Guid AttemptId { get; private set; } = Guid.NewGuid();
    public NotificationChannel Channel { get; private set; }
    public DeliveryStatus Status { get; private set; }
    public DateTimeOffset AttemptedAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    private DeliveryAttempt() { }

    public static DeliveryAttempt Create(NotificationChannel channel)
    {
        return new DeliveryAttempt
        {
            Channel = channel,
            Status = DeliveryStatus.Pending,
            AttemptedAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkSent()
    {
        Status = DeliveryStatus.Sent;
    }

    public void MarkDelivered()
    {
        Status = DeliveryStatus.Delivered;
    }

    public void MarkFailed(string errorMessage)
    {
        Status = DeliveryStatus.Failed;
        ErrorMessage = errorMessage;
    }
}
