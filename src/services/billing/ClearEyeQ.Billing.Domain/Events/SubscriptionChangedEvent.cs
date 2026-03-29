using ClearEyeQ.Billing.Domain.Enums;
using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Billing.Domain.Events;

public sealed record SubscriptionChangedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public Guid SubscriptionId { get; init; }
    public TenantId TenantId { get; init; }
    public PlanTier OldTier { get; init; }
    public PlanTier NewTier { get; init; }
}
