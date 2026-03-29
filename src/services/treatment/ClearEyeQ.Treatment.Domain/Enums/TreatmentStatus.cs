namespace ClearEyeQ.Treatment.Domain.Enums;

public enum TreatmentStatus
{
    Draft = 0,
    PendingApproval = 1,
    Active = 2,
    PendingAdjustmentApproval = 3,
    EscalationRecommended = 4,
    Resolved = 5,
    Maintenance = 6,
    Rejected = 7
}
