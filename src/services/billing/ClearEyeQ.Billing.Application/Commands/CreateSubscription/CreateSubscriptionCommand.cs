using ClearEyeQ.Billing.Domain.Enums;
using MediatR;

namespace ClearEyeQ.Billing.Application.Commands.CreateSubscription;

public sealed record CreateSubscriptionCommand(
    Guid TenantId,
    PlanTier PlanTier,
    string? StripeCustomerId = null) : IRequest<Guid>;
