namespace ClearEyeQ.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for an eye scan.
/// </summary>
public readonly record struct ScanId(Guid Value)
{
    public static ScanId New() => new(Guid.NewGuid());

    public static implicit operator Guid(ScanId scanId) => scanId.Value;
    public static implicit operator ScanId(Guid guid) => new(guid);

    public override string ToString() => Value.ToString();
}
