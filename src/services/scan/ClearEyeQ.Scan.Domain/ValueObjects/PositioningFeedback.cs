namespace ClearEyeQ.Scan.Domain.ValueObjects;

public sealed record PositioningFeedback
{
    public double AlignmentScore { get; }
    public string DirectionalHint { get; }
    public bool IsReady { get; }

    public PositioningFeedback(double alignmentScore, string directionalHint, bool isReady)
    {
        if (alignmentScore < 0 || alignmentScore > 1)
            throw new ArgumentOutOfRangeException(nameof(alignmentScore), "Alignment score must be between 0 and 1.");

        ArgumentNullException.ThrowIfNull(directionalHint);

        AlignmentScore = alignmentScore;
        DirectionalHint = directionalHint;
        IsReady = isReady;
    }
}
