using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.SharedKernel.Domain;

/// <summary>
/// Interface for HIPAA-auditable entities that track creation and modification metadata.
/// </summary>
public interface IAuditableEntity
{
    AuditMetadata Audit { get; set; }
}
