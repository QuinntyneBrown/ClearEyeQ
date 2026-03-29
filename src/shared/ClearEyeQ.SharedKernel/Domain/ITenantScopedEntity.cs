using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.SharedKernel.Domain;

/// <summary>
/// Interface for entities scoped to a specific tenant, ensuring data isolation.
/// </summary>
public interface ITenantScopedEntity
{
    /// <summary>The tenant this entity belongs to.</summary>
    TenantId TenantId { get; }

    /// <summary>The Cosmos DB partition key derived from the tenant scope.</summary>
    PartitionKey PartitionKey { get; }
}
