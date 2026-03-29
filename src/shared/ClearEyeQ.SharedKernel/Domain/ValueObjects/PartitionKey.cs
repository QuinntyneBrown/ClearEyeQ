namespace ClearEyeQ.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Represents a Cosmos DB partition key, always rooted in tenant scope.
/// </summary>
public sealed record PartitionKey(string Value)
{
    /// <summary>
    /// Creates a partition key scoped to a tenant (for tenant-wide entities).
    /// </summary>
    public static PartitionKey ForTenant(TenantId tenantId) =>
        new(tenantId.Value.ToString());

    /// <summary>
    /// Creates a synthetic partition key for high-cardinality user workloads
    /// using the pattern TenantId|UserId.
    /// </summary>
    public static PartitionKey ForUserInTenant(TenantId tenantId, UserId userId) =>
        new($"{tenantId.Value}|{userId.Value}");

    public override string ToString() => Value;
}
