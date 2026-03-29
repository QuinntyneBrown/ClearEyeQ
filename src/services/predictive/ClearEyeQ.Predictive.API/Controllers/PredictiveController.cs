using ClearEyeQ.Predictive.Application.Commands.DetectFlareUpRisk;
using ClearEyeQ.Predictive.Application.Commands.GenerateForecast;
using ClearEyeQ.Predictive.Application.Queries.GetLatestForecast;
using ClearEyeQ.Predictive.Application.Queries.GetTrajectory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClearEyeQ.Predictive.API.Controllers;

[ApiController]
[Route("api/predictions")]
[Authorize]
public sealed class PredictiveController : ControllerBase
{
    private readonly IMediator _mediator;

    public PredictiveController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("forecast")]
    [ProducesResponseType(typeof(GenerateForecastResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateForecast(
        [FromBody] GenerateForecastRequest request,
        CancellationToken ct)
    {
        var command = new GenerateForecastCommand(request.UserId, request.TenantId, request.ForecastDays);
        var predictionId = await _mediator.Send(command, ct);

        return AcceptedAtAction(
            nameof(GetLatestForecast),
            new { userId = request.UserId, tenantId = request.TenantId },
            new GenerateForecastResponse(predictionId));
    }

    [HttpPost("flare-up-check")]
    [ProducesResponseType(typeof(FlareUpResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckFlareUpRisk(
        [FromBody] FlareUpCheckRequest request,
        CancellationToken ct)
    {
        var command = new DetectFlareUpRiskCommand(
            request.UserId,
            request.TenantId,
            request.ActiveConditions);

        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpGet("latest")]
    [ProducesResponseType(typeof(ForecastDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLatestForecast(
        [FromQuery] Guid userId,
        [FromQuery] Guid tenantId,
        CancellationToken ct)
    {
        var query = new GetLatestForecastQuery(userId, tenantId);
        var result = await _mediator.Send(query, ct);

        return result is not null ? Ok(result) : NotFound();
    }

    [HttpGet("trajectory")]
    [ProducesResponseType(typeof(TrajectoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrajectory(
        [FromQuery] Guid userId,
        [FromQuery] Guid tenantId,
        CancellationToken ct)
    {
        var query = new GetTrajectoryQuery(userId, tenantId);
        var result = await _mediator.Send(query, ct);

        return result is not null ? Ok(result) : NotFound();
    }
}

public sealed record GenerateForecastRequest(Guid UserId, Guid TenantId, int ForecastDays = 3);
public sealed record GenerateForecastResponse(Guid PredictionId);
public sealed record FlareUpCheckRequest(Guid UserId, Guid TenantId, List<string> ActiveConditions);
