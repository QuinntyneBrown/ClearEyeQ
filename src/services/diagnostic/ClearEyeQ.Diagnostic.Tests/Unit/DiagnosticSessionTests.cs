using ClearEyeQ.Diagnostic.Domain.Aggregates;
using ClearEyeQ.Diagnostic.Domain.Entities;
using ClearEyeQ.Diagnostic.Domain.Enums;
using ClearEyeQ.Diagnostic.Domain.Events;
using ClearEyeQ.Diagnostic.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace ClearEyeQ.Diagnostic.Tests.Unit;

public sealed class DiagnosticSessionTests
{
    private static DiagnosticSession CreateSession()
    {
        return DiagnosticSession.Create(
            ScanId.New(),
            UserId.New(),
            TenantId.New());
    }

    private static Diagnosis CreateDiagnosis(
        string code = "H10.1",
        string name = "Allergic Conjunctivitis",
        double confidence = 0.85,
        Severity severity = Severity.Moderate)
    {
        return new Diagnosis(
            code,
            name,
            new ConfidenceScore(confidence),
            severity,
            [new EvidenceReference("Scan", "redness-zone-1", "High redness in temporal zone")]);
    }

    [Fact]
    public void Create_ShouldInitializeWithCorrectState()
    {
        var scanId = ScanId.New();
        var userId = UserId.New();
        var tenantId = TenantId.New();

        var session = DiagnosticSession.Create(scanId, userId, tenantId);

        session.ScanId.Should().Be(scanId);
        session.UserId.Should().Be(userId);
        session.TenantId.Should().Be(tenantId);
        session.Status.Should().Be(DiagnosticStatus.Initiated);
        session.Diagnoses.Should().BeEmpty();
        session.CausalGraph.Should().BeNull();
        session.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        session.SessionId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void AddDiagnosis_ShouldAddAndSetStatusToAnalyzing()
    {
        var session = CreateSession();
        var diagnosis = CreateDiagnosis();

        session.AddDiagnosis(diagnosis);

        session.Diagnoses.Should().HaveCount(1);
        session.Status.Should().Be(DiagnosticStatus.Analyzing);
    }

    [Fact]
    public void AddDiagnosis_WhenCompleted_ShouldThrow()
    {
        var session = CreateSession();
        session.AddDiagnosis(CreateDiagnosis());
        session.Complete();

        var act = () => session.AddDiagnosis(CreateDiagnosis("H10.0", "Bacterial Conjunctivitis"));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*completed*");
    }

    [Fact]
    public void SetCausalGraph_ShouldSetGraph()
    {
        var session = CreateSession();
        var graph = new CausalGraph(
            [new CausalFactor("f1", "Pollen", CausalCategory.Environmental, 0.8)],
            [new CausalRelation("f1", "d1", 0.9)]);

        session.SetCausalGraph(graph);

        session.CausalGraph.Should().NotBeNull();
        session.CausalGraph!.Nodes.Should().HaveCount(1);
        session.CausalGraph.Edges.Should().HaveCount(1);
    }

    [Fact]
    public void SetCausalGraph_WhenCompleted_ShouldThrow()
    {
        var session = CreateSession();
        session.AddDiagnosis(CreateDiagnosis());
        session.Complete();

        var graph = new CausalGraph([], []);
        var act = () => session.SetCausalGraph(graph);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*completed*");
    }

    [Fact]
    public void Complete_ShouldSetStatusAndRaiseEvent()
    {
        var session = CreateSession();
        session.AddDiagnosis(CreateDiagnosis());

        session.Complete();

        session.Status.Should().Be(DiagnosticStatus.Completed);
        session.DomainEvents.Should().HaveCount(1);
        session.DomainEvents[0].Should().BeOfType<DiagnosisCompletedEvent>();

        var evt = (DiagnosisCompletedEvent)session.DomainEvents[0];
        evt.SessionId.Should().Be(session.SessionId);
        evt.TopConditions.Should().HaveCount(1);
    }

    [Fact]
    public void Complete_WithoutDiagnoses_ShouldThrow()
    {
        var session = CreateSession();

        var act = () => session.Complete();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*at least one diagnosis*");
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldThrow()
    {
        var session = CreateSession();
        session.AddDiagnosis(CreateDiagnosis());
        session.Complete();

        var act = () => session.Complete();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already completed*");
    }

    [Fact]
    public void MarkFailed_ShouldSetStatusToFailed()
    {
        var session = CreateSession();

        session.MarkFailed();

        session.Status.Should().Be(DiagnosticStatus.Failed);
    }

    [Fact]
    public void Complete_ShouldReturnTopFiveConditionsOrderedByConfidence()
    {
        var session = CreateSession();

        for (int i = 1; i <= 7; i++)
        {
            session.AddDiagnosis(CreateDiagnosis(
                code: $"H{i:D2}",
                name: $"Condition {i}",
                confidence: i * 0.1));
        }

        session.Complete();

        var evt = (DiagnosisCompletedEvent)session.DomainEvents[0];
        evt.TopConditions.Should().HaveCount(5);
        evt.TopConditions[0].Confidence.Should().BeApproximately(0.7, 0.001);
        evt.TopConditions[4].Confidence.Should().BeApproximately(0.3, 0.001);
    }

    [Fact]
    public void PartitionKey_ShouldBeTenantPipeUser()
    {
        var session = CreateSession();

        session.PartitionKey.Value.Should().Contain("|");
        session.PartitionKey.Value.Should().StartWith(session.TenantId.Value.ToString());
    }
}
