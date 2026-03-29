using MediatR;

namespace ClearEyeQ.Billing.Application.Commands.CancelSubscription;

public sealed record CancelSubscriptionCommand(Guid SubscriptionId) : IRequest;
