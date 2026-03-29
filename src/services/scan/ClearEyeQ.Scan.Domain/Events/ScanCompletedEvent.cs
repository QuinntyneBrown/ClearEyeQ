using ClearEyeQ.Scan.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Scan.Domain.Events;

public sealed record ScanCompletedEvent(
    ScanId ScanId,
    UserId UserId,
    TenantId TenantId,
    RednessScore RednessScore) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
