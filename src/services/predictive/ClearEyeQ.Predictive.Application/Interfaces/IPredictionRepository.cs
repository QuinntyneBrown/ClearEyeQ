using ClearEyeQ.Predictive.Domain.Aggregates;
using ClearEyeQ.SharedKernel.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Predictive.Application.Interfaces;

public interface IPredictionRepository : IRepository<Prediction>
{
    Task<Prediction?> GetLatestByUserAsync(UserId userId, TenantId tenantId, CancellationToken ct);
}
