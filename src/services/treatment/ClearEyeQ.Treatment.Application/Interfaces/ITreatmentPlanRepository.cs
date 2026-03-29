using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using ClearEyeQ.Treatment.Domain.Aggregates;

namespace ClearEyeQ.Treatment.Application.Interfaces;

public interface ITreatmentPlanRepository
{
    Task<TreatmentPlan?> GetByIdAsync(Guid planId, TenantId tenantId, CancellationToken ct);
    Task<TreatmentPlan?> GetActivePlanAsync(UserId userId, TenantId tenantId, CancellationToken ct);
    Task<IReadOnlyList<TreatmentPlan>> GetActivePlansAsync(TenantId tenantId, CancellationToken ct);
    Task AddAsync(TreatmentPlan plan, CancellationToken ct);
    Task UpdateAsync(TreatmentPlan plan, CancellationToken ct);
}
