using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using ClearEyeQ.Treatment.Domain.Aggregates;
using ClearEyeQ.Treatment.Domain.Entities;
using ClearEyeQ.Treatment.Domain.Enums;
using ClearEyeQ.Treatment.Domain.Events;
using ClearEyeQ.Treatment.Domain.ValueObjects;
using FluentAssertions;

namespace ClearEyeQ.Treatment.Tests.Unit;

public sealed class TreatmentPlanTests
{
    private readonly UserId _userId = UserId.New();
    private readonly TenantId _tenantId = TenantId.New();
    private readonly Guid _diagnosisId = Guid.NewGuid();

    [Fact]
    public void Propose_ShouldCreatePlanInPendingApprovalStatus()
    {
        var plan = TreatmentPlan.Propose(_userId, _tenantId, _diagnosisId);

        plan.Status.Should().Be(TreatmentStatus.PendingApproval);
        plan.UserId.Should().Be(_userId);
        plan.TenantId.Should().Be(_tenantId);
        plan.DiagnosisId.Should().Be(_diagnosisId);
        plan.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TreatmentPlanProposedEvent>();
    }

    [Fact]
    public void Activate_FromPendingApproval_ShouldTransitionToActive()
    {
        var plan = TreatmentPlan.Propose(_userId, _tenantId, _diagnosisId);
        plan.ClearDomainEvents();

        plan.Activate();

        plan.Status.Should().Be(TreatmentStatus.Active);
        plan.ActivatedAt.Should().NotBeNull();
        plan.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TreatmentPlanActivatedEvent>();
    }

    [Fact]
    public void Activate_FromActiveStatus_ShouldThrow()
    {
        var plan = TreatmentPlan.Propose(_userId, _tenantId, _diagnosisId);
        plan.Activate();

        var act = () => plan.Activate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reject_ShouldTransitionToRejected()
    {
        var plan = TreatmentPlan.Propose(_userId, _tenantId, _diagnosisId);

        plan.Reject("Not suitable for patient");

        plan.Status.Should().Be(TreatmentStatus.Rejected);
        plan.RejectionReason.Should().Be("Not suitable for patient");
    }

    [Fact]
    public void AddPhase_ShouldAddPhaseToCollection()
    {
        var plan = TreatmentPlan.Propose(_userId, _tenantId, _diagnosisId);
        var phase = TreatmentPhase.Create(1, TimeSpan.FromDays(30));

        plan.AddPhase(phase);

        plan.Phases.Should().HaveCount(1);
        plan.Phases[0].PhaseNumber.Should().Be(1);
    }

    [Fact]
    public void RecordEfficacy_WhenActive_ShouldAddMeasurement()
    {
        var plan = TreatmentPlan.Propose(_userId, _tenantId, _diagnosisId);
        plan.Activate();

        plan.RecordEfficacy(rednessScore: 3.0, baselineScore: 8.0);

        plan.EfficacyMeasurements.Should().HaveCount(1);
        plan.EfficacyMeasurements[0].DeltaPercent.Should().Be(62.5);
    }

    [Fact]
    public void RecordEfficacy_WhenNotActive_ShouldThrow()
    {
        var plan = TreatmentPlan.Propose(_userId, _tenantId, _diagnosisId);

        var act = () => plan.RecordEfficacy(3.0, 8.0);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void EvaluateEfficacy_WhenBelowThreshold_ShouldReturnTrue()
    {
        var escalationRule = new EscalationRule(
            DaysThreshold: 0, // Immediate for testing
            MinImprovementPercent: 50.0,
            Action: "Refer to specialist");

        var plan = TreatmentPlan.Propose(_userId, _tenantId, _diagnosisId, escalationRule);
        plan.Activate();
        plan.RecordEfficacy(rednessScore: 7.0, baselineScore: 8.0); // Only 12.5% improvement

        var shouldEscalate = plan.EvaluateEfficacy();

        shouldEscalate.Should().BeTrue();
    }

    [Fact]
    public void ProposeAdjustment_WhenActive_ShouldTransitionToPendingAdjustmentApproval()
    {
        var plan = TreatmentPlan.Propose(_userId, _tenantId, _diagnosisId);
        plan.Activate();
        plan.ClearDomainEvents();

        plan.ProposeAdjustment("Insufficient efficacy with current dosage");

        plan.Status.Should().Be(TreatmentStatus.PendingAdjustmentApproval);
        plan.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TreatmentAdjustmentProposedEvent>();
    }

    [Fact]
    public void ApplyAdjustment_ShouldTransitionBackToActive()
    {
        var plan = TreatmentPlan.Propose(_userId, _tenantId, _diagnosisId);
        plan.Activate();
        plan.ProposeAdjustment("Need dose change");
        plan.ClearDomainEvents();

        var interventionId = Guid.NewGuid();
        plan.ApplyAdjustment(interventionId, "Increased dosage from 0.5mg to 1mg");

        plan.Status.Should().Be(TreatmentStatus.Active);
        plan.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<InterventionAdjustedEvent>();
    }

    [Fact]
    public void Escalate_ShouldTransitionToEscalationRecommended()
    {
        var escalationRule = new EscalationRule(0, 50.0, "Refer to ophthalmologist");
        var plan = TreatmentPlan.Propose(_userId, _tenantId, _diagnosisId, escalationRule);
        plan.Activate();
        plan.RecordEfficacy(7.0, 8.0);
        plan.ClearDomainEvents();

        plan.Escalate();

        plan.Status.Should().Be(TreatmentStatus.EscalationRecommended);
        plan.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<EscalationRecommendedEvent>();
    }

    [Fact]
    public void Intervention_Hierarchy_ShouldCreateCorrectTypes()
    {
        var medication = MedicationIntervention.Create(
            "Cyclosporine", "0.05%", "BID", "Ophthalmic", "Restasis eye drops");

        var behavioral = BehavioralIntervention.Create(
            BehavioralType.ScreenBreak, "Every 20 minutes", "20-20-20 rule");

        var environmental = EnvironmentalIntervention.Create(
            EnvironmentalTarget.Humidity, "Maintain 40-60% humidity", "Use humidifier");

        medication.InterventionType.Should().Be(InterventionType.Medication);
        medication.DrugName.Should().Be("Cyclosporine");

        behavioral.InterventionType.Should().Be(InterventionType.Behavioral);
        behavioral.BehavioralType.Should().Be(BehavioralType.ScreenBreak);

        environmental.InterventionType.Should().Be(InterventionType.Environmental);
        environmental.Target.Should().Be(EnvironmentalTarget.Humidity);
    }
}
