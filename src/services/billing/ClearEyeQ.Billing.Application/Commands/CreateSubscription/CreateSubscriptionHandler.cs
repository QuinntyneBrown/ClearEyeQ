using ClearEyeQ.Billing.Application.Interfaces;
using ClearEyeQ.Billing.Domain.Aggregates;
using ClearEyeQ.Billing.Domain.Enums;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Billing.Application.Commands.CreateSubscription;

public sealed class CreateSubscriptionHandler : IRequestHandler<CreateSubscriptionCommand, Guid>
{
    private readonly ISubscriptionRepository _repository;
    private readonly IPaymentGateway _paymentGateway;

    public CreateSubscriptionHandler(
        ISubscriptionRepository repository,
        IPaymentGateway paymentGateway)
    {
        _repository = repository;
        _paymentGateway = paymentGateway;
    }

    public async Task<Guid> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        string? stripeSubscriptionId = null;

        if (request.PlanTier != PlanTier.Free && request.StripeCustomerId is not null)
        {
            stripeSubscriptionId = await _paymentGateway.CreateSubscriptionAsync(
                request.StripeCustomerId,
                request.PlanTier,
                cancellationToken);
        }

        var subscription = Subscription.Create(
            new TenantId(request.TenantId),
            request.PlanTier,
            stripeSubscriptionId);

        await _repository.AddAsync(subscription, cancellationToken);

        return subscription.SubscriptionId;
    }
}
