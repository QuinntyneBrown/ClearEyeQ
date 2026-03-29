using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ClearEyeQ.Predictive.Application.Commands.DetectFlareUpRisk;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Predictive.Infrastructure.Consumers;

public sealed class DiagnosisCompletedConsumer : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly IMediator _mediator;
    private readonly ILogger<DiagnosisCompletedConsumer> _logger;

    public DiagnosisCompletedConsumer(
        ServiceBusClient serviceBusClient,
        IMediator mediator,
        ILogger<DiagnosisCompletedConsumer> logger)
    {
        _processor = serviceBusClient.CreateProcessor(
            "diagnosis-completed",
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
            "Received DiagnosisCompleted message {MessageId}",
            args.Message.MessageId);

        var diagnosisEvent = JsonSerializer.Deserialize<DiagnosisCompletedMessage>(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        if (diagnosisEvent is null)
        {
            _logger.LogWarning("Failed to deserialize DiagnosisCompleted message {MessageId}", args.Message.MessageId);
            await args.DeadLetterMessageAsync(args.Message, "DeserializationFailed", "Could not deserialize message body");
            return;
        }

        var conditions = diagnosisEvent.TopConditions
            .Select(c => c.ConditionCode)
            .ToList();

        var command = new DetectFlareUpRiskCommand(diagnosisEvent.UserId, diagnosisEvent.TenantId, conditions);
        await _mediator.Send(command, args.CancellationToken);
        await args.CompleteMessageAsync(args.Message);

        _logger.LogInformation(
            "Processed DiagnosisCompleted message {MessageId} for user {UserId}",
            args.Message.MessageId, diagnosisEvent.UserId);
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception,
            "Error processing DiagnosisCompleted message. Source: {Source}",
            args.ErrorSource);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _processor.DisposeAsync().AsTask().GetAwaiter().GetResult();
        base.Dispose();
    }

    private sealed record DiagnosisCompletedMessage(
        Guid SessionId,
        Guid UserId,
        Guid TenantId,
        List<TopConditionMessage> TopConditions);

    private sealed record TopConditionMessage(
        string ConditionCode,
        string ConditionName,
        double Confidence,
        string Severity);
}
