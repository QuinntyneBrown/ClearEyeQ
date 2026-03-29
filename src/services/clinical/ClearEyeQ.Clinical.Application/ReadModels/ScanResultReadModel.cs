namespace ClearEyeQ.Clinical.Application.ReadModels;

/// <summary>
/// Projected read model for scan results, sourced from the Scan bounded context.
/// </summary>
public sealed class ScanResultReadModel
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ScanId { get; set; }
    public string EyeSide { get; set; } = string.Empty;
    public double RednessScore { get; set; }
    public double TearFilmStability { get; set; }
    public double ConfidenceScore { get; set; }
    public DateTimeOffset CompletedAtUtc { get; set; }
}
