using ClearEyeQ.Diagnostic.Domain.Entities;
using ClearEyeQ.Diagnostic.Domain.Enums;
using ClearEyeQ.Diagnostic.Domain.Events;
using ClearEyeQ.Diagnostic.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Diagnostic.Domain.Aggregates;

public sealed class DiagnosticSession : AggregateRoot
{
    public Guid SessionId => Id;
    public UserId UserId { get; private set; }
    private TenantId _tenantId;
    public override TenantId TenantId => _tenantId;
    public override PartitionKey PartitionKey => PartitionKey.ForUserInTenant(_tenantId, UserId);
    public ScanId ScanId { get; private set; }
    public DiagnosticStatus Status { get; private set; }
    public List<Diagnosis> Diagnoses { get; private set; } = [];
    public CausalGraph? CausalGraph { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private DiagnosticSession() { }

    public static DiagnosticSession Create(ScanId scanId, UserId userId, TenantId tenantId)
    {
        var session = new DiagnosticSession
        {
            Id = Guid.NewGuid(),
            ScanId = scanId,
            UserId = userId,
            _tenantId = tenantId,
            Status = DiagnosticStatus.Initiated,
            CreatedAt = DateTimeOffset.UtcNow,
            Audit = AuditMetadata.Create(userId.ToString())
        };

        return session;
    }

    public void AddDiagnosis(Diagnosis diagnosis)
    {
        ArgumentNullException.ThrowIfNull(diagnosis);

        if (Status == DiagnosticStatus.Completed)
            throw new InvalidOperationException("Cannot add diagnosis to a completed session.");

        Status = DiagnosticStatus.Analyzing;
        Diagnoses.Add(diagnosis);
    }

    public void SetCausalGraph(CausalGraph causalGraph)
    {
        ArgumentNullException.ThrowIfNull(causalGraph);

        if (Status == DiagnosticStatus.Completed)
            throw new InvalidOperationException("Cannot set causal graph on a completed session.");

        CausalGraph = causalGraph;
    }

    public void Complete()
    {
        if (Status == DiagnosticStatus.Completed)
            throw new InvalidOperationException("Session is already completed.");

        if (Diagnoses.Count == 0)
            throw new InvalidOperationException("Cannot complete session without at least one diagnosis.");

        Status = DiagnosticStatus.Completed;
        Audit = Audit.WithModification(UserId.ToString());

        AddDomainEvent(new DiagnosisCompletedEvent
        {
            SessionId = SessionId,
            UserId = UserId,
            TenantId = _tenantId,
            TopConditions = Diagnoses
                .OrderByDescending(d => d.ConfidenceScore.Value)
                .Take(5)
                .Select(d => new DiagnosisCompletedEvent.TopCondition(
                    d.ConditionCode,
                    d.ConditionName,
                    d.ConfidenceScore.Value,
                    d.Severity))
                .ToList()
        });
    }

    public void MarkFailed()
    {
        Status = DiagnosticStatus.Failed;
        Audit = Audit.WithModification(UserId.ToString());
    }
}
