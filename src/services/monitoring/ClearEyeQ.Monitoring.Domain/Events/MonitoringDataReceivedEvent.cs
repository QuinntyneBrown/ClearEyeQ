using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Monitoring.Domain.Events;

public sealed record MonitoringDataReceivedEvent(
    Guid SessionId,
    UserId UserId,
    TenantId TenantId,
    string DataType,
    DateTimeOffset OccurredAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    DateTimeOffset IDomainEvent.OccurredAt => OccurredAt;
}
