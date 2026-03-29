using ClearEyeQ.SharedKernel.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.SharedKernel.Infrastructure.Messaging;

/// <summary>
/// Background service that polls the transactional outbox at a fixed interval,
/// publishes pending integration events to Service Bus, and marks them as published.
/// </summary>
public sealed class OutboxRelay : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxRelay> _logger;
    private readonly TimeSpan _pollingInterval;
    private readonly int _batchSize;

    public OutboxRelay(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxRelay> logger,
        TimeSpan? pollingInterval = null,
        int batchSize = 50)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pollingInterval = pollingInterval ?? TimeSpan.FromSeconds(5);
        _batchSize = batchSize;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "OutboxRelay started. Polling every {Interval}s with batch size {BatchSize}",
            _pollingInterval.TotalSeconds, _batchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingEventsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutboxRelay encountered an error during polling cycle");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("OutboxRelay stopped");
    }

    private async Task ProcessPendingEventsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var outboxStore = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        var pendingEvents = await outboxStore.GetPendingAsync(_batchSize, ct);

        if (pendingEvents.Count == 0)
        {
            return;
        }

        _logger.LogInformation("OutboxRelay found {Count} pending events", pendingEvents.Count);

        foreach (var envelope in pendingEvents)
        {
            try
            {
                await publisher.PublishAsync(envelope, ct);
                await outboxStore.MarkPublishedAsync(envelope.EventId, ct);

                _logger.LogDebug(
                    "OutboxRelay published and marked event {EventId} ({EventType})",
                    envelope.EventId, envelope.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "OutboxRelay failed to publish event {EventId} ({EventType}). Will retry next cycle.",
                    envelope.EventId, envelope.EventType);
            }
        }
    }
}
