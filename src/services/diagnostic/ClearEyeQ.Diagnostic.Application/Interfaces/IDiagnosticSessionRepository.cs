using ClearEyeQ.Diagnostic.Domain.Aggregates;
using ClearEyeQ.SharedKernel.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Diagnostic.Application.Interfaces;

public interface IDiagnosticSessionRepository : IRepository<DiagnosticSession>
{
    Task<DiagnosticSession?> GetBySessionIdAsync(Guid sessionId, TenantId tenantId, CancellationToken ct);
}
