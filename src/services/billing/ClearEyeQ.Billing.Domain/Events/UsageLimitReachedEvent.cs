using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Billing.Domain.Events;

public sealed record UsageLimitReachedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public Guid SubscriptionId { get; init; }
    public TenantId TenantId { get; init; }
    public int ScanCount { get; init; }
    public int ScanLimit { get; init; }
}
