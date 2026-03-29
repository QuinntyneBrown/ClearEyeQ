using ClearEyeQ.Predictive.Domain.Enums;
using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Predictive.Domain.Events;

public sealed record FlareUpWarningEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
    public Guid PredictionId { get; init; }
    public UserId UserId { get; init; }
    public TenantId TenantId { get; init; }
    public double Probability { get; init; }
    public RiskLevel RiskLevel { get; init; }
}
