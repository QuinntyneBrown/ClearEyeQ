namespace ClearEyeQ.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for a tenant, ensuring data isolation across the platform.
/// </summary>
public readonly record struct TenantId(Guid Value)
{
    public static TenantId New() => new(Guid.NewGuid());

    public static implicit operator Guid(TenantId tenantId) => tenantId.Value;
    public static implicit operator TenantId(Guid guid) => new(guid);

    public override string ToString() => Value.ToString();
}
