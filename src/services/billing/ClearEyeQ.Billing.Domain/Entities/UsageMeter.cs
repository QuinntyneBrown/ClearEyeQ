namespace ClearEyeQ.Billing.Domain.Entities;

public sealed class UsageMeter
{
    public Guid MeterId { get; private set; } = Guid.NewGuid();
    public int ScanCount { get; private set; }
    public int ScanLimit { get; private set; }
    public DateOnly PeriodStart { get; private set; }

    private UsageMeter() { }

    public static UsageMeter Create(int scanLimit, DateOnly periodStart)
    {
        return new UsageMeter
        {
            ScanCount = 0,
            ScanLimit = scanLimit,
            PeriodStart = periodStart
        };
    }

    public bool IncrementUsage()
    {
        ScanCount++;
        return ScanCount >= ScanLimit;
    }

    public bool HasReachedLimit() => ScanCount >= ScanLimit;

    public void Reset(DateOnly newPeriodStart)
    {
        ScanCount = 0;
        PeriodStart = newPeriodStart;
    }

    public void UpdateLimit(int newLimit)
    {
        ScanLimit = newLimit;
    }
}
