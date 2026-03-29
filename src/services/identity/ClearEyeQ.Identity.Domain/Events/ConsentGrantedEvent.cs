using ClearEyeQ.Identity.Domain.Enums;
using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Identity.Domain.Events;

public sealed record ConsentGrantedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public UserId UserId { get; init; }
    public ConsentType ConsentType { get; init; }
}
