using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Treatment.Domain.Events;

public sealed record InterventionAdjustedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public Guid PlanId { get; init; }
    public Guid InterventionId { get; init; }
    public TenantId TenantId { get; init; }
    public string AdjustmentDescription { get; init; } = string.Empty;
}
