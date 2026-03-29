using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Billing.Application.Interfaces;

public interface IUsageMeterStore
{
    Task<int> GetCurrentUsageAsync(TenantId tenantId, CancellationToken ct);
    Task IncrementAsync(TenantId tenantId, CancellationToken ct);
    Task ResetAsync(TenantId tenantId, CancellationToken ct);
}
