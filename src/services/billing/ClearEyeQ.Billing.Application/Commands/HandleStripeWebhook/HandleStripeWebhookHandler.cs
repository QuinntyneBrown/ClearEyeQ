using ClearEyeQ.Billing.Application.Interfaces;
using MediatR;

namespace ClearEyeQ.Billing.Application.Commands.HandleStripeWebhook;

public sealed class HandleStripeWebhookHandler : IRequestHandler<HandleStripeWebhookCommand>
{
    private readonly ISubscriptionRepository _repository;

    public HandleStripeWebhookHandler(ISubscriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(HandleStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _repository.GetByStripeIdAsync(request.StripeSubscriptionId, cancellationToken);

        if (subscription is null)
            return;

        switch (request.EventType)
        {
            case "invoice.payment_succeeded":
                subscription.HandlePaymentSuccess();
                break;

            case "invoice.payment_failed":
                subscription.HandlePaymentFailure();
                break;

            case "customer.subscription.deleted":
                subscription.Cancel();
                break;

            default:
                return;
        }

        await _repository.UpdateAsync(subscription, cancellationToken);
    }
}
