using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ClearEyeQ.SharedKernel.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.Events;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.SharedKernel.Infrastructure.Messaging;

/// <summary>
/// Publishes integration event envelopes to Azure Service Bus with
/// correlation, subject, and tenant metadata on each message.
/// </summary>
public sealed class ServiceBusPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusPublisher> _logger;

    public ServiceBusPublisher(ServiceBusSender sender, ILogger<ServiceBusPublisher> logger)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Publishes an integration event envelope as a Service Bus message.
    /// </summary>
    public async Task PublishAsync(IntegrationEventEnvelope envelope, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var json = JsonSerializer.Serialize(envelope, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(json))
        {
            MessageId = envelope.EventId.ToString(),
            Subject = envelope.EventType,
            CorrelationId = envelope.CorrelationId.ToString(),
            ContentType = "application/json",
            ApplicationProperties =
            {
                ["TenantId"] = envelope.TenantId.Value.ToString(),
                ["SubjectId"] = envelope.SubjectId.ToString(),
                ["CausationId"] = envelope.CausationId.ToString(),
                ["SchemaVersion"] = envelope.SchemaVersion,
                ["EventType"] = envelope.EventType,
                ["OccurredAtUtc"] = envelope.OccurredAtUtc.ToString("O")
            }
        };

        await _sender.SendMessageAsync(message, ct);

        _logger.LogInformation(
            "Published {EventType} (EventId={EventId}, CorrelationId={CorrelationId}, TenantId={TenantId})",
            envelope.EventType, envelope.EventId, envelope.CorrelationId, envelope.TenantId);
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
    }
}
