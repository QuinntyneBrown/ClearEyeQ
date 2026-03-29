using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ClearEyeQ.Clinical.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.Events;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Clinical.Infrastructure.Messaging;

/// <summary>
/// Publishes integration events to Azure Service Bus topics.
/// </summary>
public sealed class ServiceBusIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusIntegrationEventPublisher> _logger;

    public ServiceBusIntegrationEventPublisher(
        ServiceBusSender sender,
        ILogger<ServiceBusIntegrationEventPublisher> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task PublishAsync(IntegrationEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var body = JsonSerializer.Serialize(envelope, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        var message = new ServiceBusMessage(body)
        {
            MessageId = envelope.EventId.ToString(),
            Subject = envelope.EventType,
            ContentType = "application/json",
            ApplicationProperties =
            {
                ["EventType"] = envelope.EventType,
                ["SchemaVersion"] = envelope.SchemaVersion,
                ["TenantId"] = envelope.TenantId.Value.ToString()
            }
        };

        _logger.LogInformation(
            "Publishing integration event {EventType} with ID {EventId} for tenant {TenantId}",
            envelope.EventType, envelope.EventId, envelope.TenantId);

        await _sender.SendMessageAsync(message, cancellationToken);
    }
}
