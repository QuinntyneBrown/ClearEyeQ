using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Treatment.Domain.Events;

public sealed record EscalationRecommendedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public Guid PlanId { get; init; }
    public UserId UserId { get; init; }
    public TenantId TenantId { get; init; }
    public string RecommendedAction { get; init; } = string.Empty;
    public double CurrentImprovementPercent { get; init; }
}
