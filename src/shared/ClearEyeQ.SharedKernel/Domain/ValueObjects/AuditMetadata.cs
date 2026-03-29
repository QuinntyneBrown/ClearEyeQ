namespace ClearEyeQ.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Immutable audit metadata for HIPAA-compliant entity tracking.
/// </summary>
public sealed record AuditMetadata(
    DateTimeOffset CreatedAt,
    UserId CreatedBy,
    DateTimeOffset? ModifiedAt,
    UserId? ModifiedBy)
{
    /// <summary>
    /// Creates new audit metadata for entity creation.
    /// </summary>
    public static AuditMetadata Create(UserId createdBy) =>
        new(DateTimeOffset.UtcNow, createdBy, null, null);

    /// <summary>
    /// Returns new audit metadata reflecting a modification by the specified user.
    /// </summary>
    public AuditMetadata WithModification(UserId modifiedBy) =>
        this with { ModifiedAt = DateTimeOffset.UtcNow, ModifiedBy = modifiedBy };
}
