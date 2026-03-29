using MediatR;

namespace ClearEyeQ.Billing.Application.Commands.HandleStripeWebhook;

public sealed record HandleStripeWebhookCommand(
    string EventType,
    string StripeSubscriptionId) : IRequest;
