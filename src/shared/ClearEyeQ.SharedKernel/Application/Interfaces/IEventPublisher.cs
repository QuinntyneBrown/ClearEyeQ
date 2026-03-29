using ClearEyeQ.SharedKernel.Domain.Events;

namespace ClearEyeQ.SharedKernel.Application.Interfaces;

/// <summary>
/// Interface for publishing integration events to the message bus.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an integration event envelope to the message bus.
    /// </summary>
    Task PublishAsync(IntegrationEventEnvelope envelope, CancellationToken ct = default);
}
