using ClearEyeQ.Notifications.Application.Commands.SendNotification;
using ClearEyeQ.Notifications.Application.Queries.GetNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClearEyeQ.Notifications.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class NotificationController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> SendNotification(
        [FromBody] SendNotificationCommand command,
        CancellationToken ct)
    {
        var notificationId = await _mediator.Send(command, ct);
        return Created($"/api/v1/notification/{notificationId}", notificationId);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] Guid userId,
        [FromQuery] Guid tenantId,
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetNotificationsQuery(userId, tenantId, limit), ct);
        return Ok(result);
    }
}
