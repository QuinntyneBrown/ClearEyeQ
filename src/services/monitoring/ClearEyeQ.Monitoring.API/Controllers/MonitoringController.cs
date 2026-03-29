using ClearEyeQ.Monitoring.Application.Commands.IngestWearableData;
using ClearEyeQ.Monitoring.Application.Commands.RecordBlinkRate;
using ClearEyeQ.Monitoring.Application.Commands.RecordSleep;
using ClearEyeQ.Monitoring.Application.Queries.GetMonitoringDashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClearEyeQ.Monitoring.API.Controllers;

[ApiController]
[Route("api/monitoring")]
[Authorize]
public sealed class MonitoringController(IMediator mediator) : ControllerBase
{
    [HttpPost("wearable")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> IngestWearableData(
        [FromBody] IngestWearableDataCommand command,
        CancellationToken ct)
    {
        var sessionId = await mediator.Send(command, ct);
        return Created($"/api/monitoring/sessions/{sessionId}", new { sessionId });
    }

    [HttpPost("blink-rate")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordBlinkRate(
        [FromBody] RecordBlinkRateCommand command,
        CancellationToken ct)
    {
        var sessionId = await mediator.Send(command, ct);
        return Created($"/api/monitoring/sessions/{sessionId}", new { sessionId });
    }

    [HttpPost("sleep")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordSleep(
        [FromBody] RecordSleepCommand command,
        CancellationToken ct)
    {
        var sessionId = await mediator.Send(command, ct);
        return Created($"/api/monitoring/sessions/{sessionId}", new { sessionId });
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(MonitoringDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] Guid userId,
        [FromQuery] Guid tenantId,
        CancellationToken ct)
    {
        var query = new GetMonitoringDashboardQuery(userId, tenantId);
        var dashboard = await mediator.Send(query, ct);
        return Ok(dashboard);
    }
}
