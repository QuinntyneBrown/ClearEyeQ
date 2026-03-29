using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.SharedKernel.Domain;

/// <summary>
/// Base class for all aggregate roots. Provides domain event tracking,
/// audit metadata, and tenant scoping.
/// </summary>
public abstract class AggregateRoot : IAuditableEntity, ITenantScopedEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>Unique identifier for the aggregate.</summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>Domain events raised during the current unit of work.</summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>HIPAA audit metadata for this entity.</summary>
    public AuditMetadata Audit { get; set; } = default!;

    /// <summary>The tenant this aggregate belongs to.</summary>
    public abstract TenantId TenantId { get; }

    /// <summary>The Cosmos DB partition key for this aggregate.</summary>
    public abstract PartitionKey PartitionKey { get; }

    /// <summary>
    /// Adds a domain event to be dispatched after the aggregate is persisted.
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all pending domain events. Called after successful dispatch.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
