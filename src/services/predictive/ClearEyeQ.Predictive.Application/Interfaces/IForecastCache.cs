using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Predictive.Application.Interfaces;

public interface IForecastCache
{
    Task<string?> GetForecastAsync(UserId userId, TenantId tenantId, CancellationToken ct);
    Task SetForecastAsync(UserId userId, TenantId tenantId, string forecastJson, TimeSpan ttl, CancellationToken ct);
    Task InvalidateAsync(UserId userId, TenantId tenantId, CancellationToken ct);
}
