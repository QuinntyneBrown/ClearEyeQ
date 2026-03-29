using ClearEyeQ.Predictive.Domain.Entities;
using ClearEyeQ.Predictive.Domain.Enums;
using ClearEyeQ.Predictive.Domain.Events;
using ClearEyeQ.SharedKernel.Domain;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Predictive.Domain.Aggregates;

public sealed class Prediction : AggregateRoot
{
    public Guid PredictionId => Id;
    public UserId UserId { get; private set; }
    private TenantId _tenantId;
    public override TenantId TenantId => _tenantId;
    public override PartitionKey PartitionKey => PartitionKey.ForUserInTenant(_tenantId, UserId);
    public DateTimeOffset GeneratedAt { get; private set; }
    public List<ForecastDay> Forecast { get; private set; } = [];
    public FlareUpAlert? FlareUpAlert { get; private set; }
    public TrajectoryModel? TrajectoryModel { get; private set; }
    public PredictionStatus Status { get; private set; }

    private Prediction() { }

    public static Prediction Create(UserId userId, TenantId tenantId)
    {
        var prediction = new Prediction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            _tenantId = tenantId,
            GeneratedAt = DateTimeOffset.UtcNow,
            Status = PredictionStatus.Generating,
            Audit = AuditMetadata.Create(userId.ToString())
        };

        return prediction;
    }

    public void SetForecast(List<ForecastDay> forecast)
    {
        ArgumentNullException.ThrowIfNull(forecast);

        if (Status == PredictionStatus.Completed)
            throw new InvalidOperationException("Cannot modify a completed prediction.");

        Forecast = forecast;
    }

    public void SetFlareUpAlert(FlareUpAlert alert)
    {
        ArgumentNullException.ThrowIfNull(alert);

        if (Status == PredictionStatus.Completed)
            throw new InvalidOperationException("Cannot modify a completed prediction.");

        FlareUpAlert = alert;

        if (alert.Probability >= 0.7)
        {
            AddDomainEvent(new FlareUpWarningEvent
            {
                PredictionId = PredictionId,
                UserId = UserId,
                TenantId = _tenantId,
                Probability = alert.Probability,
                RiskLevel = alert.Level
            });
        }
    }

    public void SetTrajectory(TrajectoryModel trajectory)
    {
        ArgumentNullException.ThrowIfNull(trajectory);

        if (Status == PredictionStatus.Completed)
            throw new InvalidOperationException("Cannot modify a completed prediction.");

        TrajectoryModel = trajectory;
    }

    public void Complete()
    {
        if (Status == PredictionStatus.Completed)
            throw new InvalidOperationException("Prediction is already completed.");

        if (Forecast.Count == 0)
            throw new InvalidOperationException("Cannot complete prediction without forecast data.");

        Status = PredictionStatus.Completed;
        Audit = Audit.WithModification(UserId.ToString());

        AddDomainEvent(new ForecastGeneratedEvent
        {
            PredictionId = PredictionId,
            UserId = UserId,
            TenantId = _tenantId,
            ForecastDays = Forecast.Count
        });
    }

    public void MarkFailed()
    {
        Status = PredictionStatus.Failed;
        Audit = Audit.WithModification(UserId.ToString());
    }
}
