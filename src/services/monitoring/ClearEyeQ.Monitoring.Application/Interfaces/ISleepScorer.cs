using ClearEyeQ.Monitoring.Domain.ValueObjects;

namespace ClearEyeQ.Monitoring.Application.Interfaces;

public interface ISleepScorer
{
    double CalculateQualityScore(TimeSpan duration, SleepStages stages);
}
