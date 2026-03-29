using ClearEyeQ.Scan.Application.Commands.InitiateScan;
using ClearEyeQ.Scan.Application.Commands.ProcessScan;
using ClearEyeQ.Scan.Application.Queries.CompareScan;
using ClearEyeQ.Scan.Application.Queries.GetScanResult;
using ClearEyeQ.Scan.Domain.Enums;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClearEyeQ.Scan.API.Controllers;

[ApiController]
[Route("api/scans")]
[Authorize]
public sealed class ScanController(IMediator mediator) : ControllerBase
{
    [HttpPost("initiate")]
    [ProducesResponseType(typeof(InitiateScanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Initiate(
        [FromBody] InitiateScanRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var command = new InitiateScanCommand(
            userId,
            tenantId,
            request.EyeSide,
            request.DeviceModel);

        var scanId = await mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetResult),
            new { id = scanId.Value },
            new InitiateScanResponse(scanId.Value));
    }

    [HttpPost("{id:guid}/process")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Process(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var command = new ProcessScanCommand(new ScanId(id), tenantId);
        await mediator.Send(command, cancellationToken);
        return Accepted();
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ScanResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResult(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var query = new GetScanResultQuery(new ScanId(id), tenantId);

        try
        {
            var result = await mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpGet("{id:guid}/compare/{baselineId:guid}")]
    [ProducesResponseType(typeof(ScanComparisonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Compare(
        Guid id,
        Guid baselineId,
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var query = new CompareScanQuery(new ScanId(id), new ScanId(baselineId), tenantId);

        try
        {
            var result = await mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    private UserId GetUserId()
    {
        var claim = User.FindFirst("sub")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID claim not found.");
        return new UserId(Guid.Parse(claim));
    }

    private TenantId GetTenantId()
    {
        var claim = User.FindFirst("tenant_id")?.Value
            ?? throw new UnauthorizedAccessException("Tenant ID claim not found.");
        return new TenantId(Guid.Parse(claim));
    }
}

public sealed record InitiateScanRequest(EyeSide EyeSide, string DeviceModel);

public sealed record InitiateScanResponse(Guid ScanId);
