using System.Text.Json;
using ClearEyeQ.Clinical.Application.Interfaces;
using ClearEyeQ.Clinical.Application.ReadModels;
using ClearEyeQ.SharedKernel.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ClearEyeQ.Clinical.Infrastructure.Projectors;

/// <summary>
/// Subscribes to ScanCompleted integration events and updates
/// PatientSummary and ScanResult read models.
/// </summary>
public sealed class ScanCompletedProjector : InboxConsumer<ScanCompletedProjector.ScanCompletedEvent>
{
    private readonly IPatientReadModelStore _store;

    public ScanCompletedProjector(
        IPatientReadModelStore store,
        IConnectionMultiplexer redis,
        ILogger<ScanCompletedProjector> logger)
        : base(redis, logger)
    {
        _store = store;
    }

    protected override async Task HandleAsync(ScanCompletedEvent message, CancellationToken ct)
    {
        var scanResult = new ScanResultReadModel
        {
            TenantId = message.TenantId,
            PatientId = message.PatientId,
            ScanId = message.ScanId,
            EyeSide = message.EyeSide,
            RednessScore = message.RednessScore,
            TearFilmStability = message.TearFilmStability,
            ConfidenceScore = message.ConfidenceScore,
            CompletedAtUtc = message.CompletedAtUtc
        };

        await _store.UpsertScanResultAsync(scanResult, ct);

        var summary = new PatientSummaryReadModel
        {
            TenantId = message.TenantId,
            PatientId = message.PatientId,
            Name = message.PatientName,
            LastScanDate = message.CompletedAtUtc,
            RednessScore = message.RednessScore,
            Status = "Active",
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        await _store.UpsertPatientSummaryAsync(summary, ct);
    }

    public sealed record ScanCompletedEvent(
        Guid TenantId,
        Guid PatientId,
        string PatientName,
        Guid ScanId,
        string EyeSide,
        double RednessScore,
        double TearFilmStability,
        double ConfidenceScore,
        DateTimeOffset CompletedAtUtc);
}
