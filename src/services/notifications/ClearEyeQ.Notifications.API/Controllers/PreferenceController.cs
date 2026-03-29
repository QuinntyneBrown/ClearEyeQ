using ClearEyeQ.Notifications.Application.Commands.UpdatePreferences;
using ClearEyeQ.Notifications.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClearEyeQ.Notifications.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class PreferenceController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPreferenceRepository _preferenceRepository;

    public PreferenceController(
        IMediator mediator,
        IPreferenceRepository preferenceRepository)
    {
        _mediator = mediator;
        _preferenceRepository = preferenceRepository;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPreferences(
        [FromQuery] Guid userId,
        [FromQuery] Guid tenantId,
        CancellationToken ct)
    {
        var preferences = await _preferenceRepository.GetByUserAsync(
            new UserId(userId), new TenantId(tenantId), ct);
        return Ok(preferences);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UpdatePreferencesCommand command,
        CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return NoContent();
    }
}
