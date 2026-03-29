using ClearEyeQ.Billing.Application.Interfaces;
using MediatR;

namespace ClearEyeQ.Billing.Application.Commands.CancelSubscription;

public sealed class CancelSubscriptionHandler : IRequestHandler<CancelSubscriptionCommand>
{
    private readonly ISubscriptionRepository _repository;
    private readonly IPaymentGateway _paymentGateway;

    public CancelSubscriptionHandler(
        ISubscriptionRepository repository,
        IPaymentGateway paymentGateway)
    {
        _repository = repository;
        _paymentGateway = paymentGateway;
    }

    public async Task Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _repository.GetByIdAsync(request.SubscriptionId, cancellationToken)
            ?? throw new InvalidOperationException($"Subscription {request.SubscriptionId} not found.");

        if (subscription.StripeSubscriptionId is not null)
        {
            await _paymentGateway.CancelSubscriptionAsync(
                subscription.StripeSubscriptionId,
                cancellationToken);
        }

        subscription.Cancel();

        await _repository.UpdateAsync(subscription, cancellationToken);
    }
}
