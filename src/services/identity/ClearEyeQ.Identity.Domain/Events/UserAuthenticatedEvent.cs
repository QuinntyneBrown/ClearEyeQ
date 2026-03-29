using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Identity.Domain.Events;

public sealed record UserAuthenticatedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public UserId UserId { get; init; }
    public string DeviceFingerprint { get; init; } = default!;
}
