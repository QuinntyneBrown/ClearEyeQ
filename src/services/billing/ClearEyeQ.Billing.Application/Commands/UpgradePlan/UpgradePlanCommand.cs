using ClearEyeQ.Billing.Domain.Enums;
using MediatR;

namespace ClearEyeQ.Billing.Application.Commands.UpgradePlan;

public sealed record UpgradePlanCommand(
    Guid SubscriptionId,
    PlanTier NewTier) : IRequest;
