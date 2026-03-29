using System.Text.Json;
using ClearEyeQ.Identity.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using Microsoft.Azure.Cosmos;

namespace ClearEyeQ.Identity.Infrastructure.Audit;

public sealed class CosmosAuditLogRepository : IAuditLogger
{
    private readonly Container _container;

    public CosmosAuditLogRepository(CosmosClient cosmosClient, string databaseName)
    {
        _container = cosmosClient.GetContainer(databaseName, "audit-log");
    }

    public async Task LogAsync(string action, UserId userId, string detail, CancellationToken cancellationToken = default)
    {
        var entry = new
        {
            id = Guid.NewGuid().ToString(),
            userId = userId.Value.ToString(),
            action,
            detail,
            occurredAt = DateTimeOffset.UtcNow,
            partitionKey = userId.Value.ToString()
        };

        var json = JsonSerializer.Serialize(entry);
        var document = JsonDocument.Parse(json).RootElement.Clone();

        await _container.CreateItemAsync(
            document,
            new Microsoft.Azure.Cosmos.PartitionKey(userId.Value.ToString()),
            cancellationToken: cancellationToken);
    }
}
