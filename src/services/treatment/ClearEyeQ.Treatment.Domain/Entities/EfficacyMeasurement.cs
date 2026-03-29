namespace ClearEyeQ.Treatment.Domain.Entities;

public sealed class EfficacyMeasurement
{
    public Guid MeasurementId { get; private set; } = Guid.NewGuid();
    public DateTimeOffset MeasuredAt { get; private set; }
    public double RednessScore { get; private set; }
    public double BaselineScore { get; private set; }
    public double DeltaPercent { get; private set; }

    private EfficacyMeasurement() { }

    public static EfficacyMeasurement Record(
        double rednessScore,
        double baselineScore)
    {
        var delta = baselineScore > 0
            ? ((baselineScore - rednessScore) / baselineScore) * 100.0
            : 0.0;

        return new EfficacyMeasurement
        {
            MeasuredAt = DateTimeOffset.UtcNow,
            RednessScore = rednessScore,
            BaselineScore = baselineScore,
            DeltaPercent = Math.Round(delta, 2)
        };
    }
}
