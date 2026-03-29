using ClearEyeQ.Billing.Application.Interfaces;
using ClearEyeQ.Billing.Domain.Aggregates;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Billing.Infrastructure.Persistence;

public sealed class EfSubscriptionRepository : ISubscriptionRepository
{
    private readonly BillingDbContext _context;
    private readonly ILogger<EfSubscriptionRepository> _logger;

    public EfSubscriptionRepository(
        BillingDbContext context,
        ILogger<EfSubscriptionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Subscription?> GetByIdAsync(Guid subscriptionId, CancellationToken ct)
    {
        return await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId, ct);
    }

    public async Task<Subscription?> GetByTenantAsync(TenantId tenantId, CancellationToken ct)
    {
        return await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);
    }

    public async Task<Subscription?> GetByStripeIdAsync(string stripeSubscriptionId, CancellationToken ct)
    {
        return await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId, ct);
    }

    public async Task AddAsync(Subscription subscription, CancellationToken ct)
    {
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Created subscription {SubscriptionId} for tenant {TenantId}",
            subscription.SubscriptionId, subscription.TenantId);
    }

    public async Task UpdateAsync(Subscription subscription, CancellationToken ct)
    {
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Updated subscription {SubscriptionId} for tenant {TenantId}",
            subscription.SubscriptionId, subscription.TenantId);
    }
}
