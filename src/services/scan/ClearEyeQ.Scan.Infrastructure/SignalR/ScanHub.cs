using ClearEyeQ.Scan.Domain.ValueObjects;
using Microsoft.AspNetCore.SignalR;

namespace ClearEyeQ.Scan.Infrastructure.SignalR;

public sealed class ScanHub : Hub
{
    /// <summary>
    /// Sends real-time positioning feedback to the connected client during eye alignment.
    /// </summary>
    public async Task SendPositioningFeedback(string scanId, PositioningFeedback feedback)
    {
        await Clients.Caller.SendAsync("PositioningFeedback", new
        {
            scanId,
            feedback.AlignmentScore,
            feedback.DirectionalHint,
            feedback.IsReady
        });
    }

    /// <summary>
    /// Sends the final scan result to the connected client after processing completes.
    /// </summary>
    public async Task SendScanResult(string userId, object scanResult)
    {
        await Clients.User(userId).SendAsync("ScanResult", scanResult);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
        }
        await base.OnDisconnectedAsync(exception);
    }
}
