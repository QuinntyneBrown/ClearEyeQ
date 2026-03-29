using ClearEyeQ.Diagnostic.Application.Queries.GetDiagnosticSession;

namespace ClearEyeQ.Diagnostic.Application.Queries.GetCausalGraph;

public sealed record CausalGraphDetailDto(
    Guid SessionId,
    List<CausalFactorDto> Nodes,
    List<CausalRelationDto> Edges,
    int NodeCount,
    int EdgeCount);
