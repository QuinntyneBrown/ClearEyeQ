using ClearEyeQ.Diagnostic.Application.Commands.GenerateDiagnosis;
using ClearEyeQ.Diagnostic.Application.Queries.GetCausalGraph;
using ClearEyeQ.Diagnostic.Application.Queries.GetDiagnosticSession;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClearEyeQ.Diagnostic.API.Controllers;

[ApiController]
[Route("api/diagnostics")]
[Authorize]
public sealed class DiagnosticController : ControllerBase
{
    private readonly IMediator _mediator;

    public DiagnosticController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("generate")]
    [ProducesResponseType(typeof(GenerateDiagnosisResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateDiagnosis(
        [FromBody] GenerateDiagnosisRequest request,
        CancellationToken ct)
    {
        var command = new GenerateDiagnosisCommand(request.ScanId, request.UserId, request.TenantId);
        var sessionId = await _mediator.Send(command, ct);

        return AcceptedAtAction(
            nameof(GetSession),
            new { sessionId },
            new GenerateDiagnosisResponse(sessionId));
    }

    [HttpGet("{sessionId:guid}")]
    [ProducesResponseType(typeof(DiagnosticSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSession(Guid sessionId, [FromQuery] Guid tenantId, CancellationToken ct)
    {
        var query = new GetDiagnosticSessionQuery(sessionId, tenantId);
        var result = await _mediator.Send(query, ct);

        return result is not null ? Ok(result) : NotFound();
    }

    [HttpGet("{sessionId:guid}/causal-graph")]
    [ProducesResponseType(typeof(CausalGraphDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCausalGraph(Guid sessionId, [FromQuery] Guid tenantId, CancellationToken ct)
    {
        var query = new GetCausalGraphQuery(sessionId, tenantId);
        var result = await _mediator.Send(query, ct);

        return result is not null ? Ok(result) : NotFound();
    }
}

public sealed record GenerateDiagnosisRequest(Guid ScanId, Guid UserId, Guid TenantId);
public sealed record GenerateDiagnosisResponse(Guid SessionId);
