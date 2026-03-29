using ClearEyeQ.Scan.Domain.Entities;
using ClearEyeQ.Scan.Domain.Enums;
using ClearEyeQ.Scan.Domain.Events;
using ClearEyeQ.Scan.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using PartitionKey = ClearEyeQ.SharedKernel.Domain.ValueObjects.PartitionKey;

namespace ClearEyeQ.Scan.Domain.Aggregates;

public sealed class Scan : AggregateRoot
{
    private readonly List<ScanImage> _images = [];

    public ScanId ScanId { get; private set; }
    public UserId UserId { get; private set; }
    public EyeSide EyeSide { get; private set; }
    public ScanStatus Status { get; private set; }
    public CaptureMetadata CaptureMetadata { get; private set; } = default!;
    public IReadOnlyList<ScanImage> Images => _images.AsReadOnly();
    public RednessScore? RednessScore { get; private set; }
    public TearFilmMetrics? TearFilmMetrics { get; private set; }
    public ScanComparison? Comparison { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public string? FailureReason { get; private set; }

    private TenantId _tenantId;
    public override TenantId TenantId => _tenantId;
    public override PartitionKey PartitionKey => PartitionKey.ForUserInTenant(_tenantId, UserId);

    private Scan() { }

    public static Scan Initiate(
        UserId userId,
        TenantId tenantId,
        EyeSide eyeSide,
        CaptureMetadata captureMetadata)
    {
        ArgumentNullException.ThrowIfNull(captureMetadata);

        var scan = new Scan
        {
            Id = Guid.NewGuid(),
            ScanId = ScanId.New(),
            UserId = userId,
            _tenantId = tenantId,
            EyeSide = eyeSide,
            Status = ScanStatus.Initiated,
            CaptureMetadata = captureMetadata,
            CreatedAt = DateTimeOffset.UtcNow,
            Audit = AuditMetadata.Create(userId.Value.ToString())
        };

        return scan;
    }

    public void AddImage(ScanImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        if (Status != ScanStatus.Initiated && Status != ScanStatus.Capturing)
            throw new InvalidOperationException($"Cannot add images when scan is in {Status} status.");

        _images.Add(image);
        Status = ScanStatus.Capturing;

        // Auto-select the highest quality image
        var bestImage = _images.OrderByDescending(i => i.QualityScore).First();
        foreach (var img in _images) img.Deselect();
        bestImage.Select();
    }

    public void MarkProcessing()
    {
        if (Status != ScanStatus.Capturing && Status != ScanStatus.Initiated)
            throw new InvalidOperationException($"Cannot mark as processing when scan is in {Status} status.");

        if (_images.Count == 0)
            throw new InvalidOperationException("Cannot process a scan with no images.");

        Status = ScanStatus.Processing;
        Audit = Audit.WithModification(UserId.Value.ToString());
    }

    public void Complete(RednessScore rednessScore, TearFilmMetrics tearFilmMetrics)
    {
        ArgumentNullException.ThrowIfNull(rednessScore);
        ArgumentNullException.ThrowIfNull(tearFilmMetrics);

        if (Status != ScanStatus.Processing)
            throw new InvalidOperationException($"Cannot complete scan when scan is in {Status} status.");

        RednessScore = rednessScore;
        TearFilmMetrics = tearFilmMetrics;
        Status = ScanStatus.Completed;
        Audit = Audit.WithModification(UserId.Value.ToString());

        AddDomainEvent(new ScanCompletedEvent(ScanId, UserId, TenantId, rednessScore));
    }

    public void Fail(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Failure reason is required.", nameof(reason));

        FailureReason = reason;
        Status = ScanStatus.Failed;
        Audit = Audit.WithModification(UserId.Value.ToString());

        AddDomainEvent(new ScanFailedEvent(ScanId, reason));
    }

    public void CompareWith(ScanId baselineScanId, RednessScore baselineRednessScore)
    {
        ArgumentNullException.ThrowIfNull(baselineRednessScore);

        if (Status != ScanStatus.Completed)
            throw new InvalidOperationException("Cannot compare a scan that is not completed.");

        if (RednessScore is null || TearFilmMetrics is null)
            throw new InvalidOperationException("Cannot compare a scan without results.");

        var rednessDelta = RednessScore.Overall - baselineRednessScore.Overall;
        var tearFilmDelta = 0.0; // Comparison is based on redness; tear film delta can be extended
        Comparison = new ScanComparison(baselineScanId, rednessDelta, tearFilmDelta);
    }
}
