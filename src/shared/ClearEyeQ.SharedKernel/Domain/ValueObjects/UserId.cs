namespace ClearEyeQ.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for a user.
/// </summary>
public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());

    public static implicit operator Guid(UserId userId) => userId.Value;
    public static implicit operator UserId(Guid guid) => new(guid);

    public override string ToString() => Value.ToString();
}
