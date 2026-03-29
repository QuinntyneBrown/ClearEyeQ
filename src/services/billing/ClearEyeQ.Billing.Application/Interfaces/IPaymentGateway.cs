using ClearEyeQ.Billing.Domain.Enums;

namespace ClearEyeQ.Billing.Application.Interfaces;

public interface IPaymentGateway
{
    Task<string> CreateSubscriptionAsync(string customerId, PlanTier tier, CancellationToken ct);
    Task<string> CreateCheckoutSessionAsync(string customerId, PlanTier tier, string successUrl, string cancelUrl, CancellationToken ct);
    Task CancelSubscriptionAsync(string stripeSubscriptionId, CancellationToken ct);
    Task UpdateSubscriptionAsync(string stripeSubscriptionId, PlanTier newTier, CancellationToken ct);
    bool VerifyWebhookSignature(string payload, string signature, string secret);
}
