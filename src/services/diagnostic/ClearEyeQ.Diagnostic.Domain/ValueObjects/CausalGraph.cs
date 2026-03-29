using ClearEyeQ.Diagnostic.Domain.Entities;

namespace ClearEyeQ.Diagnostic.Domain.ValueObjects;

public sealed record CausalGraph(
    List<CausalFactor> Nodes,
    List<CausalRelation> Edges);
