namespace ClearEyeQ.Monitoring.Domain.ValueObjects;

public sealed record SleepStages(
    TimeSpan Deep,
    TimeSpan Light,
    TimeSpan Rem,
    TimeSpan Awake)
{
    public TimeSpan TotalSleepTime => Deep + Light + Rem;
    public TimeSpan TotalTimeInBed => Deep + Light + Rem + Awake;

    public double SleepEfficiency =>
        TotalTimeInBed.TotalMinutes > 0
            ? TotalSleepTime.TotalMinutes / TotalTimeInBed.TotalMinutes
            : 0.0;
}
