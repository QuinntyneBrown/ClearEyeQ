using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.SharedKernel.Application.Interfaces;

/// <summary>
/// Generic repository interface for aggregate persistence with tenant scoping.
/// </summary>
/// <typeparam name="T">The aggregate root type.</typeparam>
public interface IRepository<T> where T : AggregateRoot
{
    /// <summary>
    /// Retrieves an aggregate by its identifier within a tenant scope.
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken ct);

    /// <summary>
    /// Persists a new aggregate.
    /// </summary>
    Task AddAsync(T entity, CancellationToken ct);

    /// <summary>
    /// Updates an existing aggregate.
    /// </summary>
    Task UpdateAsync(T entity, CancellationToken ct);
}
