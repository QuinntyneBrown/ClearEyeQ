namespace ClearEyeQ.Billing.Application.Queries.CheckFeatureAccess;

public sealed record FeatureAccessDto(
    bool HasAccess,
    string? Reason);
