using System.Text.Json;
using ClearEyeQ.Predictive.Application.Interfaces;
using ClearEyeQ.Predictive.Domain.Aggregates;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Predictive.Application.Commands.GenerateForecast;

public sealed class GenerateForecastHandler : IRequestHandler<GenerateForecastCommand, Guid>
{
    private readonly IPredictionRepository _repository;
    private readonly IPredictiveMLClient _mlClient;
    private readonly IForecastCache _cache;
    private readonly IMediator _mediator;
    private readonly ILogger<GenerateForecastHandler> _logger;

    public GenerateForecastHandler(
        IPredictionRepository repository,
        IPredictiveMLClient mlClient,
        IForecastCache cache,
        IMediator mediator,
        ILogger<GenerateForecastHandler> logger)
    {
        _repository = repository;
        _mlClient = mlClient;
        _cache = cache;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Guid> Handle(GenerateForecastCommand request, CancellationToken ct)
    {
        var userId = new UserId(request.UserId);
        var tenantId = new TenantId(request.TenantId);

        var prediction = Prediction.Create(userId, tenantId);
        await _repository.AddAsync(prediction, ct);

        _logger.LogInformation(
            "Prediction {PredictionId} created for user {UserId}, tenant {TenantId}",
            prediction.PredictionId, userId, tenantId);

        try
        {
            var rednessHistory = new List<TimeSeriesInput>();
            var environmentalHistory = new List<TimeSeriesInput>();
            var monitoringHistory = new List<TimeSeriesInput>();

            var forecast = await _mlClient.GenerateForecastAsync(
                userId,
                rednessHistory,
                environmentalHistory,
                monitoringHistory,
                request.ForecastDays,
                ct);

            prediction.SetForecast(forecast);

            var flareUp = await _mlClient.DetectFlareUpAsync(
                userId,
                rednessHistory,
                new List<string>(),
                ct);

            prediction.SetFlareUpAlert(flareUp);

            var trajectory = await _mlClient.ModelTrajectoryAsync(
                userId,
                rednessHistory,
                new List<string>(),
                horizonMonths: 6,
                ct);

            prediction.SetTrajectory(trajectory);
            prediction.Complete();

            await _repository.UpdateAsync(prediction, ct);

            var forecastJson = JsonSerializer.Serialize(forecast, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            await _cache.SetForecastAsync(userId, tenantId, forecastJson, TimeSpan.FromHours(1), ct);

            foreach (var domainEvent in prediction.DomainEvents)
            {
                await _mediator.Publish(domainEvent, ct);
            }

            prediction.ClearDomainEvents();

            _logger.LogInformation(
                "Prediction {PredictionId} completed with {Days} forecast days",
                prediction.PredictionId, forecast.Count);

            return prediction.PredictionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Prediction {PredictionId} failed for user {UserId}",
                prediction.PredictionId, userId);

            prediction.MarkFailed();
            await _repository.UpdateAsync(prediction, ct);
            throw;
        }
    }
}
