using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ClearEyeQ.Predictive.Application.Commands.GenerateForecast;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Predictive.Infrastructure.Consumers;

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
            "predictive-engine",
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
            "Received ScanCompleted message {MessageId} in predictive consumer",
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

        var command = new GenerateForecastCommand(scanEvent.UserId, scanEvent.TenantId);
        await _mediator.Send(command, args.CancellationToken);
        await args.CompleteMessageAsync(args.Message);

        _logger.LogInformation(
            "Processed ScanCompleted message {MessageId} for user {UserId}",
            args.Message.MessageId, scanEvent.UserId);
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception,
            "Error processing ScanCompleted message in predictive consumer. Source: {Source}",
            args.ErrorSource);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _processor.DisposeAsync().AsTask().GetAwaiter().GetResult();
        base.Dispose();
    }

    private sealed record ScanCompletedMessage(Guid ScanId, Guid UserId, Guid TenantId);
}
