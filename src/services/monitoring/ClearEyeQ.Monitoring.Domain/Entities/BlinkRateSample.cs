namespace ClearEyeQ.Monitoring.Domain.Entities;

public sealed class BlinkRateSample
{
    public Guid Id { get; private set; }
    public double BlinksPerMinute { get; private set; }
    public double FatigueScore { get; private set; }
    public DateTimeOffset MeasuredAt { get; private set; }

    private BlinkRateSample() { }

    public static BlinkRateSample Create(
        double blinksPerMinute,
        double fatigueScore,
        DateTimeOffset measuredAt)
    {
        if (blinksPerMinute < 0)
            throw new ArgumentOutOfRangeException(nameof(blinksPerMinute), "Blinks per minute cannot be negative.");

        if (fatigueScore is < 0.0 or > 1.0)
            throw new ArgumentOutOfRangeException(nameof(fatigueScore), "Fatigue score must be between 0.0 and 1.0.");

        return new BlinkRateSample
        {
            Id = Guid.NewGuid(),
            BlinksPerMinute = blinksPerMinute,
            FatigueScore = fatigueScore,
            MeasuredAt = measuredAt
        };
    }
}
