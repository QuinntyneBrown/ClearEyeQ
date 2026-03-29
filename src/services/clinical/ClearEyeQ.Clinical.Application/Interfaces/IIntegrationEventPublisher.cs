using ClearEyeQ.SharedKernel.Domain.Events;

namespace ClearEyeQ.Clinical.Application.Interfaces;

/// <summary>
/// Publishes integration events to the message bus for downstream consumers.
/// </summary>
public interface IIntegrationEventPublisher
{
    Task PublishAsync(IntegrationEventEnvelope envelope, CancellationToken cancellationToken = default);
}
