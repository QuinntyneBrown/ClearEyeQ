using ClearEyeQ.Predictive.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Predictive.Application.Queries.GetLatestForecast;

public sealed class GetLatestForecastHandler : IRequestHandler<GetLatestForecastQuery, ForecastDto?>
{
    private readonly IPredictionRepository _repository;
    private readonly IForecastCache _cache;

    public GetLatestForecastHandler(IPredictionRepository repository, IForecastCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<ForecastDto?> Handle(GetLatestForecastQuery request, CancellationToken ct)
    {
        var userId = new UserId(request.UserId);
        var tenantId = new TenantId(request.TenantId);

        var prediction = await _repository.GetLatestByUserAsync(userId, tenantId, ct);

        if (prediction is null)
            return null;

        return new ForecastDto(
            prediction.PredictionId,
            prediction.UserId,
            prediction.GeneratedAt,
            prediction.Status.ToString(),
            prediction.Forecast.Select(f => new ForecastDayDto(
                f.Date.ToString("yyyy-MM-dd"),
                f.PredictedScore,
                f.Risk.ToString(),
                f.PrimaryFactor,
                f.ConfidenceLower,
                f.ConfidenceUpper)).ToList(),
            prediction.FlareUpAlert is not null
                ? new FlareUpAlertDto(
                    prediction.FlareUpAlert.Probability,
                    prediction.FlareUpAlert.Level.ToString(),
                    prediction.FlareUpAlert.TriggerFactors,
                    prediction.FlareUpAlert.PreventiveActions)
                : null);
    }
}
