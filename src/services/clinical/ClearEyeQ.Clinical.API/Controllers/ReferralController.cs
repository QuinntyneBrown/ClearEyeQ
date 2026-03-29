using ClearEyeQ.Clinical.Application.Commands.AcceptReferral;
using ClearEyeQ.Clinical.Application.Commands.DeclineReferral;
using ClearEyeQ.Clinical.Application.Queries.GetReferralInbox;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClearEyeQ.Clinical.API.Controllers;

[ApiController]
[Route("api/clinical/referrals")]
[Authorize]
public sealed class ReferralController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReferralController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ReferralDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReferrals(
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetReferralInboxQuery(tenantId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/accept")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Accept(
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        Guid id,
        [FromBody] ReferralActionRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new AcceptReferralCommand(tenantId, id, request.ClinicianId, request.Rationale), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/decline")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Decline(
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        Guid id,
        [FromBody] ReferralActionRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeclineReferralCommand(tenantId, id, request.ClinicianId, request.Rationale), cancellationToken);
        return NoContent();
    }
}

public sealed record ReferralActionRequest(string ClinicianId, string Rationale);
