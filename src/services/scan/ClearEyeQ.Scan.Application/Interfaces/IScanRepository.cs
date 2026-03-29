using ClearEyeQ.Scan.Domain.Aggregates;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Scan.Application.Interfaces;

public interface IScanRepository
{
    Task<Domain.Aggregates.Scan?> GetByIdAsync(ScanId scanId, TenantId tenantId, CancellationToken cancellationToken = default);
    Task SaveAsync(Domain.Aggregates.Scan scan, CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.Aggregates.Scan scan, CancellationToken cancellationToken = default);
}
