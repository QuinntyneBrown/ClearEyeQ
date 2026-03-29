namespace ClearEyeQ.Diagnostic.Application.Queries.GetDiagnosticSession;

public sealed record DiagnosticSessionDto(
    Guid SessionId,
    Guid UserId,
    Guid TenantId,
    Guid ScanId,
    string Status,
    DateTimeOffset CreatedAt,
    List<DiagnosisDto> Diagnoses,
    CausalGraphDto? CausalGraph);

public sealed record DiagnosisDto(
    string ConditionCode,
    string ConditionName,
    double ConfidenceScore,
    string Severity,
    List<EvidenceReferenceDto> EvidenceReferences);

public sealed record EvidenceReferenceDto(string Source, string Key, string Description);

public sealed record CausalGraphDto(
    List<CausalFactorDto> Nodes,
    List<CausalRelationDto> Edges);

public sealed record CausalFactorDto(string FactorId, string Label, string Category, double Weight);
public sealed record CausalRelationDto(string SourceId, string TargetId, double Strength);
