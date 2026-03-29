using ClearEyeQ.Monitoring.Domain.Aggregates;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Monitoring.Application.Interfaces;

public interface IMonitoringRepository
{
    Task<MonitoringSession?> GetByIdAsync(Guid sessionId, TenantId tenantId, CancellationToken ct);
    Task<MonitoringSession?> GetActiveSessionAsync(UserId userId, TenantId tenantId, CancellationToken ct);
    Task AddAsync(MonitoringSession session, CancellationToken ct);
    Task UpdateAsync(MonitoringSession session, CancellationToken ct);
    Task<IReadOnlyList<MonitoringSession>> GetRecentSessionsAsync(UserId userId, TenantId tenantId, int count, CancellationToken ct);
}
