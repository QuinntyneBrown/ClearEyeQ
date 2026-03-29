using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Notifications.Domain.Events;

public sealed record NotificationFailedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public Guid NotificationId { get; init; }
    public UserId UserId { get; init; }
    public TenantId TenantId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
