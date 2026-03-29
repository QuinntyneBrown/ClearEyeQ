using ClearEyeQ.Clinical.Application.Interfaces;
using ClearEyeQ.Clinical.Application.ReadModels;
using ClearEyeQ.SharedKernel.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ClearEyeQ.Clinical.Infrastructure.Projectors;

/// <summary>
/// Subscribes to TreatmentPlanProposed integration events and creates
/// TreatmentPlanReadModel entries for clinician review.
/// </summary>
public sealed class TreatmentPlanProposedProjector : InboxConsumer<TreatmentPlanProposedProjector.TreatmentPlanProposedEvent>
{
    private readonly IPatientReadModelStore _store;

    public TreatmentPlanProposedProjector(
        IPatientReadModelStore store,
        IConnectionMultiplexer redis,
        ILogger<TreatmentPlanProposedProjector> logger)
        : base(redis, logger)
    {
        _store = store;
    }

    protected override async Task HandleAsync(TreatmentPlanProposedEvent message, CancellationToken ct)
    {
        var plan = new TreatmentPlanReadModel
        {
            TenantId = message.TenantId,
            PatientId = message.PatientId,
            TreatmentPlanId = message.TreatmentPlanId,
            Status = "Proposed",
            InterventionSummary = message.InterventionSummary,
            Rationale = message.Rationale,
            ProposedAtUtc = message.ProposedAtUtc
        };

        await _store.UpsertTreatmentPlanAsync(plan, ct);

        var summary = new PatientSummaryReadModel
        {
            TenantId = message.TenantId,
            PatientId = message.PatientId,
            Name = string.Empty,
            ActiveTreatment = $"Proposed: {message.InterventionSummary}",
            Status = "Active",
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        await _store.UpsertPatientSummaryAsync(summary, ct);
    }

    public sealed record TreatmentPlanProposedEvent(
        Guid TenantId,
        Guid PatientId,
        Guid TreatmentPlanId,
        string InterventionSummary,
        string Rationale,
        DateTimeOffset ProposedAtUtc);
}
