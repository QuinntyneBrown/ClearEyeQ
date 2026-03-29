using System.Net.Http.Headers;
using System.Text;
using ClearEyeQ.Notifications.Application.Interfaces;
using ClearEyeQ.Notifications.Domain.Enums;
using ClearEyeQ.Notifications.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Notifications.Infrastructure.Channels;

public sealed class TwilioSmsSender : IChannelSender
{
    private readonly HttpClient _httpClient;
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _fromNumber;
    private readonly ILogger<TwilioSmsSender> _logger;

    public NotificationChannel Channel => NotificationChannel.SMS;

    public TwilioSmsSender(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<TwilioSmsSender> logger)
    {
        _httpClient = httpClient;
        _accountSid = configuration["Twilio:AccountSid"] ?? string.Empty;
        _authToken = configuration["Twilio:AuthToken"] ?? string.Empty;
        _fromNumber = configuration["Twilio:FromNumber"] ?? string.Empty;
        _logger = logger;
    }

    public async Task<bool> SendAsync(UserId userId, NotificationContent content, CancellationToken ct)
    {
        var url = $"https://api.twilio.com/2010-04-01/Accounts/{_accountSid}/Messages.json";
        var messageBody = $"{content.Title}: {content.Body}";

        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("From", _fromNumber),
            new KeyValuePair<string, string>("To", $"+1{userId.Value.ToString()[..10]}"),
            new KeyValuePair<string, string>("Body", messageBody)
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        var authBytes = Encoding.ASCII.GetBytes($"{_accountSid}:{_authToken}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
        request.Content = formData;

        try
        {
            var response = await _httpClient.SendAsync(request, ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("SMS sent to user {UserId}", userId);
                return true;
            }

            _logger.LogWarning("SMS send failed for user {UserId}: {StatusCode}",
                userId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMS send exception for user {UserId}", userId);
            return false;
        }
    }
}
