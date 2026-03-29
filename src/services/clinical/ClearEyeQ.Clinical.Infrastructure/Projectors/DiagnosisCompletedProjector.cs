using ClearEyeQ.Clinical.Application.Interfaces;
using ClearEyeQ.Clinical.Application.ReadModels;
using ClearEyeQ.SharedKernel.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ClearEyeQ.Clinical.Infrastructure.Projectors;

/// <summary>
/// Subscribes to DiagnosisCompleted integration events and updates
/// the DiagnosticReadModel and PatientSummary projections.
/// </summary>
public sealed class DiagnosisCompletedProjector : InboxConsumer<DiagnosisCompletedProjector.DiagnosisCompletedEvent>
{
    private readonly IPatientReadModelStore _store;

    public DiagnosisCompletedProjector(
        IPatientReadModelStore store,
        IConnectionMultiplexer redis,
        ILogger<DiagnosisCompletedProjector> logger)
        : base(redis, logger)
    {
        _store = store;
    }

    protected override async Task HandleAsync(DiagnosisCompletedEvent message, CancellationToken ct)
    {
        var diagnostic = new DiagnosticReadModel
        {
            TenantId = message.TenantId,
            PatientId = message.PatientId,
            DiagnosticSessionId = message.DiagnosticSessionId,
            PrimaryDiagnosis = message.PrimaryDiagnosis,
            Severity = message.Severity,
            ConfidenceScore = message.ConfidenceScore,
            CausalFactors = message.CausalFactors,
            CompletedAtUtc = message.CompletedAtUtc
        };

        await _store.UpsertDiagnosticAsync(diagnostic, ct);

        var summary = new PatientSummaryReadModel
        {
            TenantId = message.TenantId,
            PatientId = message.PatientId,
            Name = string.Empty,
            LatestDiagnosis = message.PrimaryDiagnosis,
            Status = "Active",
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        await _store.UpsertPatientSummaryAsync(summary, ct);
    }

    public sealed record DiagnosisCompletedEvent(
        Guid TenantId,
        Guid PatientId,
        Guid DiagnosticSessionId,
        string PrimaryDiagnosis,
        string Severity,
        double ConfidenceScore,
        string CausalFactors,
        DateTimeOffset CompletedAtUtc);
}
