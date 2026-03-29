using ClearEyeQ.Treatment.Application.Commands.ActivateTreatmentPlan;
using ClearEyeQ.Treatment.Application.Commands.CreateTreatmentPlan;
using ClearEyeQ.Treatment.Application.Commands.EvaluateEscalation;
using ClearEyeQ.Treatment.Application.Commands.ProposeAdjustment;
using ClearEyeQ.Treatment.Application.Commands.RecordEfficacy;
using ClearEyeQ.Treatment.Application.Commands.RejectTreatmentPlan;
using ClearEyeQ.Treatment.Application.Queries.GetActivePlan;
using ClearEyeQ.Treatment.Application.Queries.GetTreatmentPlan;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClearEyeQ.Treatment.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class TreatmentController : ControllerBase
{
    private readonly IMediator _mediator;

    public TreatmentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("plans")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreatePlan(
        [FromBody] CreateTreatmentPlanCommand command,
        CancellationToken ct)
    {
        var planId = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetPlan), new { planId, tenantId = command.TenantId }, planId);
    }

    [HttpGet("plans/{planId:guid}")]
    [ProducesResponseType(typeof(TreatmentPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlan(
        [FromRoute] Guid planId,
        [FromQuery] Guid tenantId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTreatmentPlanQuery(planId, tenantId), ct);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpGet("plans/active")]
    [ProducesResponseType(typeof(TreatmentPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActivePlan(
        [FromQuery] Guid userId,
        [FromQuery] Guid tenantId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetActivePlanQuery(userId, tenantId), ct);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpPost("plans/{planId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ActivatePlan(
        [FromRoute] Guid planId,
        [FromQuery] Guid tenantId,
        CancellationToken ct)
    {
        await _mediator.Send(new ActivateTreatmentPlanCommand(planId, tenantId), ct);
        return NoContent();
    }

    [HttpPost("plans/{planId:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RejectPlan(
        [FromRoute] Guid planId,
        [FromQuery] Guid tenantId,
        [FromBody] RejectPlanRequest request,
        CancellationToken ct)
    {
        await _mediator.Send(new RejectTreatmentPlanCommand(planId, tenantId, request.Reason), ct);
        return NoContent();
    }

    [HttpPost("plans/{planId:guid}/efficacy")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RecordEfficacy(
        [FromRoute] Guid planId,
        [FromQuery] Guid tenantId,
        [FromBody] RecordEfficacyRequest request,
        CancellationToken ct)
    {
        await _mediator.Send(new RecordEfficacyCommand(
            planId, tenantId, request.RednessScore, request.BaselineScore), ct);
        return NoContent();
    }

    [HttpPost("plans/{planId:guid}/evaluate-escalation")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> EvaluateEscalation(
        [FromRoute] Guid planId,
        [FromQuery] Guid tenantId,
        CancellationToken ct)
    {
        var escalated = await _mediator.Send(new EvaluateEscalationCommand(planId, tenantId), ct);
        return Ok(new { Escalated = escalated });
    }

    [HttpPost("plans/{planId:guid}/propose-adjustment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ProposeAdjustment(
        [FromRoute] Guid planId,
        [FromQuery] Guid tenantId,
        [FromBody] ProposeAdjustmentRequest request,
        CancellationToken ct)
    {
        await _mediator.Send(new ProposeAdjustmentCommand(planId, tenantId, request.Reason), ct);
        return NoContent();
    }
}

public sealed record RejectPlanRequest(string Reason);
public sealed record RecordEfficacyRequest(double RednessScore, double BaselineScore);
public sealed record ProposeAdjustmentRequest(string Reason);
