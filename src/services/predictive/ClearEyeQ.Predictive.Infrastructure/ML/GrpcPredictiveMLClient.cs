using ClearEyeQ.Predictive.Application.Interfaces;
using ClearEyeQ.Predictive.Domain.Enums;
using ClearEyeQ.Predictive.Infrastructure.ML.Proto;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using DomainForecastDay = ClearEyeQ.Predictive.Domain.Entities.ForecastDay;
using DomainFlareUpAlert = ClearEyeQ.Predictive.Domain.Entities.FlareUpAlert;
using DomainTrajectoryModel = ClearEyeQ.Predictive.Domain.Entities.TrajectoryModel;
using DomainTrajectoryPoint = ClearEyeQ.Predictive.Domain.ValueObjects.TrajectoryPoint;

namespace ClearEyeQ.Predictive.Infrastructure.ML;

public sealed class GrpcPredictiveMLClient : IPredictiveMLClient
{
    private readonly PredictiveMLService.PredictiveMLServiceClient _client;
    private readonly ILogger<GrpcPredictiveMLClient> _logger;

    public GrpcPredictiveMLClient(
        PredictiveMLService.PredictiveMLServiceClient client,
        ILogger<GrpcPredictiveMLClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<List<DomainForecastDay>> GenerateForecastAsync(
        UserId userId,
        List<TimeSeriesInput> rednessHistory,
        List<TimeSeriesInput> environmentalHistory,
        List<TimeSeriesInput> monitoringHistory,
        int forecastDays,
        CancellationToken ct)
    {
        var request = new ForecastRequest
        {
            UserId = userId.Value.ToString(),
            ForecastDays = forecastDays
        };

        foreach (var ts in rednessHistory)
        {
            request.RednessHistory.Add(new TimeSeriesPoint
            {
                Timestamp = ts.Timestamp,
                Value = ts.Value,
                MetricName = ts.MetricName
            });
        }

        foreach (var ts in environmentalHistory)
        {
            request.EnvironmentalHistory.Add(new TimeSeriesPoint
            {
                Timestamp = ts.Timestamp,
                Value = ts.Value,
                MetricName = ts.MetricName
            });
        }

        foreach (var ts in monitoringHistory)
        {
            request.MonitoringHistory.Add(new TimeSeriesPoint
            {
                Timestamp = ts.Timestamp,
                Value = ts.Value,
                MetricName = ts.MetricName
            });
        }

        _logger.LogInformation("Calling GenerateForecast for user {UserId}", userId);

        var response = await _client.GenerateForecastAsync(request, cancellationToken: ct);

        return response.Days.Select(d => new DomainForecastDay(
            DateOnly.Parse(d.Date),
            d.PredictedScore,
            ParseRiskLevel(d.RiskLevel),
            d.PrimaryFactor,
            d.ConfidenceLower,
            d.ConfidenceUpper)).ToList();
    }

    public async Task<DomainFlareUpAlert> DetectFlareUpAsync(
        UserId userId,
        List<TimeSeriesInput> recentData,
        List<string> activeConditions,
        CancellationToken ct)
    {
        var request = new FlareUpRequest
        {
            UserId = userId.Value.ToString()
        };

        foreach (var ts in recentData)
        {
            request.RecentData.Add(new TimeSeriesPoint
            {
                Timestamp = ts.Timestamp,
                Value = ts.Value,
                MetricName = ts.MetricName
            });
        }

        request.ActiveConditions.AddRange(activeConditions);

        _logger.LogInformation("Calling DetectFlareUpRisk for user {UserId}", userId);

        var response = await _client.DetectFlareUpRiskAsync(request, cancellationToken: ct);

        return new DomainFlareUpAlert(
            response.Probability,
            response.TriggerFactors.ToList(),
            response.PreventiveActions.ToList(),
            ParseRiskLevel(response.RiskLevel));
    }

    public async Task<DomainTrajectoryModel> ModelTrajectoryAsync(
        UserId userId,
        List<TimeSeriesInput> historicalScores,
        List<string> activeTreatments,
        int horizonMonths,
        CancellationToken ct)
    {
        var request = new TrajectoryRequest
        {
            UserId = userId.Value.ToString(),
            HorizonMonths = horizonMonths
        };

        foreach (var ts in historicalScores)
        {
            request.HistoricalScores.Add(new TimeSeriesPoint
            {
                Timestamp = ts.Timestamp,
                Value = ts.Value,
                MetricName = ts.MetricName
            });
        }

        request.ActiveTreatments.AddRange(activeTreatments);

        _logger.LogInformation("Calling ModelTrajectory for user {UserId}", userId);

        var response = await _client.ModelTrajectoryAsync(request, cancellationToken: ct);

        return new DomainTrajectoryModel(
            horizonMonths,
            response.WithTreatment.Select(t => new DomainTrajectoryPoint(
                DateOnly.Parse(t.Date),
                t.ProjectedScore,
                t.ConfidenceLower,
                t.ConfidenceUpper)).ToList(),
            response.WithoutTreatment.Select(t => new DomainTrajectoryPoint(
                DateOnly.Parse(t.Date),
                t.ProjectedScore,
                t.ConfidenceLower,
                t.ConfidenceUpper)).ToList());
    }

    private static RiskLevel ParseRiskLevel(string level)
    {
        return Enum.TryParse<RiskLevel>(level, ignoreCase: true, out var result)
            ? result
            : RiskLevel.Low;
    }
}
