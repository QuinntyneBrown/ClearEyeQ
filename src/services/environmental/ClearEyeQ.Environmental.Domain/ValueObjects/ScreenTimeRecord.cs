namespace ClearEyeQ.Environmental.Domain.ValueObjects;

public sealed record ScreenTimeRecord(TimeSpan TotalDuration, Dictionary<string, TimeSpan> AppBreakdown)
{
    public double TotalHours => TotalDuration.TotalHours;
}
