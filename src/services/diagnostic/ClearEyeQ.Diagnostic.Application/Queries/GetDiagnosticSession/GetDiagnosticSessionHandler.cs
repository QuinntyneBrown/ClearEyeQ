using ClearEyeQ.Diagnostic.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Diagnostic.Application.Queries.GetDiagnosticSession;

public sealed class GetDiagnosticSessionHandler : IRequestHandler<GetDiagnosticSessionQuery, DiagnosticSessionDto?>
{
    private readonly IDiagnosticSessionRepository _repository;

    public GetDiagnosticSessionHandler(IDiagnosticSessionRepository repository)
    {
        _repository = repository;
    }

    public async Task<DiagnosticSessionDto?> Handle(GetDiagnosticSessionQuery request, CancellationToken ct)
    {
        var tenantId = new TenantId(request.TenantId);
        var session = await _repository.GetBySessionIdAsync(request.SessionId, tenantId, ct);

        if (session is null)
            return null;

        return new DiagnosticSessionDto(
            session.SessionId,
            session.UserId,
            tenantId,
            session.ScanId,
            session.Status.ToString(),
            session.CreatedAt,
            session.Diagnoses.Select(d => new DiagnosisDto(
                d.ConditionCode,
                d.ConditionName,
                d.ConfidenceScore.Value,
                d.Severity.ToString(),
                d.EvidenceReferences.Select(e => new EvidenceReferenceDto(e.Source, e.Key, e.Description)).ToList()
            )).ToList(),
            session.CausalGraph is not null
                ? new CausalGraphDto(
                    session.CausalGraph.Nodes.Select(n => new CausalFactorDto(
                        n.FactorId, n.Label, n.CausalCategory.ToString(), n.Weight)).ToList(),
                    session.CausalGraph.Edges.Select(e => new CausalRelationDto(
                        e.SourceId, e.TargetId, e.Strength)).ToList())
                : null);
    }
}
