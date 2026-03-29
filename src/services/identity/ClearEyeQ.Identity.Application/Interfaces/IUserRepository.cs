using ClearEyeQ.Identity.Domain.Aggregates;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Identity.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId userId, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, TenantId tenantId, CancellationToken cancellationToken = default);
    Task SaveAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string email, TenantId tenantId, CancellationToken cancellationToken = default);
}
