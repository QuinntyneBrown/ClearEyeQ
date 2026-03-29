using ClearEyeQ.Billing.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Billing.Application.Queries.GetSubscription;

public sealed class GetSubscriptionHandler : IRequestHandler<GetSubscriptionQuery, SubscriptionDto?>
{
    private readonly ISubscriptionRepository _repository;

    public GetSubscriptionHandler(ISubscriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<SubscriptionDto?> Handle(GetSubscriptionQuery request, CancellationToken cancellationToken)
    {
        var tenantId = new TenantId(request.TenantId);
        var subscription = await _repository.GetByTenantAsync(tenantId, cancellationToken);

        if (subscription is null)
            return null;

        return new SubscriptionDto(
            SubscriptionId: subscription.SubscriptionId,
            TenantId: subscription.TenantId.Value,
            PlanTier: subscription.PlanTier,
            Status: subscription.Status,
            CurrentPeriodStart: subscription.CurrentPeriodStart,
            CurrentPeriodEnd: subscription.CurrentPeriodEnd,
            ScanCount: subscription.UsageMeter.ScanCount,
            ScanLimit: subscription.UsageMeter.ScanLimit);
    }
}
