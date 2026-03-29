using Xunit;
using ClearEyeQ.Environmental.Domain.Aggregates;
using ClearEyeQ.Environmental.Domain.Enums;
using ClearEyeQ.Environmental.Domain.Events;
using ClearEyeQ.Environmental.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using FluentAssertions;

namespace ClearEyeQ.Environmental.Tests.Unit;

public sealed class EnvironmentalSnapshotTests
{
    private readonly UserId _userId = UserId.New();
    private readonly TenantId _tenantId = TenantId.New();

    [Fact]
    public void Create_ShouldReturnSnapshotWithDomainEvent()
    {
        var location = new GeoLocation(40.7128, -74.0060);

        var snapshot = EnvironmentalSnapshot.Create(_userId, _tenantId, location);

        snapshot.SnapshotId.Should().NotBeEmpty();
        snapshot.UserId.Should().Be(_userId);
        snapshot.TenantId.Should().Be(_tenantId);
        snapshot.GeoLocation.Should().Be(location);
        snapshot.CapturedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        snapshot.AirQualityReading.Should().BeNull();
        snapshot.PollenCount.Should().BeNull();
        snapshot.UvIndex.Should().BeNull();
        snapshot.HumidityReading.Should().BeNull();
        snapshot.ScreenTimeRecord.Should().BeNull();

        snapshot.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<EnvironmentalSnapshotCapturedEvent>();
    }

    [Fact]
    public void Create_WithoutLocation_ShouldSucceed()
    {
        var snapshot = EnvironmentalSnapshot.Create(_userId, _tenantId);

        snapshot.GeoLocation.Should().BeNull();
        snapshot.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void SetAirQuality_ShouldClassifyLevel()
    {
        var snapshot = EnvironmentalSnapshot.Create(_userId, _tenantId);

        snapshot.SetAirQuality(75, 25.3, 40.1);

        snapshot.AirQualityReading.Should().NotBeNull();
        snapshot.AirQualityReading!.Aqi.Should().Be(75);
        snapshot.AirQualityReading.Pm25.Should().Be(25.3);
        snapshot.AirQualityReading.Pm10.Should().Be(40.1);
        snapshot.AirQualityReading.Level.Should().Be(AirQualityLevel.Moderate);
    }

    [Theory]
    [InlineData(30, AirQualityLevel.Good)]
    [InlineData(75, AirQualityLevel.Moderate)]
    [InlineData(120, AirQualityLevel.UnhealthyForSensitive)]
    [InlineData(180, AirQualityLevel.Unhealthy)]
    [InlineData(250, AirQualityLevel.VeryUnhealthy)]
    [InlineData(350, AirQualityLevel.Hazardous)]
    public void SetAirQuality_ShouldClassifyCorrectLevel(int aqi, AirQualityLevel expectedLevel)
    {
        var snapshot = EnvironmentalSnapshot.Create(_userId, _tenantId);

        snapshot.SetAirQuality(aqi, 10.0, 20.0);

        snapshot.AirQualityReading!.Level.Should().Be(expectedLevel);
    }

    [Fact]
    public void SetPollen_ShouldClassifyOverallLevel()
    {
        var snapshot = EnvironmentalSnapshot.Create(_userId, _tenantId);

        snapshot.SetPollen(tree: 100, grass: 50, weed: 30);

        snapshot.PollenCount.Should().NotBeNull();
        snapshot.PollenCount!.Tree.Should().Be(100);
        snapshot.PollenCount.Grass.Should().Be(50);
        snapshot.PollenCount.Weed.Should().Be(30);
        snapshot.PollenCount.OverallLevel.Should().Be("High");
    }

    [Theory]
    [InlineData(10, 5, 15, "Low")]
    [InlineData(50, 30, 40, "Moderate")]
    [InlineData(150, 80, 100, "High")]
    [InlineData(300, 250, 200, "Very High")]
    public void SetPollen_ShouldClassifyCorrectOverallLevel(int tree, int grass, int weed, string expectedLevel)
    {
        var snapshot = EnvironmentalSnapshot.Create(_userId, _tenantId);

        snapshot.SetPollen(tree, grass, weed);

        snapshot.PollenCount!.OverallLevel.Should().Be(expectedLevel);
    }

    [Fact]
    public void SetUv_ShouldClassifyRisk()
    {
        var snapshot = EnvironmentalSnapshot.Create(_userId, _tenantId);

        snapshot.SetUv(7.5);

        snapshot.UvIndex.Should().NotBeNull();
        snapshot.UvIndex!.Value.Should().Be(7.5);
        snapshot.UvIndex.RiskCategory.Should().Be("High");
    }

    [Theory]
    [InlineData(1.0, "Low")]
    [InlineData(4.0, "Moderate")]
    [InlineData(7.0, "High")]
    [InlineData(9.5, "Very High")]
    [InlineData(12.0, "Extreme")]
    public void SetUv_ShouldClassifyCorrectRisk(double uv, string expectedRisk)
    {
        var snapshot = EnvironmentalSnapshot.Create(_userId, _tenantId);

        snapshot.SetUv(uv);

        snapshot.UvIndex!.RiskCategory.Should().Be(expectedRisk);
    }

    [Fact]
    public void SetHumidity_ShouldSetPercentage()
    {
        var snapshot = EnvironmentalSnapshot.Create(_userId, _tenantId);

        snapshot.SetHumidity(45.0);

        snapshot.HumidityReading.Should().NotBeNull();
        snapshot.HumidityReading!.Percentage.Should().Be(45.0);
        snapshot.HumidityReading.IsComfortableForEyes.Should().BeTrue();
    }

    [Fact]
    public void SetHumidity_WhenOutOfRange_ShouldThrow()
    {
        var snapshot = EnvironmentalSnapshot.Create(_userId, _tenantId);

        var act = () => snapshot.SetHumidity(110.0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetHumidity_WhenTooLow_ShouldNotBeComfortable()
    {
        var snapshot = EnvironmentalSnapshot.Create(_userId, _tenantId);

        snapshot.SetHumidity(15.0);

        snapshot.HumidityReading!.IsComfortableForEyes.Should().BeFalse();
    }

    [Fact]
    public void SetScreenTime_ShouldSetRecord()
    {
        var snapshot = EnvironmentalSnapshot.Create(_userId, _tenantId);
        var breakdown = new Dictionary<string, TimeSpan>
        {
            ["Chrome"] = TimeSpan.FromHours(2),
            ["Slack"] = TimeSpan.FromHours(1.5),
            ["VSCode"] = TimeSpan.FromHours(4)
        };

        snapshot.SetScreenTime(TimeSpan.FromHours(7.5), breakdown);

        snapshot.ScreenTimeRecord.Should().NotBeNull();
        snapshot.ScreenTimeRecord!.TotalDuration.Should().Be(TimeSpan.FromHours(7.5));
        snapshot.ScreenTimeRecord.TotalHours.Should().Be(7.5);
        snapshot.ScreenTimeRecord.AppBreakdown.Should().HaveCount(3);
    }

    [Fact]
    public void SetScreenTime_WhenNegativeDuration_ShouldThrow()
    {
        var snapshot = EnvironmentalSnapshot.Create(_userId, _tenantId);

        var act = () => snapshot.SetScreenTime(TimeSpan.FromHours(-1), new Dictionary<string, TimeSpan>());

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void PartitionKey_ShouldBeTenantUserComposite()
    {
        var snapshot = EnvironmentalSnapshot.Create(_userId, _tenantId);

        snapshot.PartitionKey.Value.Should().Be($"{_tenantId.Value}|{_userId.Value}");
    }
}
