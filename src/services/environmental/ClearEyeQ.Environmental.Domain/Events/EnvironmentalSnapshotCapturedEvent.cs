using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Environmental.Domain.Events;

public sealed record EnvironmentalSnapshotCapturedEvent(
    Guid SnapshotId,
    UserId UserId,
    TenantId TenantId,
    DateTimeOffset CapturedAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    DateTimeOffset IDomainEvent.OccurredAt => CapturedAt;
}
