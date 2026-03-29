using ClearEyeQ.Predictive.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ClearEyeQ.Predictive.Infrastructure.Cache;

public sealed class RedisForecastCache : IForecastCache
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisForecastCache> _logger;

    public RedisForecastCache(IConnectionMultiplexer redis, ILogger<RedisForecastCache> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<string?> GetForecastAsync(UserId userId, TenantId tenantId, CancellationToken ct)
    {
        var db = _redis.GetDatabase();
        var key = BuildKey(userId, tenantId);
        var value = await db.StringGetAsync(key);

        if (value.HasValue)
        {
            _logger.LogDebug("Cache hit for forecast key {Key}", key);
            return value.ToString();
        }

        _logger.LogDebug("Cache miss for forecast key {Key}", key);
        return null;
    }

    public async Task SetForecastAsync(UserId userId, TenantId tenantId, string forecastJson, TimeSpan ttl, CancellationToken ct)
    {
        var db = _redis.GetDatabase();
        var key = BuildKey(userId, tenantId);

        await db.StringSetAsync(key, forecastJson, ttl);

        _logger.LogDebug("Cached forecast at key {Key} with TTL {TTL}", key, ttl);
    }

    public async Task InvalidateAsync(UserId userId, TenantId tenantId, CancellationToken ct)
    {
        var db = _redis.GetDatabase();
        var key = BuildKey(userId, tenantId);

        await db.KeyDeleteAsync(key);

        _logger.LogDebug("Invalidated forecast cache for key {Key}", key);
    }

    private static string BuildKey(UserId userId, TenantId tenantId) =>
        $"forecast:{tenantId.Value}:{userId.Value}";
}
