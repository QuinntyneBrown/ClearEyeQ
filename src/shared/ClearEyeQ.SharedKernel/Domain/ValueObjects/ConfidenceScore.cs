namespace ClearEyeQ.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Represents a confidence score in the range [0.0, 1.0] with human-readable labels.
/// </summary>
public sealed record ConfidenceScore
{
    public double Value { get; }

    public ConfidenceScore(double value)
    {
        if (value < 0.0 || value > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "Confidence score must be between 0.0 and 1.0 inclusive.");
        }

        Value = value;
    }

    /// <summary>
    /// Human-readable label derived from the score value.
    /// Low: [0.0, 0.25), Medium: [0.25, 0.50), High: [0.50, 0.75), VeryHigh: [0.75, 1.0]
    /// </summary>
    public string Label => Value switch
    {
        < 0.25 => "Low",
        < 0.50 => "Medium",
        < 0.75 => "High",
        _ => "VeryHigh"
    };

    public override string ToString() => $"{Value:F2} ({Label})";
}
