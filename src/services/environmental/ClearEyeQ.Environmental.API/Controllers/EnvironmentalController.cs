using ClearEyeQ.Environmental.Application.Commands.CaptureSnapshot;
using ClearEyeQ.Environmental.Application.Commands.IngestScreenTime;
using ClearEyeQ.Environmental.Application.Queries.GetLatestSnapshot;
using ClearEyeQ.Environmental.Application.Queries.GetSnapshotHistory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClearEyeQ.Environmental.API.Controllers;

[ApiController]
[Route("api/environmental")]
[Authorize]
public sealed class EnvironmentalController(IMediator mediator) : ControllerBase
{
    [HttpGet("latest")]
    [ProducesResponseType(typeof(EnvironmentalSnapshotDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLatest(
        [FromQuery] Guid userId,
        [FromQuery] Guid tenantId,
        CancellationToken ct)
    {
        var query = new GetLatestSnapshotQuery(userId, tenantId);
        var result = await mediator.Send(query, ct);

        return result is not null ? Ok(result) : NotFound();
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(SnapshotHistoryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] Guid userId,
        [FromQuery] Guid tenantId,
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken ct)
    {
        var query = new GetSnapshotHistoryQuery(userId, tenantId, from, to);
        var result = await mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpPost("screen-time")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> IngestScreenTime(
        [FromBody] IngestScreenTimeCommand command,
        CancellationToken ct)
    {
        var snapshotId = await mediator.Send(command, ct);
        return Created($"/api/environmental/snapshots/{snapshotId}", new { snapshotId });
    }

    [HttpPost("capture")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CaptureSnapshot(
        [FromBody] CaptureSnapshotCommand command,
        CancellationToken ct)
    {
        var snapshotId = await mediator.Send(command, ct);
        return Created($"/api/environmental/snapshots/{snapshotId}", new { snapshotId });
    }
}
