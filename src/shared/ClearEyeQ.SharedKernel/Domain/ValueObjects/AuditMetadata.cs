namespace ClearEyeQ.SharedKernel.Domain.ValueObjects;

/// <summary>
/// HIPAA audit metadata tracking creation and modification details.
/// </summary>
public sealed record AuditMetadata
{
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public string CreatedBy { get; init; } = string.Empty;
    public DateTimeOffset? ModifiedAt { get; init; }
    public string? ModifiedBy { get; init; }

    public static AuditMetadata Create(string createdBy) =>
        new() { CreatedAt = DateTimeOffset.UtcNow, CreatedBy = createdBy };

    public AuditMetadata WithModification(string modifiedBy) =>
        this with { ModifiedAt = DateTimeOffset.UtcNow, ModifiedBy = modifiedBy };
}
