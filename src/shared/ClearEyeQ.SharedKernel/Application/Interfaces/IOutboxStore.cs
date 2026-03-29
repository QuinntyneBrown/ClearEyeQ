using ClearEyeQ.SharedKernel.Domain.Events;

namespace ClearEyeQ.SharedKernel.Application.Interfaces;

/// <summary>
/// Interface for the transactional outbox store that persists integration events
/// for reliable asynchronous publishing.
/// </summary>
public interface IOutboxStore
{
    /// <summary>
    /// Saves an integration event envelope to the outbox.
    /// </summary>
    Task SaveEventAsync(IntegrationEventEnvelope evt, CancellationToken ct);

    /// <summary>
    /// Retrieves a batch of unpublished events ordered by occurrence time.
    /// </summary>
    Task<IReadOnlyList<IntegrationEventEnvelope>> GetPendingAsync(int batchSize, CancellationToken ct);

    /// <summary>
    /// Marks an event as successfully published so it is not re-sent.
    /// </summary>
    Task MarkPublishedAsync(Guid eventId, CancellationToken ct);
}
