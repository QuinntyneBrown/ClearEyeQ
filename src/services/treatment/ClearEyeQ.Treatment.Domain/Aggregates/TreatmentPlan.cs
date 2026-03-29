using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using ClearEyeQ.Treatment.Domain.Entities;
using ClearEyeQ.Treatment.Domain.Enums;
using ClearEyeQ.Treatment.Domain.Events;
using ClearEyeQ.Treatment.Domain.ValueObjects;

namespace ClearEyeQ.Treatment.Domain.Aggregates;

public sealed class TreatmentPlan : AggregateRoot
{
    private readonly List<TreatmentPhase> _phases = [];
    private readonly List<EfficacyMeasurement> _efficacyMeasurements = [];

    public Guid PlanId => Id;
    public UserId UserId { get; private set; }
    private TenantId _tenantId;
    public override TenantId TenantId => _tenantId;
    public override PartitionKey PartitionKey => PartitionKey.ForUserInTenant(_tenantId, UserId);
    public Guid DiagnosisId { get; private set; }
    public TreatmentStatus Status { get; private set; }
    public IReadOnlyList<TreatmentPhase> Phases => _phases.AsReadOnly();
    public IReadOnlyList<EfficacyMeasurement> EfficacyMeasurements => _efficacyMeasurements.AsReadOnly();
    public EscalationRule? EscalationRule { get; private set; }
    public DateTimeOffset? ActivatedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    private TreatmentPlan() { }

    public static TreatmentPlan Propose(
        UserId userId,
        TenantId tenantId,
        Guid diagnosisId,
        EscalationRule? escalationRule = null)
    {
        var plan = new TreatmentPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            _tenantId = tenantId,
            DiagnosisId = diagnosisId,
            Status = TreatmentStatus.PendingApproval,
            EscalationRule = escalationRule,
            Audit = AuditMetadata.Create("system")
        };

        plan.AddDomainEvent(new TreatmentPlanProposedEvent
        {
            PlanId = plan.Id,
            UserId = userId,
            TenantId = tenantId,
            DiagnosisId = diagnosisId
        });

        return plan;
    }

    public void Activate()
    {
        if (Status is not TreatmentStatus.PendingApproval)
            throw new InvalidOperationException($"Cannot activate plan in {Status} status.");

        Status = TreatmentStatus.Active;
        ActivatedAt = DateTimeOffset.UtcNow;
        Audit = Audit.WithModification("clinician");

        AddDomainEvent(new TreatmentPlanActivatedEvent
        {
            PlanId = Id,
            UserId = UserId,
            TenantId = _tenantId
        });
    }

    public void Reject(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (Status is not (TreatmentStatus.PendingApproval or TreatmentStatus.PendingAdjustmentApproval))
            throw new InvalidOperationException($"Cannot reject plan in {Status} status.");

        Status = TreatmentStatus.Rejected;
        RejectionReason = reason;
        Audit = Audit.WithModification("clinician");
    }

    public void AddPhase(TreatmentPhase phase)
    {
        ArgumentNullException.ThrowIfNull(phase);

        if (Status is TreatmentStatus.Rejected or TreatmentStatus.Resolved)
            throw new InvalidOperationException($"Cannot add phase to plan in {Status} status.");

        _phases.Add(phase);
    }

    public void RecordEfficacy(double rednessScore, double baselineScore)
    {
        if (Status is not (TreatmentStatus.Active or TreatmentStatus.Maintenance))
            throw new InvalidOperationException($"Cannot record efficacy for plan in {Status} status.");

        var measurement = EfficacyMeasurement.Record(rednessScore, baselineScore);
        _efficacyMeasurements.Add(measurement);
    }

    public bool EvaluateEfficacy()
    {
        if (EscalationRule is null || _efficacyMeasurements.Count == 0 || ActivatedAt is null)
            return false;

        var daysSinceActivation = (DateTimeOffset.UtcNow - ActivatedAt.Value).TotalDays;
        if (daysSinceActivation < EscalationRule.DaysThreshold)
            return false;

        var latestMeasurement = _efficacyMeasurements
            .OrderByDescending(m => m.MeasuredAt)
            .First();

        return latestMeasurement.DeltaPercent < EscalationRule.MinImprovementPercent;
    }

    public void ProposeAdjustment(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (Status is not TreatmentStatus.Active)
            throw new InvalidOperationException($"Cannot propose adjustment for plan in {Status} status.");

        Status = TreatmentStatus.PendingAdjustmentApproval;
        Audit = Audit.WithModification("system");

        AddDomainEvent(new TreatmentAdjustmentProposedEvent
        {
            PlanId = Id,
            UserId = UserId,
            TenantId = _tenantId,
            Reason = reason
        });
    }

    public void ApplyAdjustment(Guid interventionId, string adjustmentDescription)
    {
        if (Status is not TreatmentStatus.PendingAdjustmentApproval)
            throw new InvalidOperationException($"Cannot apply adjustment for plan in {Status} status.");

        Status = TreatmentStatus.Active;
        Audit = Audit.WithModification("clinician");

        AddDomainEvent(new InterventionAdjustedEvent
        {
            PlanId = Id,
            InterventionId = interventionId,
            TenantId = _tenantId,
            AdjustmentDescription = adjustmentDescription
        });
    }

    public void Escalate()
    {
        if (Status is not TreatmentStatus.Active)
            throw new InvalidOperationException($"Cannot escalate plan in {Status} status.");

        var latestMeasurement = _efficacyMeasurements
            .OrderByDescending(m => m.MeasuredAt)
            .FirstOrDefault();

        Status = TreatmentStatus.EscalationRecommended;
        Audit = Audit.WithModification("system");

        AddDomainEvent(new EscalationRecommendedEvent
        {
            PlanId = Id,
            UserId = UserId,
            TenantId = _tenantId,
            RecommendedAction = EscalationRule?.Action ?? "Specialist referral",
            CurrentImprovementPercent = latestMeasurement?.DeltaPercent ?? 0.0
        });
    }

    public void VerifyResolution()
    {
        if (Status is not TreatmentStatus.Active)
            throw new InvalidOperationException($"Cannot verify resolution for plan in {Status} status.");

        var latestMeasurements = _efficacyMeasurements
            .OrderByDescending(m => m.MeasuredAt)
            .Take(3)
            .ToList();

        if (latestMeasurements.Count < 1)
            throw new InvalidOperationException("No efficacy measurements available for resolution verification.");

        var averageDelta = latestMeasurements.Average(m => m.DeltaPercent);

        if (averageDelta >= 80.0)
        {
            Status = TreatmentStatus.Resolved;
            Audit = Audit.WithModification("system");
        }
    }

    public void TransitionToMaintenance()
    {
        if (Status is not TreatmentStatus.Resolved)
            throw new InvalidOperationException($"Cannot transition to maintenance from {Status} status.");

        Status = TreatmentStatus.Maintenance;
        Audit = Audit.WithModification("system");
    }
}
