using ClearEyeQ.Billing.Application.Interfaces;
using MediatR;

namespace ClearEyeQ.Billing.Application.Commands.UpgradePlan;

public sealed class UpgradePlanHandler : IRequestHandler<UpgradePlanCommand>
{
    private readonly ISubscriptionRepository _repository;
    private readonly IPaymentGateway _paymentGateway;

    public UpgradePlanHandler(
        ISubscriptionRepository repository,
        IPaymentGateway paymentGateway)
    {
        _repository = repository;
        _paymentGateway = paymentGateway;
    }

    public async Task Handle(UpgradePlanCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _repository.GetByIdAsync(request.SubscriptionId, cancellationToken)
            ?? throw new InvalidOperationException($"Subscription {request.SubscriptionId} not found.");

        if (subscription.StripeSubscriptionId is not null)
        {
            await _paymentGateway.UpdateSubscriptionAsync(
                subscription.StripeSubscriptionId,
                request.NewTier,
                cancellationToken);
        }

        subscription.Upgrade(request.NewTier);

        await _repository.UpdateAsync(subscription, cancellationToken);
    }
}
