using ClearEyeQ.SharedKernel.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.Events;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Infrastructure.Messaging;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace ClearEyeQ.SharedKernel.Tests.Infrastructure;

public sealed class OutboxRelayTests
{
    private readonly IOutboxStore _outboxStore;
    private readonly IEventPublisher _publisher;
    private readonly IServiceScopeFactory _scopeFactory;

    public OutboxRelayTests()
    {
        _outboxStore = Substitute.For<IOutboxStore>();
        _publisher = Substitute.For<IEventPublisher>();

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IOutboxStore)).Returns(_outboxStore);
        serviceProvider.GetService(typeof(IEventPublisher)).Returns(_publisher);

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(serviceProvider);

        _scopeFactory = Substitute.For<IServiceScopeFactory>();
        _scopeFactory.CreateScope().Returns(scope);
    }

    [Fact]
    public async Task ExecuteAsync_NoPendingEvents_DoesNotPublish()
    {
        _outboxStore.GetPendingAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<IntegrationEventEnvelope>());

        using var cts = new CancellationTokenSource();

        var relay = new OutboxRelay(
            _scopeFactory,
            NullLogger<OutboxRelay>.Instance,
            pollingInterval: TimeSpan.FromMilliseconds(50));

        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        await RunRelaySafelyAsync(relay, cts.Token);

        await _publisher.DidNotReceive()
            .PublishAsync(Arg.Any<IntegrationEventEnvelope>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithPendingEvents_PublishesAndMarksPublished()
    {
        var tenantId = TenantId.New();
        var envelope = IntegrationEventEnvelope.Create(
            new { Test = "data" },
            tenantId,
            subjectId: Guid.NewGuid(),
            correlationId: Guid.NewGuid(),
            causationId: Guid.NewGuid());

        var callCount = 0;
        _outboxStore.GetPendingAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                callCount++;
                if (callCount == 1)
                    return new List<IntegrationEventEnvelope> { envelope };
                return new List<IntegrationEventEnvelope>();
            });

        using var cts = new CancellationTokenSource();

        var relay = new OutboxRelay(
            _scopeFactory,
            NullLogger<OutboxRelay>.Instance,
            pollingInterval: TimeSpan.FromMilliseconds(50));

        cts.CancelAfter(TimeSpan.FromMilliseconds(200));

        await RunRelaySafelyAsync(relay, cts.Token);

        await _publisher.Received(1)
            .PublishAsync(envelope, Arg.Any<CancellationToken>());

        await _outboxStore.Received(1)
            .MarkPublishedAsync(envelope.EventId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PublishFails_DoesNotMarkPublished()
    {
        var tenantId = TenantId.New();
        var envelope = IntegrationEventEnvelope.Create(
            new { Test = "data" },
            tenantId,
            subjectId: Guid.NewGuid(),
            correlationId: Guid.NewGuid(),
            causationId: Guid.NewGuid());

        _outboxStore.GetPendingAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<IntegrationEventEnvelope> { envelope });

        _publisher.PublishAsync(Arg.Any<IntegrationEventEnvelope>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service Bus unavailable"));

        using var cts = new CancellationTokenSource();

        var relay = new OutboxRelay(
            _scopeFactory,
            NullLogger<OutboxRelay>.Instance,
            pollingInterval: TimeSpan.FromMilliseconds(50));

        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        await RunRelaySafelyAsync(relay, cts.Token);

        await _outboxStore.DidNotReceive()
            .MarkPublishedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    private static async Task RunRelaySafelyAsync(OutboxRelay relay, CancellationToken ct)
    {
        try
        {
            await relay.StartAsync(ct);
            await Task.Delay(TimeSpan.FromMilliseconds(300), CancellationToken.None);
            await relay.StopAsync(CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation fires
        }
    }
}
