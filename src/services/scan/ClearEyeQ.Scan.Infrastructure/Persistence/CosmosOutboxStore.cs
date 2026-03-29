using System.Text.Json;
using ClearEyeQ.Scan.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.Events;
using Microsoft.Azure.Cosmos;

namespace ClearEyeQ.Scan.Infrastructure.Persistence;

public sealed class CosmosOutboxStore : IOutboxStore
{
    private readonly Container _container;

    public CosmosOutboxStore(CosmosClient cosmosClient, string databaseName)
    {
        _container = cosmosClient.GetContainer(databaseName, "outbox");
    }

    public async Task SaveAsync(IntegrationEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        var document = new
        {
            id = envelope.EventId.ToString(),
            partitionKey = envelope.TenantId.Value.ToString(),
            eventType = envelope.EventType,
            schemaVersion = envelope.SchemaVersion,
            tenantId = envelope.TenantId.Value.ToString(),
            subjectId = envelope.SubjectId.ToString(),
            correlationId = envelope.CorrelationId.ToString(),
            causationId = envelope.CausationId.ToString(),
            occurredAtUtc = envelope.OccurredAtUtc,
            payloadJson = envelope.PayloadJson,
            publishedAt = (DateTimeOffset?)null,
            status = "Pending"
        };

        await _container.CreateItemAsync(
            document,
            new Microsoft.Azure.Cosmos.PartitionKey(envelope.TenantId.Value.ToString()),
            cancellationToken: cancellationToken);
    }
}
