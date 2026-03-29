using ClearEyeQ.Treatment.Application.Interfaces;
using ClearEyeQ.Treatment.Domain.Aggregates;

namespace ClearEyeQ.Treatment.Infrastructure.Services;

public sealed class EfficacyCalculator : IEfficacyCalculator
{
    private const double ResolutionThresholdPercent = 80.0;

    public double CalculateImprovement(TreatmentPlan plan)
    {
        var measurements = plan.EfficacyMeasurements;
        if (measurements.Count == 0)
            return 0.0;

        var latest = measurements
            .OrderByDescending(m => m.MeasuredAt)
            .First();

        return latest.DeltaPercent;
    }

    public bool ShouldEscalate(TreatmentPlan plan)
    {
        return plan.EvaluateEfficacy();
    }

    public bool IsResolved(TreatmentPlan plan)
    {
        var measurements = plan.EfficacyMeasurements;
        if (measurements.Count < 3)
            return false;

        var recentMeasurements = measurements
            .OrderByDescending(m => m.MeasuredAt)
            .Take(3)
            .ToList();

        var averageDelta = recentMeasurements.Average(m => m.DeltaPercent);
        return averageDelta >= ResolutionThresholdPercent;
    }
}
