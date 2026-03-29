using ClearEyeQ.Billing.Application.Commands.HandleStripeWebhook;
using ClearEyeQ.Billing.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace ClearEyeQ.Billing.Infrastructure.Payments;

public sealed class StripeWebhookHandler
{
    private readonly IMediator _mediator;
    private readonly IPaymentGateway _paymentGateway;
    private readonly string _webhookSecret;
    private readonly ILogger<StripeWebhookHandler> _logger;

    public StripeWebhookHandler(
        IMediator mediator,
        IPaymentGateway paymentGateway,
        IConfiguration configuration,
        ILogger<StripeWebhookHandler> logger)
    {
        _mediator = mediator;
        _paymentGateway = paymentGateway;
        _webhookSecret = configuration["Stripe:WebhookSecret"]
            ?? throw new InvalidOperationException("Stripe webhook secret is required.");
        _logger = logger;
    }

    public async Task<bool> HandleWebhookAsync(string payload, string signature, CancellationToken ct)
    {
        if (!_paymentGateway.VerifyWebhookSignature(payload, signature, _webhookSecret))
        {
            _logger.LogWarning("Stripe webhook signature verification failed");
            return false;
        }

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(payload, signature, _webhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to construct Stripe event");
            return false;
        }

        var subscriptionId = ExtractSubscriptionId(stripeEvent);
        if (subscriptionId is null)
        {
            _logger.LogInformation("Stripe event {EventType} does not contain a subscription ID, skipping",
                stripeEvent.Type);
            return true;
        }

        _logger.LogInformation("Processing Stripe webhook event {EventType} for subscription {SubscriptionId}",
            stripeEvent.Type, subscriptionId);

        await _mediator.Send(new HandleStripeWebhookCommand(
            EventType: stripeEvent.Type,
            StripeSubscriptionId: subscriptionId), ct);

        return true;
    }

    private static string? ExtractSubscriptionId(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is Subscription subscription)
            return subscription.Id;

        if (stripeEvent.Data.Object is Invoice invoice)
            return invoice.SubscriptionId;

        return null;
    }
}
