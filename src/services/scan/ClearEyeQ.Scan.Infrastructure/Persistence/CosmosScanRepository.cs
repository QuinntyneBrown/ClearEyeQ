using System.Net;
using System.Text.Json;
using ClearEyeQ.Scan.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using Microsoft.Azure.Cosmos;

namespace ClearEyeQ.Scan.Infrastructure.Persistence;

public sealed class CosmosScanRepository : IScanRepository
{
    private readonly Container _container;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public CosmosScanRepository(CosmosClient cosmosClient, string databaseName)
    {
        _container = cosmosClient.GetContainer(databaseName, "scans");
    }

    public async Task<Domain.Aggregates.Scan?> GetByIdAsync(
        ScanId scanId,
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Query by ScanId since it differs from the document Id
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.scanId.value = @scanId")
                .WithParameter("@scanId", scanId.Value.ToString());

            var iterator = _container.GetItemQueryIterator<ScanDocument>(
                query,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new Microsoft.Azure.Cosmos.PartitionKey(
                        $"{tenantId.Value}|*")
                });

            // Try without partition key filter to search across user partitions
            iterator = _container.GetItemQueryIterator<ScanDocument>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                var doc = response.FirstOrDefault();
                if (doc is not null)
                {
                    return doc.ToDomain();
                }
            }

            return null;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task SaveAsync(Domain.Aggregates.Scan scan, CancellationToken cancellationToken = default)
    {
        var document = ScanDocument.FromDomain(scan);
        await _container.CreateItemAsync(
            document,
            new Microsoft.Azure.Cosmos.PartitionKey(scan.PartitionKey.Value),
            cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(Domain.Aggregates.Scan scan, CancellationToken cancellationToken = default)
    {
        var document = ScanDocument.FromDomain(scan);
        await _container.ReplaceItemAsync(
            document,
            scan.Id.ToString(),
            new Microsoft.Azure.Cosmos.PartitionKey(scan.PartitionKey.Value),
            cancellationToken: cancellationToken);
    }
}

/// <summary>
/// Cosmos DB document representation of a Scan aggregate.
/// Handles serialization/deserialization between domain and persistence models.
/// </summary>
internal sealed class ScanDocument
{
    public string id { get; set; } = string.Empty;
    public string partitionKey { get; set; } = string.Empty;
    public ScanIdDoc scanId { get; set; } = new();
    public UserIdDoc userId { get; set; } = new();
    public TenantIdDoc tenantId { get; set; } = new();
    public string eyeSide { get; set; } = string.Empty;
    public string status { get; set; } = string.Empty;
    public CaptureMetadataDoc captureMetadata { get; set; } = new();
    public List<ScanImageDoc> images { get; set; } = [];
    public RednessScoreDoc? rednessScore { get; set; }
    public TearFilmMetricsDoc? tearFilmMetrics { get; set; }
    public ScanComparisonDoc? comparison { get; set; }
    public DateTimeOffset createdAt { get; set; }
    public string? failureReason { get; set; }
    public AuditMetadataDoc audit { get; set; } = new();

    public static ScanDocument FromDomain(Domain.Aggregates.Scan scan)
    {
        var doc = new ScanDocument
        {
            id = scan.Id.ToString(),
            partitionKey = scan.PartitionKey.Value,
            scanId = new ScanIdDoc { value = scan.ScanId.Value.ToString() },
            userId = new UserIdDoc { value = scan.UserId.Value.ToString() },
            tenantId = new TenantIdDoc { value = scan.TenantId.Value.ToString() },
            eyeSide = scan.EyeSide.ToString(),
            status = scan.Status.ToString(),
            captureMetadata = new CaptureMetadataDoc
            {
                deviceModel = scan.CaptureMetadata.DeviceModel,
                frameCount = scan.CaptureMetadata.FrameCount,
                burstDurationMs = scan.CaptureMetadata.BurstDuration.TotalMilliseconds,
                ambientLightLux = scan.CaptureMetadata.AmbientLightLux
            },
            images = scan.Images.Select(i => new ScanImageDoc
            {
                id = i.Id.ToString(),
                frameIndex = i.FrameIndex,
                blobUri = i.BlobUri,
                qualityScore = i.QualityScore,
                isSelected = i.IsSelected
            }).ToList(),
            createdAt = scan.CreatedAt,
            failureReason = scan.FailureReason,
            audit = new AuditMetadataDoc
            {
                createdAt = scan.Audit.CreatedAt,
                createdBy = scan.Audit.CreatedBy,
                modifiedAt = scan.Audit.ModifiedAt,
                modifiedBy = scan.Audit.ModifiedBy
            }
        };

        if (scan.RednessScore is not null)
        {
            doc.rednessScore = new RednessScoreDoc
            {
                overall = scan.RednessScore.Overall,
                confidence = scan.RednessScore.Confidence,
                zoneScores = new Dictionary<string, double>(scan.RednessScore.ZoneScores)
            };
        }

        if (scan.TearFilmMetrics is not null)
        {
            doc.tearFilmMetrics = new TearFilmMetricsDoc
            {
                breakUpTime = scan.TearFilmMetrics.BreakUpTime,
                lipidLayerGrade = scan.TearFilmMetrics.LipidLayerGrade,
                coveragePercentage = scan.TearFilmMetrics.CoveragePercentage
            };
        }

        if (scan.Comparison is not null)
        {
            doc.comparison = new ScanComparisonDoc
            {
                baselineScanId = scan.Comparison.BaselineScanId.Value.ToString(),
                rednessDelta = scan.Comparison.RednessDelta,
                tearFilmDelta = scan.Comparison.TearFilmDelta
            };
        }

        return doc;
    }

    public Domain.Aggregates.Scan ToDomain()
    {
        var tenantIdVal = new TenantId(Guid.Parse(tenantId.value));
        var userIdVal = new UserId(Guid.Parse(userId.value));
        var scanIdVal = new ScanId(Guid.Parse(scanId.value));
        var eyeSideVal = Enum.Parse<Domain.Enums.EyeSide>(eyeSide);
        var statusVal = Enum.Parse<Domain.Enums.ScanStatus>(status);

        var metadata = new Domain.ValueObjects.CaptureMetadata(
            captureMetadata.deviceModel,
            captureMetadata.frameCount,
            TimeSpan.FromMilliseconds(captureMetadata.burstDurationMs),
            captureMetadata.ambientLightLux);

        var scan = Domain.Aggregates.Scan.Initiate(userIdVal, tenantIdVal, eyeSideVal, metadata);

        // Use reflection to restore persisted state since the aggregate is designed for
        // event-driven state transitions, not direct property setting
        var type = typeof(Domain.Aggregates.Scan);

        type.BaseType!.GetProperty("Id")!.SetValue(scan, Guid.Parse(id));
        type.GetProperty("ScanId")!.GetSetMethod(true)!.Invoke(scan, [scanIdVal]);
        type.GetProperty("Status")!.GetSetMethod(true)!.Invoke(scan, [statusVal]);
        type.GetProperty("CreatedAt")!.GetSetMethod(true)!.Invoke(scan, [createdAt]);
        type.GetProperty("FailureReason")!.GetSetMethod(true)!.Invoke(scan, [failureReason]);

        scan.Audit = new AuditMetadata
        {
            CreatedAt = audit.createdAt,
            CreatedBy = audit.createdBy,
            ModifiedAt = audit.modifiedAt,
            ModifiedBy = audit.modifiedBy
        };

        // Restore images
        foreach (var img in images)
        {
            var scanImage = new Domain.Entities.ScanImage(img.frameIndex, img.blobUri, img.qualityScore);
            var imgType = typeof(Domain.Entities.ScanImage);
            imgType.GetProperty("Id")!.GetSetMethod(true)!.Invoke(scanImage, [Guid.Parse(img.id)]);
            if (img.isSelected) scanImage.Select();
            scan.AddImage(scanImage);
        }

        // Restore status after adding images (which changes status to Capturing)
        type.GetProperty("Status")!.GetSetMethod(true)!.Invoke(scan, [statusVal]);

        // Restore redness score
        if (rednessScore is not null)
        {
            var rs = new Domain.ValueObjects.RednessScore(
                rednessScore.overall,
                rednessScore.confidence,
                rednessScore.zoneScores);
            type.GetProperty("RednessScore")!.GetSetMethod(true)!.Invoke(scan, [rs]);
        }

        // Restore tear film metrics
        if (tearFilmMetrics is not null)
        {
            var tf = new Domain.ValueObjects.TearFilmMetrics(
                tearFilmMetrics.breakUpTime,
                tearFilmMetrics.lipidLayerGrade,
                tearFilmMetrics.coveragePercentage);
            type.GetProperty("TearFilmMetrics")!.GetSetMethod(true)!.Invoke(scan, [tf]);
        }

        // Restore comparison
        if (comparison is not null)
        {
            var comp = new Domain.ValueObjects.ScanComparison(
                new ScanId(Guid.Parse(comparison.baselineScanId)),
                comparison.rednessDelta,
                comparison.tearFilmDelta);
            type.GetProperty("Comparison")!.GetSetMethod(true)!.Invoke(scan, [comp]);
        }

        // Clear any domain events raised during reconstitution
        scan.ClearDomainEvents();

        return scan;
    }
}

internal sealed class ScanIdDoc { public string value { get; set; } = string.Empty; }
internal sealed class UserIdDoc { public string value { get; set; } = string.Empty; }
internal sealed class TenantIdDoc { public string value { get; set; } = string.Empty; }

internal sealed class CaptureMetadataDoc
{
    public string deviceModel { get; set; } = string.Empty;
    public int frameCount { get; set; }
    public double burstDurationMs { get; set; }
    public double ambientLightLux { get; set; }
}

internal sealed class ScanImageDoc
{
    public string id { get; set; } = string.Empty;
    public int frameIndex { get; set; }
    public string blobUri { get; set; } = string.Empty;
    public double qualityScore { get; set; }
    public bool isSelected { get; set; }
}

internal sealed class RednessScoreDoc
{
    public double overall { get; set; }
    public double confidence { get; set; }
    public Dictionary<string, double> zoneScores { get; set; } = [];
}

internal sealed class TearFilmMetricsDoc
{
    public double breakUpTime { get; set; }
    public string lipidLayerGrade { get; set; } = string.Empty;
    public double coveragePercentage { get; set; }
}

internal sealed class ScanComparisonDoc
{
    public string baselineScanId { get; set; } = string.Empty;
    public double rednessDelta { get; set; }
    public double tearFilmDelta { get; set; }
}

internal sealed class AuditMetadataDoc
{
    public DateTimeOffset createdAt { get; set; }
    public string createdBy { get; set; } = string.Empty;
    public DateTimeOffset? modifiedAt { get; set; }
    public string? modifiedBy { get; set; }
}
