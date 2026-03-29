namespace ClearEyeQ.Scan.Domain.ValueObjects;

public sealed record RednessScore
{
    public double Overall { get; }
    public double Confidence { get; }
    public IReadOnlyDictionary<string, double> ZoneScores { get; }

    public RednessScore(double overall, double confidence, Dictionary<string, double> zoneScores)
    {
        if (overall < 0 || overall > 100)
            throw new ArgumentOutOfRangeException(nameof(overall), "Redness score must be between 0 and 100.");

        if (confidence < 0 || confidence > 1)
            throw new ArgumentOutOfRangeException(nameof(confidence), "Confidence must be between 0 and 1.");

        ArgumentNullException.ThrowIfNull(zoneScores);

        Overall = overall;
        Confidence = confidence;
        ZoneScores = new Dictionary<string, double>(zoneScores);
    }
}
