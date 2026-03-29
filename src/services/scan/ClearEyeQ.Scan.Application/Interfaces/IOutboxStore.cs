using ClearEyeQ.SharedKernel.Domain.Events;

namespace ClearEyeQ.Scan.Application.Interfaces;

public interface IOutboxStore
{
    Task SaveAsync(IntegrationEventEnvelope envelope, CancellationToken cancellationToken = default);
}
