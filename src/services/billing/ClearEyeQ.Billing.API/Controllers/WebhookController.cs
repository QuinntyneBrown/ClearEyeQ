using ClearEyeQ.Billing.Infrastructure.Payments;
using Microsoft.AspNetCore.Mvc;

namespace ClearEyeQ.Billing.API.Controllers;

[ApiController]
[Route("api/v1/webhooks")]
public sealed class WebhookController : ControllerBase
{
    private readonly StripeWebhookHandler _webhookHandler;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        StripeWebhookHandler webhookHandler,
        ILogger<WebhookController> logger)
    {
        _webhookHandler = webhookHandler;
        _logger = logger;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook(CancellationToken ct)
    {
        using var reader = new StreamReader(HttpContext.Request.Body);
        var payload = await reader.ReadToEndAsync(ct);

        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
        if (string.IsNullOrEmpty(signature))
        {
            _logger.LogWarning("Stripe webhook received without signature");
            return BadRequest("Missing Stripe-Signature header.");
        }

        var success = await _webhookHandler.HandleWebhookAsync(payload, signature, ct);

        return success ? Ok() : BadRequest("Webhook processing failed.");
    }
}
