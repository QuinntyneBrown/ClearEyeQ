using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace ClearEyeQ.SharedKernel.Tests.Domain;

public sealed class PartitionKeyTests
{
    [Fact]
    public void ForTenant_ReturnsTenantIdAsValue()
    {
        var tenantId = new TenantId(Guid.Parse("11111111-1111-1111-1111-111111111111"));

        var pk = PartitionKey.ForTenant(tenantId);

        pk.Value.Should().Be("11111111-1111-1111-1111-111111111111");
    }

    [Fact]
    public void ForUserInTenant_ReturnsPipeSeparatedValue()
    {
        var tenantId = new TenantId(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var userId = new UserId(Guid.Parse("22222222-2222-2222-2222-222222222222"));

        var pk = PartitionKey.ForUserInTenant(tenantId, userId);

        pk.Value.Should().Be("11111111-1111-1111-1111-111111111111|22222222-2222-2222-2222-222222222222");
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var tenantId = TenantId.New();
        var pk = PartitionKey.ForTenant(tenantId);

        pk.ToString().Should().Be(tenantId.Value.ToString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var tenantId = new TenantId(Guid.Parse("11111111-1111-1111-1111-111111111111"));

        var pk1 = PartitionKey.ForTenant(tenantId);
        var pk2 = PartitionKey.ForTenant(tenantId);

        pk1.Should().Be(pk2);
    }

    [Fact]
    public void Equality_DifferentTenants_AreNotEqual()
    {
        var pk1 = PartitionKey.ForTenant(TenantId.New());
        var pk2 = PartitionKey.ForTenant(TenantId.New());

        pk1.Should().NotBe(pk2);
    }
}
