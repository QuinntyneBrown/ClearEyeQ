using ClearEyeQ.Billing.Domain.Enums;

namespace ClearEyeQ.Billing.Application.Queries.GetSubscription;

public sealed record SubscriptionDto(
    Guid SubscriptionId,
    Guid TenantId,
    PlanTier PlanTier,
    SubscriptionStatus Status,
    DateTimeOffset CurrentPeriodStart,
    DateTimeOffset CurrentPeriodEnd,
    int ScanCount,
    int ScanLimit);
