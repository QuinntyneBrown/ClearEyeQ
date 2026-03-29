using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Diagnostic.Domain.Events;

public sealed record DiagnosisCompletedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
    public Guid SessionId { get; init; }
    public UserId UserId { get; init; }
    public TenantId TenantId { get; init; }
    public List<TopCondition> TopConditions { get; init; } = [];

    public sealed record TopCondition(
        string ConditionCode,
        string ConditionName,
        double Confidence,
        Severity Severity);
}
