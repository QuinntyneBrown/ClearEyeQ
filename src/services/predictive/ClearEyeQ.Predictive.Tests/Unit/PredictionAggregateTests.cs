using ClearEyeQ.Predictive.Domain.Aggregates;
using ClearEyeQ.Predictive.Domain.Entities;
using ClearEyeQ.Predictive.Domain.Enums;
using ClearEyeQ.Predictive.Domain.Events;
using ClearEyeQ.Predictive.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace ClearEyeQ.Predictive.Tests.Unit;

public sealed class PredictionAggregateTests
{
    private static Prediction CreatePrediction()
    {
        return Prediction.Create(UserId.New(), TenantId.New());
    }

    private static List<ForecastDay> CreateForecastDays(int count = 3)
    {
        return Enumerable.Range(0, count).Select(i => new ForecastDay(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(i)),
            0.5 + i * 0.1,
            RiskLevel.Moderate,
            "Pollen",
            0.4 + i * 0.1,
            0.6 + i * 0.1)).ToList();
    }

    [Fact]
    public void Create_ShouldInitializeWithCorrectState()
    {
        var userId = UserId.New();
        var tenantId = TenantId.New();

        var prediction = Prediction.Create(userId, tenantId);

        prediction.UserId.Should().Be(userId);
        prediction.TenantId.Should().Be(tenantId);
        prediction.Status.Should().Be(PredictionStatus.Generating);
        prediction.Forecast.Should().BeEmpty();
        prediction.FlareUpAlert.Should().BeNull();
        prediction.TrajectoryModel.Should().BeNull();
        prediction.PredictionId.Should().NotBe(Guid.Empty);
        prediction.GeneratedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void SetForecast_ShouldSetForecastDays()
    {
        var prediction = CreatePrediction();
        var days = CreateForecastDays();

        prediction.SetForecast(days);

        prediction.Forecast.Should().HaveCount(3);
    }

    [Fact]
    public void SetForecast_WhenCompleted_ShouldThrow()
    {
        var prediction = CreatePrediction();
        prediction.SetForecast(CreateForecastDays());
        prediction.Complete();

        var act = () => prediction.SetForecast(CreateForecastDays());

        act.Should().Throw<InvalidOperationException>().WithMessage("*completed*");
    }

    [Fact]
    public void SetFlareUpAlert_ShouldSetAlert()
    {
        var prediction = CreatePrediction();
        var alert = new FlareUpAlert(0.5, ["Pollen", "Poor Sleep"], ["Reduce screen time"], RiskLevel.Moderate);

        prediction.SetFlareUpAlert(alert);

        prediction.FlareUpAlert.Should().NotBeNull();
        prediction.FlareUpAlert!.Probability.Should().Be(0.5);
        prediction.FlareUpAlert.TriggerFactors.Should().HaveCount(2);
    }

    [Fact]
    public void SetFlareUpAlert_HighProbability_ShouldRaiseWarningEvent()
    {
        var prediction = CreatePrediction();
        var alert = new FlareUpAlert(0.85, ["Pollen"], ["Take antihistamine"], RiskLevel.High);

        prediction.SetFlareUpAlert(alert);

        prediction.DomainEvents.Should().HaveCount(1);
        prediction.DomainEvents[0].Should().BeOfType<FlareUpWarningEvent>();

        var evt = (FlareUpWarningEvent)prediction.DomainEvents[0];
        evt.Probability.Should().Be(0.85);
        evt.RiskLevel.Should().Be(RiskLevel.High);
    }

    [Fact]
    public void SetFlareUpAlert_LowProbability_ShouldNotRaiseWarningEvent()
    {
        var prediction = CreatePrediction();
        var alert = new FlareUpAlert(0.3, ["Dust"], ["Clean room"], RiskLevel.Low);

        prediction.SetFlareUpAlert(alert);

        prediction.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void SetTrajectory_ShouldSetModel()
    {
        var prediction = CreatePrediction();
        var trajectory = new TrajectoryModel(
            6,
            [new TrajectoryPoint(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)), 0.5, 0.3, 0.7)],
            [new TrajectoryPoint(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)), 0.8, 0.6, 1.0)]);

        prediction.SetTrajectory(trajectory);

        prediction.TrajectoryModel.Should().NotBeNull();
        prediction.TrajectoryModel!.HorizonMonths.Should().Be(6);
        prediction.TrajectoryModel.WithTreatment.Should().HaveCount(1);
        prediction.TrajectoryModel.WithoutTreatment.Should().HaveCount(1);
    }

    [Fact]
    public void Complete_ShouldSetStatusAndRaiseForecastEvent()
    {
        var prediction = CreatePrediction();
        prediction.SetForecast(CreateForecastDays());

        prediction.Complete();

        prediction.Status.Should().Be(PredictionStatus.Completed);
        prediction.DomainEvents.Should().ContainSingle(e => e is ForecastGeneratedEvent);

        var evt = prediction.DomainEvents.OfType<ForecastGeneratedEvent>().Single();
        evt.PredictionId.Should().Be(prediction.PredictionId);
        evt.ForecastDays.Should().Be(3);
    }

    [Fact]
    public void Complete_WithoutForecast_ShouldThrow()
    {
        var prediction = CreatePrediction();

        var act = () => prediction.Complete();

        act.Should().Throw<InvalidOperationException>().WithMessage("*forecast data*");
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldThrow()
    {
        var prediction = CreatePrediction();
        prediction.SetForecast(CreateForecastDays());
        prediction.Complete();

        var act = () => prediction.Complete();

        act.Should().Throw<InvalidOperationException>().WithMessage("*already completed*");
    }

    [Fact]
    public void MarkFailed_ShouldSetStatusToFailed()
    {
        var prediction = CreatePrediction();

        prediction.MarkFailed();

        prediction.Status.Should().Be(PredictionStatus.Failed);
    }

    [Fact]
    public void PartitionKey_ShouldBeTenantPipeUser()
    {
        var prediction = CreatePrediction();

        prediction.PartitionKey.Value.Should().Contain("|");
        prediction.PartitionKey.Value.Should().StartWith(prediction.TenantId.Value.ToString());
    }

    [Fact]
    public void FlareUpAlert_InvalidProbability_ShouldThrow()
    {
        var act = () => new FlareUpAlert(1.5, [], [], RiskLevel.Low);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TrajectoryModel_InvalidHorizon_ShouldThrow()
    {
        var act = () => new TrajectoryModel(0, [], []);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
