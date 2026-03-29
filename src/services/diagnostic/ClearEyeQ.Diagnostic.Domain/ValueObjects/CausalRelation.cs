namespace ClearEyeQ.Diagnostic.Domain.ValueObjects;

public sealed record CausalRelation(string SourceId, string TargetId, double Strength);
