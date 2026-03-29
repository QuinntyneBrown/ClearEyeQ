using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ClearEyeQ.SharedKernel.Infrastructure.Messaging;

/// <summary>
/// Abstract base class for Service Bus consumers that provides inbox deduplication
/// using Redis. Before processing, it checks if a message has already been handled.
/// After successful processing, it marks the message as processed.
/// </summary>
public abstract class InboxConsumer<TMessage>
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger _logger;
    private readonly TimeSpan _deduplicationWindow;

    protected InboxConsumer(
        IConnectionMultiplexer redis,
        ILogger logger,
        TimeSpan? deduplicationWindow = null)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _deduplicationWindow = deduplicationWindow ?? TimeSpan.FromDays(7);
    }

    /// <summary>
    /// Processes a message with inbox deduplication. If the message has already been
    /// processed (based on its ID), it is silently skipped.
    /// </summary>
    public async Task ProcessAsync(string messageId, TMessage message, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);
        ArgumentNullException.ThrowIfNull(message);

        var db = _redis.GetDatabase();
        var inboxKey = $"inbox:{GetConsumerName()}:{messageId}";

        var alreadyProcessed = await db.StringGetAsync(inboxKey);
        if (alreadyProcessed.HasValue)
        {
            _logger.LogInformation(
                "Message {MessageId} already processed by {Consumer}. Skipping.",
                messageId, GetConsumerName());
            return;
        }

        _logger.LogInformation(
            "Processing message {MessageId} in {Consumer}",
            messageId, GetConsumerName());

        await HandleAsync(message, ct);

        await db.StringSetAsync(inboxKey, DateTimeOffset.UtcNow.ToString("O"), _deduplicationWindow);

        _logger.LogInformation(
            "Message {MessageId} processed and marked in {Consumer}",
            messageId, GetConsumerName());
    }

    /// <summary>
    /// Override this method to implement the actual message handling logic.
    /// This is called only when the message has not been previously processed.
    /// </summary>
    protected abstract Task HandleAsync(TMessage message, CancellationToken ct);

    /// <summary>
    /// Returns a unique name for this consumer, used to scope inbox deduplication keys.
    /// Defaults to the concrete type name.
    /// </summary>
    protected virtual string GetConsumerName() => GetType().Name;
}
