using ClearEyeQ.Diagnostic.Application.Interfaces;
using ClearEyeQ.Diagnostic.Application.Queries.GetDiagnosticSession;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Diagnostic.Application.Queries.GetCausalGraph;

public sealed class GetCausalGraphHandler : IRequestHandler<GetCausalGraphQuery, CausalGraphDetailDto?>
{
    private readonly IDiagnosticSessionRepository _repository;

    public GetCausalGraphHandler(IDiagnosticSessionRepository repository)
    {
        _repository = repository;
    }

    public async Task<CausalGraphDetailDto?> Handle(GetCausalGraphQuery request, CancellationToken ct)
    {
        var tenantId = new TenantId(request.TenantId);
        var session = await _repository.GetBySessionIdAsync(request.SessionId, tenantId, ct);

        if (session?.CausalGraph is null)
            return null;

        var graph = session.CausalGraph;

        return new CausalGraphDetailDto(
            session.SessionId,
            graph.Nodes.Select(n => new CausalFactorDto(
                n.FactorId, n.Label, n.CausalCategory.ToString(), n.Weight)).ToList(),
            graph.Edges.Select(e => new CausalRelationDto(
                e.SourceId, e.TargetId, e.Strength)).ToList(),
            graph.Nodes.Count,
            graph.Edges.Count);
    }
}
