using MediatR;

namespace ClearEyeQ.SharedKernel.Domain;

/// <summary>
/// Base interface for in-process domain events raised by aggregates.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>Unique identifier for this event instance.</summary>
    Guid EventId { get; }

    /// <summary>Timestamp when the event occurred.</summary>
    DateTimeOffset OccurredAt { get; }
}
