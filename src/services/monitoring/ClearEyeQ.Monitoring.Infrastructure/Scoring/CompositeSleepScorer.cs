using ClearEyeQ.Monitoring.Application.Interfaces;
using ClearEyeQ.Monitoring.Domain.ValueObjects;

namespace ClearEyeQ.Monitoring.Infrastructure.Scoring;

/// <summary>
/// Calculates a composite sleep quality score using weighted averages of sleep metrics.
/// Weights: duration (30%), deep sleep ratio (25%), REM ratio (20%), efficiency (25%).
/// </summary>
public sealed class CompositeSleepScorer : ISleepScorer
{
    private const double DurationWeight = 0.30;
    private const double DeepSleepWeight = 0.25;
    private const double RemSleepWeight = 0.20;
    private const double EfficiencyWeight = 0.25;

    private const double OptimalDurationHours = 8.0;
    private const double MinAcceptableDurationHours = 6.0;
    private const double OptimalDeepSleepRatio = 0.20;
    private const double OptimalRemRatio = 0.25;

    public double CalculateQualityScore(TimeSpan duration, SleepStages stages)
    {
        var durationScore = CalculateDurationScore(duration);
        var deepSleepScore = CalculateDeepSleepScore(stages);
        var remScore = CalculateRemScore(stages);
        var efficiencyScore = stages.SleepEfficiency;

        var composite = (durationScore * DurationWeight)
                      + (deepSleepScore * DeepSleepWeight)
                      + (remScore * RemSleepWeight)
                      + (efficiencyScore * EfficiencyWeight);

        return Math.Clamp(composite, 0.0, 1.0);
    }

    private static double CalculateDurationScore(TimeSpan duration)
    {
        var hours = duration.TotalHours;

        if (hours >= OptimalDurationHours)
            return 1.0;

        if (hours <= 0)
            return 0.0;

        if (hours >= MinAcceptableDurationHours)
        {
            // Linear interpolation between min acceptable and optimal
            return 0.7 + 0.3 * ((hours - MinAcceptableDurationHours) / (OptimalDurationHours - MinAcceptableDurationHours));
        }

        // Below minimum: steep drop-off
        return 0.7 * (hours / MinAcceptableDurationHours);
    }

    private static double CalculateDeepSleepScore(SleepStages stages)
    {
        var totalSleep = stages.TotalSleepTime.TotalMinutes;
        if (totalSleep <= 0) return 0.0;

        var deepRatio = stages.Deep.TotalMinutes / totalSleep;

        if (deepRatio >= OptimalDeepSleepRatio)
            return 1.0;

        return deepRatio / OptimalDeepSleepRatio;
    }

    private static double CalculateRemScore(SleepStages stages)
    {
        var totalSleep = stages.TotalSleepTime.TotalMinutes;
        if (totalSleep <= 0) return 0.0;

        var remRatio = stages.Rem.TotalMinutes / totalSleep;

        if (remRatio >= OptimalRemRatio)
            return 1.0;

        return remRatio / OptimalRemRatio;
    }
}
