using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.SharedKernel.Infrastructure.Observability;

/// <summary>
/// Interface for HIPAA-compliant audit logging of PHI access and privileged mutations.
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Logs a structured audit entry for a user action within a tenant scope.
    /// </summary>
    Task LogAsync(
        TenantId tenantId,
        UserId userId,
        string action,
        string resourceType,
        string resourceId,
        string? detail = null,
        CancellationToken ct = default);
}
