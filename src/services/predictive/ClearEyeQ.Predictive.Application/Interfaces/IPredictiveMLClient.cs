using ClearEyeQ.Predictive.Domain.Entities;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Predictive.Application.Interfaces;

public interface IPredictiveMLClient
{
    Task<List<ForecastDay>> GenerateForecastAsync(
        UserId userId,
        List<TimeSeriesInput> rednessHistory,
        List<TimeSeriesInput> environmentalHistory,
        List<TimeSeriesInput> monitoringHistory,
        int forecastDays,
        CancellationToken ct);

    Task<FlareUpAlert> DetectFlareUpAsync(
        UserId userId,
        List<TimeSeriesInput> recentData,
        List<string> activeConditions,
        CancellationToken ct);

    Task<TrajectoryModel> ModelTrajectoryAsync(
        UserId userId,
        List<TimeSeriesInput> historicalScores,
        List<string> activeTreatments,
        int horizonMonths,
        CancellationToken ct);
}

public sealed record TimeSeriesInput(string Timestamp, double Value, string MetricName);
