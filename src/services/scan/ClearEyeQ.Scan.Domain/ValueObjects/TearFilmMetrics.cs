namespace ClearEyeQ.Scan.Domain.ValueObjects;

public sealed record TearFilmMetrics
{
    public double BreakUpTime { get; }
    public string LipidLayerGrade { get; }
    public double CoveragePercentage { get; }

    public TearFilmMetrics(double breakUpTime, string lipidLayerGrade, double coveragePercentage)
    {
        if (breakUpTime < 0)
            throw new ArgumentOutOfRangeException(nameof(breakUpTime), "Break-up time cannot be negative.");

        if (string.IsNullOrWhiteSpace(lipidLayerGrade))
            throw new ArgumentException("Lipid layer grade is required.", nameof(lipidLayerGrade));

        if (coveragePercentage < 0 || coveragePercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(coveragePercentage), "Coverage percentage must be between 0 and 100.");

        BreakUpTime = breakUpTime;
        LipidLayerGrade = lipidLayerGrade;
        CoveragePercentage = coveragePercentage;
    }
}
