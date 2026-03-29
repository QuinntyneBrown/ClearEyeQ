using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ClearEyeQ.Diagnostic.Application.Commands.GenerateDiagnosis;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Diagnostic.Infrastructure.Consumers;

public sealed class ScanCompletedConsumer : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly IMediator _mediator;
    private readonly ILogger<ScanCompletedConsumer> _logger;

    public ScanCompletedConsumer(
        ServiceBusClient serviceBusClient,
        IMediator mediator,
        ILogger<ScanCompletedConsumer> logger)
    {
        _processor = serviceBusClient.CreateProcessor(
            "scan-completed",
            "diagnostic-engine",
            new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 5,
                AutoCompleteMessages = false
            });

        _mediator = mediator;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;

        await _processor.StartProcessingAsync(stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

        await _processor.StopProcessingAsync(CancellationToken.None);
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        var body = args.Message.Body.ToString();

        _logger.LogInformation(
            "Received ScanCompleted message {MessageId}",
            args.Message.MessageId);

        var scanEvent = JsonSerializer.Deserialize<ScanCompletedMessage>(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        if (scanEvent is null)
        {
            _logger.LogWarning("Failed to deserialize ScanCompleted message {MessageId}", args.Message.MessageId);
            await args.DeadLetterMessageAsync(args.Message, "DeserializationFailed", "Could not deserialize message body");
            return;
        }

        var command = new GenerateDiagnosisCommand(scanEvent.ScanId, scanEvent.UserId, scanEvent.TenantId);

        await _mediator.Send(command, args.CancellationToken);
        await args.CompleteMessageAsync(args.Message);

        _logger.LogInformation(
            "Processed ScanCompleted message {MessageId} for scan {ScanId}",
            args.Message.MessageId, scanEvent.ScanId);
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception,
            "Error processing ScanCompleted message. Source: {Source}, Namespace: {Namespace}",
            args.ErrorSource, args.FullyQualifiedNamespace);

        return Task.CompletedTask;
    }

    public override async ValueTask DisposeAsync()
    {
        await _processor.DisposeAsync();
        await base.DisposeAsync();
    }

    private sealed record ScanCompletedMessage(Guid ScanId, Guid UserId, Guid TenantId);
}
