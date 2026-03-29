using ClearEyeQ.Monitoring.Domain.ValueObjects;

namespace ClearEyeQ.Monitoring.Domain.Entities;

public sealed class SleepRecord
{
    public Guid Id { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeSpan Duration { get; private set; }
    public SleepStages Stages { get; private set; } = default!;
    public double QualityScore { get; private set; }

    private SleepRecord() { }

    public static SleepRecord Create(
        DateOnly date,
        TimeSpan duration,
        SleepStages stages,
        double qualityScore)
    {
        if (qualityScore is < 0.0 or > 1.0)
            throw new ArgumentOutOfRangeException(nameof(qualityScore), "Quality score must be between 0.0 and 1.0.");

        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");

        return new SleepRecord
        {
            Id = Guid.NewGuid(),
            Date = date,
            Duration = duration,
            Stages = stages,
            QualityScore = qualityScore
        };
    }
}
