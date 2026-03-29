using ClearEyeQ.Clinical.Application.Commands.ApproveTreatmentPlan;
using ClearEyeQ.Clinical.Application.Commands.RejectTreatmentPlan;
using ClearEyeQ.Clinical.Application.Queries.GetTreatmentReviewQueue;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClearEyeQ.Clinical.API.Controllers;

[ApiController]
[Route("api/clinical/treatment-reviews")]
[Authorize]
public sealed class TreatmentReviewController : ControllerBase
{
    private readonly IMediator _mediator;

    public TreatmentReviewController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TreatmentReviewDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviewQueue(
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTreatmentReviewQueueQuery(tenantId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Approve(
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        Guid id,
        [FromBody] ReviewActionRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new ApproveTreatmentPlanCommand(tenantId, id, request.ClinicianId, request.Rationale), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reject(
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        Guid id,
        [FromBody] ReviewActionRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new RejectTreatmentPlanCommand(tenantId, id, request.ClinicianId, request.Rationale), cancellationToken);
        return NoContent();
    }
}

public sealed record ReviewActionRequest(string ClinicianId, string Rationale);
