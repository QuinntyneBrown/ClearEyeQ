using ClearEyeQ.Billing.Application.Commands.CancelSubscription;
using ClearEyeQ.Billing.Application.Commands.CreateSubscription;
using ClearEyeQ.Billing.Application.Commands.RecordUsage;
using ClearEyeQ.Billing.Application.Commands.UpgradePlan;
using ClearEyeQ.Billing.Application.Queries.CheckFeatureAccess;
using ClearEyeQ.Billing.Application.Queries.GetSubscription;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClearEyeQ.Billing.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class BillingController : ControllerBase
{
    private readonly IMediator _mediator;

    public BillingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("subscriptions")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSubscription(
        [FromBody] CreateSubscriptionCommand command,
        CancellationToken ct)
    {
        var subscriptionId = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetSubscription), new { tenantId = command.TenantId }, subscriptionId);
    }

    [HttpGet("subscriptions")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubscription(
        [FromQuery] Guid tenantId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSubscriptionQuery(tenantId), ct);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpPost("subscriptions/{subscriptionId:guid}/upgrade")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpgradePlan(
        [FromRoute] Guid subscriptionId,
        [FromBody] UpgradePlanCommand command,
        CancellationToken ct)
    {
        await _mediator.Send(command with { SubscriptionId = subscriptionId }, ct);
        return NoContent();
    }

    [HttpPost("subscriptions/{subscriptionId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CancelSubscription(
        [FromRoute] Guid subscriptionId,
        CancellationToken ct)
    {
        await _mediator.Send(new CancelSubscriptionCommand(subscriptionId), ct);
        return NoContent();
    }

    [HttpPost("usage")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RecordUsage(
        [FromBody] RecordUsageCommand command,
        CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return NoContent();
    }

    [HttpGet("features/check")]
    [ProducesResponseType(typeof(FeatureAccessDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckFeatureAccess(
        [FromQuery] Guid tenantId,
        [FromQuery] string featureName,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new CheckFeatureAccessQuery(tenantId, featureName), ct);
        return Ok(result);
    }
}
