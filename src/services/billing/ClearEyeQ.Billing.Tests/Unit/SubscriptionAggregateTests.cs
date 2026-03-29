using ClearEyeQ.Billing.Domain.Aggregates;
using ClearEyeQ.Billing.Domain.Enums;
using ClearEyeQ.Billing.Domain.Events;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace ClearEyeQ.Billing.Tests.Unit;

public sealed class SubscriptionAggregateTests
{
    private readonly TenantId _tenantId = TenantId.New();

    [Fact]
    public void Create_ShouldInitializeWithActiveStatus()
    {
        var subscription = Subscription.Create(_tenantId, PlanTier.Pro, "sub_test");

        subscription.Status.Should().Be(SubscriptionStatus.Active);
        subscription.PlanTier.Should().Be(PlanTier.Pro);
        subscription.StripeSubscriptionId.Should().Be("sub_test");
        subscription.TenantId.Should().Be(_tenantId);
        subscription.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SubscriptionCreatedEvent>();
    }

    [Fact]
    public void Create_FreeTier_ShouldHaveLimitedScans()
    {
        var subscription = Subscription.Create(_tenantId, PlanTier.Free);

        subscription.UsageMeter.ScanLimit.Should().Be(1);
        subscription.UsageMeter.ScanCount.Should().Be(0);
    }

    [Fact]
    public void Upgrade_ToHigherTier_ShouldSucceed()
    {
        var subscription = Subscription.Create(_tenantId, PlanTier.Free);
        subscription.ClearDomainEvents();

        subscription.Upgrade(PlanTier.Pro);

        subscription.PlanTier.Should().Be(PlanTier.Pro);
        subscription.UsageMeter.ScanLimit.Should().Be(10);
        subscription.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SubscriptionChangedEvent>();
    }

    [Fact]
    public void Upgrade_ToLowerTier_ShouldThrow()
    {
        var subscription = Subscription.Create(_tenantId, PlanTier.Premium);

        var act = () => subscription.Upgrade(PlanTier.Pro);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_ShouldTransitionToCancelled()
    {
        var subscription = Subscription.Create(_tenantId, PlanTier.Pro);

        subscription.Cancel();

        subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
    }

    [Fact]
    public void Cancel_AlreadyCancelled_ShouldThrow()
    {
        var subscription = Subscription.Create(_tenantId, PlanTier.Pro);
        subscription.Cancel();

        var act = () => subscription.Cancel();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RecordUsage_ShouldIncrementScanCount()
    {
        var subscription = Subscription.Create(_tenantId, PlanTier.Pro);

        subscription.RecordUsage();

        subscription.UsageMeter.ScanCount.Should().Be(1);
    }

    [Fact]
    public void RecordUsage_AtLimit_ShouldRaiseUsageLimitReachedEvent()
    {
        var subscription = Subscription.Create(_tenantId, PlanTier.Free);
        subscription.ClearDomainEvents();

        subscription.RecordUsage(); // Limit is 1 for Free

        subscription.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UsageLimitReachedEvent>();
    }

    [Fact]
    public void HandlePaymentSuccess_ShouldResetPeriod()
    {
        var subscription = Subscription.Create(_tenantId, PlanTier.Pro);
        subscription.RecordUsage();
        subscription.RecordUsage();

        subscription.HandlePaymentSuccess();

        subscription.UsageMeter.ScanCount.Should().Be(0);
        subscription.PaymentFailureCount.Should().Be(0);
    }

    [Fact]
    public void HandlePaymentFailure_FirstTime_ShouldSetPastDue()
    {
        var subscription = Subscription.Create(_tenantId, PlanTier.Pro);

        subscription.HandlePaymentFailure();

        subscription.Status.Should().Be(SubscriptionStatus.PastDue);
        subscription.PaymentFailureCount.Should().Be(1);
    }

    [Fact]
    public void HandlePaymentFailure_ThirdTime_ShouldSuspend()
    {
        var subscription = Subscription.Create(_tenantId, PlanTier.Pro);

        subscription.HandlePaymentFailure();
        subscription.HandlePaymentFailure();
        subscription.HandlePaymentFailure();

        subscription.Status.Should().Be(SubscriptionStatus.Suspended);
    }

    [Fact]
    public void HandlePaymentSuccess_AfterPastDue_ShouldReactivate()
    {
        var subscription = Subscription.Create(_tenantId, PlanTier.Pro);
        subscription.HandlePaymentFailure();
        subscription.Status.Should().Be(SubscriptionStatus.PastDue);

        subscription.HandlePaymentSuccess();

        subscription.Status.Should().Be(SubscriptionStatus.Active);
    }
}
