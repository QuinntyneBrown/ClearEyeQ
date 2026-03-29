using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Scan.Domain.ValueObjects;

public sealed record ScanComparison
{
    public ScanId BaselineScanId { get; }
    public double RednessDelta { get; }
    public double TearFilmDelta { get; }

    public ScanComparison(ScanId baselineScanId, double rednessDelta, double tearFilmDelta)
    {
        BaselineScanId = baselineScanId;
        RednessDelta = rednessDelta;
        TearFilmDelta = tearFilmDelta;
    }
}
