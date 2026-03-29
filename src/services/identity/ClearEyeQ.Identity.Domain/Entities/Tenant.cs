using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Identity.Domain.Entities;

public sealed class Tenant
{
    public TenantId TenantId { get; private set; }
    public string Name { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private Tenant() { }

    public static Tenant Create(string name)
    {
        return new Tenant
        {
            TenantId = TenantId.New(),
            Name = name,
            CreatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
