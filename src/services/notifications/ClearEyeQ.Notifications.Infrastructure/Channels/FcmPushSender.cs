using System.Net.Http.Json;
using ClearEyeQ.Notifications.Application.Interfaces;
using ClearEyeQ.Notifications.Domain.Enums;
using ClearEyeQ.Notifications.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Notifications.Infrastructure.Channels;

public sealed class FcmPushSender : IChannelSender
{
    private readonly HttpClient _httpClient;
    private readonly string _serverKey;
    private readonly ILogger<FcmPushSender> _logger;

    public NotificationChannel Channel => NotificationChannel.Push;

    public FcmPushSender(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<FcmPushSender> logger)
    {
        _httpClient = httpClient;
        _serverKey = configuration["FCM:ServerKey"] ?? string.Empty;
        _logger = logger;
    }

    public async Task<bool> SendAsync(UserId userId, NotificationContent content, CancellationToken ct)
    {
        var payload = new
        {
            to = $"/topics/user_{userId.Value}",
            notification = new
            {
                title = content.Title,
                body = content.Body
            },
            data = content.Data
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send");
        request.Headers.TryAddWithoutValidation("Authorization", $"key={_serverKey}");
        request.Content = JsonContent.Create(payload);

        try
        {
            var response = await _httpClient.SendAsync(request, ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("FCM push sent to user {UserId}", userId);
                return true;
            }

            _logger.LogWarning("FCM push failed for user {UserId}: {StatusCode}",
                userId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FCM push exception for user {UserId}", userId);
            return false;
        }
    }
}
