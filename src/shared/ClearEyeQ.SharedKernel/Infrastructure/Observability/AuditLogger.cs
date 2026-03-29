using System.Text.Json;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Infrastructure.Persistence;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.SharedKernel.Infrastructure.Observability;

/// <summary>
/// Writes structured HIPAA audit entries to a dedicated Cosmos DB container.
/// Each entry is partitioned by tenant for isolation and query efficiency.
/// </summary>
public sealed class AuditLogger : IAuditLogger
{
    private readonly CosmosDbContext _cosmosDbContext;
    private readonly ILogger<AuditLogger> _logger;
    private const string ContainerName = "AuditLog";

    public AuditLogger(CosmosDbContext cosmosDbContext, ILogger<AuditLogger> logger)
    {
        _cosmosDbContext = cosmosDbContext ?? throw new ArgumentNullException(nameof(cosmosDbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task LogAsync(
        TenantId tenantId,
        UserId userId,
        string action,
        string resourceType,
        string resourceId,
        string? detail = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceType);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId);

        var entry = new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = tenantId.Value.ToString(),
            UserId = userId.Value.ToString(),
            Action = action,
            ResourceType = resourceType,
            ResourceId = resourceId,
            Detail = detail,
            Timestamp = DateTimeOffset.UtcNow
        };

        var container = _cosmosDbContext.GetContainer(ContainerName);
        var partitionKey = new Microsoft.Azure.Cosmos.PartitionKey(entry.TenantId);

        await container.CreateItemAsync(entry, partitionKey, cancellationToken: ct);

        _logger.LogInformation(
            "Audit: {Action} on {ResourceType}/{ResourceId} by User {UserId} in Tenant {TenantId}",
            action, resourceType, resourceId, userId, tenantId);
    }

    private sealed class AuditEntry
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public required string Id { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("tenantId")]
        public required string TenantId { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("userId")]
        public required string UserId { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("action")]
        public required string Action { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("resourceType")]
        public required string ResourceType { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("resourceId")]
        public required string ResourceId { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("detail")]
        public string? Detail { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; init; }
    }
}
