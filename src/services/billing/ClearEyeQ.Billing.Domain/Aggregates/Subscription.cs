using ClearEyeQ.Billing.Domain.Entities;
using ClearEyeQ.Billing.Domain.Enums;
using ClearEyeQ.Billing.Domain.Events;
using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Billing.Domain.Aggregates;

public sealed class Subscription : AggregateRoot
{
    public Guid SubscriptionId => Id;
    private TenantId _tenantId;
    public override TenantId TenantId => _tenantId;
    public override PartitionKey PartitionKey => PartitionKey.ForTenant(_tenantId);
    public PlanTier PlanTier { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public string? StripeSubscriptionId { get; private set; }
    public DateTimeOffset CurrentPeriodStart { get; private set; }
    public DateTimeOffset CurrentPeriodEnd { get; private set; }
    public UsageMeter UsageMeter { get; private set; } = default!;
    public int PaymentFailureCount { get; private set; }

    private Subscription() { }

    public static Subscription Create(
        TenantId tenantId,
        PlanTier planTier,
        string? stripeSubscriptionId = null)
    {
        var plan = Plan.Create(planTier);
        var now = DateTimeOffset.UtcNow;

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            _tenantId = tenantId,
            PlanTier = planTier,
            Status = SubscriptionStatus.Active,
            StripeSubscriptionId = stripeSubscriptionId,
            CurrentPeriodStart = now,
            CurrentPeriodEnd = now.AddMonths(1),
            UsageMeter = UsageMeter.Create(plan.FeatureSet.MaxScansPerMonth, DateOnly.FromDateTime(now.DateTime)),
            Audit = AuditMetadata.Create("system")
        };

        subscription.AddDomainEvent(new SubscriptionCreatedEvent
        {
            SubscriptionId = subscription.Id,
            TenantId = tenantId,
            PlanTier = planTier
        });

        return subscription;
    }

    public void Upgrade(PlanTier newTier)
    {
        if (newTier <= PlanTier)
            throw new InvalidOperationException($"Cannot upgrade from {PlanTier} to {newTier}. New tier must be higher.");

        if (Status is SubscriptionStatus.Cancelled or SubscriptionStatus.Suspended)
            throw new InvalidOperationException($"Cannot upgrade subscription in {Status} status.");

        var oldTier = PlanTier;
        PlanTier = newTier;

        var newPlan = Plan.Create(newTier);
        UsageMeter.UpdateLimit(newPlan.FeatureSet.MaxScansPerMonth);
        Audit = Audit.WithModification("system");

        AddDomainEvent(new SubscriptionChangedEvent
        {
            SubscriptionId = Id,
            TenantId = _tenantId,
            OldTier = oldTier,
            NewTier = newTier
        });
    }

    public void Cancel()
    {
        if (Status == SubscriptionStatus.Cancelled)
            throw new InvalidOperationException("Subscription is already cancelled.");

        Status = SubscriptionStatus.Cancelled;
        Audit = Audit.WithModification("system");
    }

    public void RecordUsage()
    {
        if (Status is not (SubscriptionStatus.Active or SubscriptionStatus.Trialing))
            throw new InvalidOperationException($"Cannot record usage for subscription in {Status} status.");

        var limitReached = UsageMeter.IncrementUsage();

        if (limitReached)
        {
            AddDomainEvent(new UsageLimitReachedEvent
            {
                SubscriptionId = Id,
                TenantId = _tenantId,
                ScanCount = UsageMeter.ScanCount,
                ScanLimit = UsageMeter.ScanLimit
            });
        }
    }

    public void HandlePaymentSuccess()
    {
        PaymentFailureCount = 0;

        if (Status == SubscriptionStatus.PastDue)
        {
            Status = SubscriptionStatus.Active;
        }

        var now = DateTimeOffset.UtcNow;
        CurrentPeriodStart = now;
        CurrentPeriodEnd = now.AddMonths(1);
        UsageMeter.Reset(DateOnly.FromDateTime(now.DateTime));
        Audit = Audit.WithModification("stripe");
    }

    public void HandlePaymentFailure()
    {
        PaymentFailureCount++;

        if (PaymentFailureCount >= 3)
        {
            Status = SubscriptionStatus.Suspended;
        }
        else
        {
            Status = SubscriptionStatus.PastDue;
        }

        Audit = Audit.WithModification("stripe");
    }
}
