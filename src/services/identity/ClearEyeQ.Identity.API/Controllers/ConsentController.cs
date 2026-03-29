using System.Security.Claims;
using ClearEyeQ.Identity.Application.Commands.GrantConsent;
using ClearEyeQ.Identity.Application.Commands.RevokeConsent;
using ClearEyeQ.Identity.Application.Queries.GetUserProfile;
using ClearEyeQ.Identity.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClearEyeQ.Identity.API.Controllers;

[ApiController]
[Route("api/identity/consent")]
[Authorize]
public sealed class ConsentController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConsentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("grant")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Grant(
        [FromBody] GrantConsentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var command = new GrantConsentCommand(userId, request.ConsentType);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("revoke")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Revoke(
        [FromBody] RevokeConsentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var command = new RevokeConsentCommand(userId, request.ConsentType);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ConsentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var query = new GetUserProfileQuery(userId);
        var profile = await _mediator.Send(query, cancellationToken);
        return Ok(profile.Consents);
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User identity not found in token.");
        return Guid.Parse(sub);
    }
}

public sealed record GrantConsentRequest(ConsentType ConsentType);
public sealed record RevokeConsentRequest(ConsentType ConsentType);
