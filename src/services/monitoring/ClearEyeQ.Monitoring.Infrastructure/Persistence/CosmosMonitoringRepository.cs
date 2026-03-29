using System.Net;
using System.Text.Json;
using ClearEyeQ.Monitoring.Application.Interfaces;
using ClearEyeQ.Monitoring.Domain.Aggregates;
using ClearEyeQ.Monitoring.Domain.Enums;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using Microsoft.Azure.Cosmos;
using PartitionKey = ClearEyeQ.SharedKernel.Domain.ValueObjects.PartitionKey;

namespace ClearEyeQ.Monitoring.Infrastructure.Persistence;

public sealed class CosmosMonitoringRepository : IMonitoringRepository
{
    private readonly Container _container;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public CosmosMonitoringRepository(CosmosClient cosmosClient, string databaseName = "ClearEyeQ")
    {
        _container = cosmosClient.GetContainer(databaseName, "monitoring-sessions");
    }

    public async Task<MonitoringSession?> GetByIdAsync(Guid sessionId, TenantId tenantId, CancellationToken ct)
    {
        try
        {
            var response = await _container.ReadItemAsync<JsonElement>(
                sessionId.ToString(),
                new Microsoft.Azure.Cosmos.PartitionKey(tenantId.Value.ToString()),
                cancellationToken: ct);

            return DeserializeSession(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<MonitoringSession?> GetActiveSessionAsync(UserId userId, TenantId tenantId, CancellationToken ct)
    {
        var partitionKey = PartitionKey.ForUserInTenant(tenantId, userId);
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.userId = @userId AND c.status = @status ORDER BY c.createdAt DESC OFFSET 0 LIMIT 1")
            .WithParameter("@userId", userId.Value.ToString())
            .WithParameter("@status", MonitoringSessionStatus.Active.ToString());

        using var iterator = _container.GetItemQueryIterator<JsonElement>(
            query,
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = new Microsoft.Azure.Cosmos.PartitionKey(partitionKey.Value)
            });

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(ct);
            var item = response.FirstOrDefault();
            if (item.ValueKind != JsonValueKind.Undefined)
            {
                return DeserializeSession(item);
            }
        }

        return null;
    }

    public async Task AddAsync(MonitoringSession session, CancellationToken ct)
    {
        var document = SerializeSession(session);
        await _container.CreateItemAsync(
            document,
            new Microsoft.Azure.Cosmos.PartitionKey(session.PartitionKey.Value),
            cancellationToken: ct);
    }

    public async Task UpdateAsync(MonitoringSession session, CancellationToken ct)
    {
        var document = SerializeSession(session);
        await _container.UpsertItemAsync(
            document,
            new Microsoft.Azure.Cosmos.PartitionKey(session.PartitionKey.Value),
            cancellationToken: ct);
    }

    public async Task<IReadOnlyList<MonitoringSession>> GetRecentSessionsAsync(
        UserId userId, TenantId tenantId, int count, CancellationToken ct)
    {
        var partitionKey = PartitionKey.ForUserInTenant(tenantId, userId);
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.userId = @userId ORDER BY c.createdAt DESC OFFSET 0 LIMIT @count")
            .WithParameter("@userId", userId.Value.ToString())
            .WithParameter("@count", count);

        var sessions = new List<MonitoringSession>();

        using var iterator = _container.GetItemQueryIterator<JsonElement>(
            query,
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = new Microsoft.Azure.Cosmos.PartitionKey(partitionKey.Value)
            });

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(ct);
            foreach (var item in response)
            {
                var session = DeserializeSession(item);
                if (session is not null)
                    sessions.Add(session);
            }
        }

        return sessions.AsReadOnly();
    }

    private static object SerializeSession(MonitoringSession session)
    {
        return new
        {
            id = session.SessionId.ToString(),
            sessionId = session.SessionId,
            userId = session.UserId.Value.ToString(),
            tenantId = session.TenantId.Value.ToString(),
            partitionKey = session.PartitionKey.Value,
            createdAt = session.CreatedAt,
            status = session.Status.ToString(),
            wearableDataPoints = session.WearableDataPoints.Select(dp => new
            {
                id = dp.Id,
                source = dp.Source.ToString(),
                metricType = dp.MetricType.ToString(),
                value = dp.Value,
                timestamp = dp.Timestamp
            }),
            sleepRecord = session.SleepRecord is not null ? new
            {
                id = session.SleepRecord.Id,
                date = session.SleepRecord.Date.ToString("O"),
                duration = session.SleepRecord.Duration,
                stages = new
                {
                    deep = session.SleepRecord.Stages.Deep,
                    light = session.SleepRecord.Stages.Light,
                    rem = session.SleepRecord.Stages.Rem,
                    awake = session.SleepRecord.Stages.Awake
                },
                qualityScore = session.SleepRecord.QualityScore
            } : null,
            blinkRateSample = session.BlinkRateSample is not null ? new
            {
                id = session.BlinkRateSample.Id,
                blinksPerMinute = session.BlinkRateSample.BlinksPerMinute,
                fatigueScore = session.BlinkRateSample.FatigueScore,
                measuredAt = session.BlinkRateSample.MeasuredAt
            } : null,
            audit = new
            {
                createdAt = session.Audit.CreatedAt,
                createdBy = session.Audit.CreatedBy,
                modifiedAt = session.Audit.ModifiedAt,
                modifiedBy = session.Audit.ModifiedBy
            }
        };
    }

    private static MonitoringSession? DeserializeSession(JsonElement element)
    {
        var json = element.GetRawText();
        return JsonSerializer.Deserialize<MonitoringSession>(json, JsonOptions);
    }
}
