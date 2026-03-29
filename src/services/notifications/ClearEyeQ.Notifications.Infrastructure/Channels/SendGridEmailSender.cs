using System.Net.Http.Json;
using ClearEyeQ.Notifications.Application.Interfaces;
using ClearEyeQ.Notifications.Domain.Enums;
using ClearEyeQ.Notifications.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Notifications.Infrastructure.Channels;

public sealed class SendGridEmailSender : IChannelSender
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly ILogger<SendGridEmailSender> _logger;

    public NotificationChannel Channel => NotificationChannel.Email;

    public SendGridEmailSender(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<SendGridEmailSender> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["SendGrid:ApiKey"] ?? string.Empty;
        _fromEmail = configuration["SendGrid:FromEmail"] ?? "noreply@cleareyeq.com";
        _fromName = configuration["SendGrid:FromName"] ?? "ClearEyeQ";
        _logger = logger;
    }

    public async Task<bool> SendAsync(UserId userId, NotificationContent content, CancellationToken ct)
    {
        var payload = new
        {
            personalizations = new[]
            {
                new
                {
                    to = new[] { new { email = $"user_{userId.Value}@placeholder.com" } },
                    subject = content.Title
                }
            },
            from = new { email = _fromEmail, name = _fromName },
            content = new[]
            {
                new { type = "text/html", value = $"<h2>{content.Title}</h2><p>{content.Body}</p>" }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sendgrid.com/v3/mail/send");
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {_apiKey}");
        request.Content = JsonContent.Create(payload);

        try
        {
            var response = await _httpClient.SendAsync(request, ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent to user {UserId}", userId);
                return true;
            }

            _logger.LogWarning("Email send failed for user {UserId}: {StatusCode}",
                userId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email send exception for user {UserId}", userId);
            return false;
        }
    }
}
