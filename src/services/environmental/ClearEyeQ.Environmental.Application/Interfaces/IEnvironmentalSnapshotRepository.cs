using ClearEyeQ.Environmental.Domain.Aggregates;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Environmental.Application.Interfaces;

public interface IEnvironmentalSnapshotRepository
{
    Task<EnvironmentalSnapshot?> GetByIdAsync(Guid snapshotId, TenantId tenantId, CancellationToken ct);
    Task<EnvironmentalSnapshot?> GetLatestAsync(UserId userId, TenantId tenantId, CancellationToken ct);
    Task<IReadOnlyList<EnvironmentalSnapshot>> GetHistoryAsync(UserId userId, TenantId tenantId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    Task AddAsync(EnvironmentalSnapshot snapshot, CancellationToken ct);
}
