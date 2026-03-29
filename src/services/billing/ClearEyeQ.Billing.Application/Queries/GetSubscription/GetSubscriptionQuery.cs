using MediatR;

namespace ClearEyeQ.Billing.Application.Queries.GetSubscription;

public sealed record GetSubscriptionQuery(Guid TenantId) : IRequest<SubscriptionDto?>;
