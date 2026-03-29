namespace ClearEyeQ.Diagnostic.Domain.Enums;

public enum DiagnosticStatus
{
    Initiated = 0,
    GatheringContext = 1,
    Analyzing = 2,
    Completed = 3,
    Failed = 4
}
