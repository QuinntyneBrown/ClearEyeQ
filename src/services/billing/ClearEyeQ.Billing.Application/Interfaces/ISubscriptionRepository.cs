using ClearEyeQ.Billing.Domain.Aggregates;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Billing.Application.Interfaces;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByIdAsync(Guid subscriptionId, CancellationToken ct);
    Task<Subscription?> GetByTenantAsync(TenantId tenantId, CancellationToken ct);
    Task<Subscription?> GetByStripeIdAsync(string stripeSubscriptionId, CancellationToken ct);
    Task AddAsync(Subscription subscription, CancellationToken ct);
    Task UpdateAsync(Subscription subscription, CancellationToken ct);
}
