using ClearEyeQ.Predictive.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Predictive.Application.Commands.DetectFlareUpRisk;

public sealed class DetectFlareUpRiskHandler : IRequestHandler<DetectFlareUpRiskCommand, FlareUpResultDto>
{
    private readonly IPredictiveMLClient _mlClient;
    private readonly ILogger<DetectFlareUpRiskHandler> _logger;

    public DetectFlareUpRiskHandler(
        IPredictiveMLClient mlClient,
        ILogger<DetectFlareUpRiskHandler> logger)
    {
        _mlClient = mlClient;
        _logger = logger;
    }

    public async Task<FlareUpResultDto> Handle(DetectFlareUpRiskCommand request, CancellationToken ct)
    {
        var userId = new UserId(request.UserId);

        _logger.LogInformation("Detecting flare-up risk for user {UserId}", userId);

        var recentData = new List<TimeSeriesInput>();

        var alert = await _mlClient.DetectFlareUpAsync(
            userId,
            recentData,
            request.ActiveConditions,
            ct);

        _logger.LogInformation(
            "Flare-up detection completed for user {UserId}: probability={Probability}, level={Level}",
            userId, alert.Probability, alert.Level);

        return new FlareUpResultDto(
            alert.Probability,
            alert.Level.ToString(),
            alert.TriggerFactors,
            alert.PreventiveActions);
    }
}
