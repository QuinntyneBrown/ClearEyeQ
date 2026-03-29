using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Diagnostic.Application.Interfaces;

public interface IMedicationRepository
{
    Task<List<string>> GetActiveMedicationsAsync(UserId userId, TenantId tenantId, CancellationToken ct);
}
