using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace ClearEyeQ.SharedKernel.Tests.Domain;

public sealed class AggregateRootTests
{
    [Fact]
    public void NewAggregate_HasNoEvents()
    {
        var aggregate = new TestAggregate(TenantId.New());

        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void RaiseEvent_AddsDomainEvent()
    {
        var aggregate = new TestAggregate(TenantId.New());

        aggregate.DoSomething();

        aggregate.DomainEvents.Should().HaveCount(1);
        aggregate.DomainEvents[0].Should().BeOfType<TestDomainEvent>();
    }

    [Fact]
    public void RaiseMultipleEvents_AddsAllEvents()
    {
        var aggregate = new TestAggregate(TenantId.New());

        aggregate.DoSomething();
        aggregate.DoSomething();
        aggregate.DoSomething();

        aggregate.DomainEvents.Should().HaveCount(3);
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var aggregate = new TestAggregate(TenantId.New());
        aggregate.DoSomething();
        aggregate.DoSomething();

        aggregate.ClearDomainEvents();

        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Id_IsAssignedOnCreation()
    {
        var aggregate = new TestAggregate(TenantId.New());

        aggregate.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void TenantId_IsSetFromConstructor()
    {
        var tenantId = TenantId.New();
        var aggregate = new TestAggregate(tenantId);

        aggregate.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public void PartitionKey_IsDerivedFromTenantId()
    {
        var tenantId = TenantId.New();
        var aggregate = new TestAggregate(tenantId);

        aggregate.PartitionKey.Value.Should().Be(tenantId.Value.ToString());
    }

    [Fact]
    public void DomainEvents_ReturnsReadOnlyCopy()
    {
        var aggregate = new TestAggregate(TenantId.New());
        aggregate.DoSomething();

        var events = aggregate.DomainEvents;

        events.Should().BeAssignableTo<IReadOnlyList<IDomainEvent>>();
    }

    #region Test Doubles

    private sealed class TestAggregate : AggregateRoot
    {
        private readonly TenantId _tenantId;

        public TestAggregate(TenantId tenantId)
        {
            _tenantId = tenantId;
        }

        public override TenantId TenantId => _tenantId;
        public override PartitionKey PartitionKey => PartitionKey.ForTenant(_tenantId);

        public void DoSomething()
        {
            AddDomainEvent(new TestDomainEvent());
        }
    }

    private sealed record TestDomainEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
    }

    #endregion
}
