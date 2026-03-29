using ClearEyeQ.Billing.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using PlanTier = ClearEyeQ.Billing.Domain.Enums.PlanTier;

namespace ClearEyeQ.Billing.Infrastructure.Payments;

public sealed class StripePaymentGateway : IPaymentGateway
{
    private readonly ILogger<StripePaymentGateway> _logger;
    private readonly Dictionary<PlanTier, string> _priceIds;

    public StripePaymentGateway(
        IConfiguration configuration,
        ILogger<StripePaymentGateway> logger)
    {
        _logger = logger;

        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe secret key is required.");

        _priceIds = new Dictionary<PlanTier, string>
        {
            [PlanTier.Pro] = configuration["Stripe:PriceIds:Pro"] ?? "price_pro",
            [PlanTier.Premium] = configuration["Stripe:PriceIds:Premium"] ?? "price_premium",
            [PlanTier.Autonomous] = configuration["Stripe:PriceIds:Autonomous"] ?? "price_autonomous"
        };
    }

    public async Task<string> CreateSubscriptionAsync(string customerId, PlanTier tier, CancellationToken ct)
    {
        var service = new SubscriptionService();
        var options = new SubscriptionCreateOptions
        {
            Customer = customerId,
            Items = [new SubscriptionItemOptions { Price = GetPriceId(tier) }]
        };

        var subscription = await service.CreateAsync(options, cancellationToken: ct);

        _logger.LogInformation("Created Stripe subscription {SubscriptionId} for customer {CustomerId}",
            subscription.Id, customerId);

        return subscription.Id;
    }

    public async Task<string> CreateCheckoutSessionAsync(
        string customerId,
        PlanTier tier,
        string successUrl,
        string cancelUrl,
        CancellationToken ct)
    {
        var service = new Stripe.Checkout.SessionService();
        var options = new Stripe.Checkout.SessionCreateOptions
        {
            Customer = customerId,
            Mode = "subscription",
            LineItems = [new Stripe.Checkout.SessionLineItemOptions
            {
                Price = GetPriceId(tier),
                Quantity = 1
            }],
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl
        };

        var session = await service.CreateAsync(options, cancellationToken: ct);

        _logger.LogInformation("Created Stripe checkout session {SessionId} for customer {CustomerId}",
            session.Id, customerId);

        return session.Url;
    }

    public async Task CancelSubscriptionAsync(string stripeSubscriptionId, CancellationToken ct)
    {
        var service = new SubscriptionService();
        await service.CancelAsync(stripeSubscriptionId, cancellationToken: ct);

        _logger.LogInformation("Cancelled Stripe subscription {SubscriptionId}", stripeSubscriptionId);
    }

    public async Task UpdateSubscriptionAsync(string stripeSubscriptionId, PlanTier newTier, CancellationToken ct)
    {
        var service = new SubscriptionService();
        var subscription = await service.GetAsync(stripeSubscriptionId, cancellationToken: ct);

        var itemId = subscription.Items.Data[0].Id;

        await service.UpdateAsync(stripeSubscriptionId, new SubscriptionUpdateOptions
        {
            Items = [new SubscriptionItemOptions
            {
                Id = itemId,
                Price = GetPriceId(newTier)
            }]
        }, cancellationToken: ct);

        _logger.LogInformation("Updated Stripe subscription {SubscriptionId} to tier {Tier}",
            stripeSubscriptionId, newTier);
    }

    public bool VerifyWebhookSignature(string payload, string signature, string secret)
    {
        try
        {
            EventUtility.ConstructEvent(payload, signature, secret);
            return true;
        }
        catch (StripeException)
        {
            _logger.LogWarning("Invalid Stripe webhook signature");
            return false;
        }
    }

    private string GetPriceId(PlanTier tier)
    {
        if (_priceIds.TryGetValue(tier, out var priceId))
            return priceId;

        throw new InvalidOperationException($"No Stripe price ID configured for tier {tier}.");
    }
}
