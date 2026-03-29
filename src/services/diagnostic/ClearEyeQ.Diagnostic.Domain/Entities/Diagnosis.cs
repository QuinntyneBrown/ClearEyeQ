using ClearEyeQ.Diagnostic.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Diagnostic.Domain.Entities;

public sealed class Diagnosis
{
    public string ConditionCode { get; private set; }
    public string ConditionName { get; private set; }
    public ConfidenceScore ConfidenceScore { get; private set; }
    public Severity Severity { get; private set; }
    public List<EvidenceReference> EvidenceReferences { get; private set; }

    private Diagnosis()
    {
        ConditionCode = string.Empty;
        ConditionName = string.Empty;
        ConfidenceScore = new ConfidenceScore(0);
        EvidenceReferences = [];
    }

    public Diagnosis(
        string conditionCode,
        string conditionName,
        ConfidenceScore confidenceScore,
        Severity severity,
        List<EvidenceReference> evidenceReferences)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conditionCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(conditionName);
        ArgumentNullException.ThrowIfNull(confidenceScore);

        ConditionCode = conditionCode;
        ConditionName = conditionName;
        ConfidenceScore = confidenceScore;
        Severity = severity;
        EvidenceReferences = evidenceReferences ?? [];
    }
}
