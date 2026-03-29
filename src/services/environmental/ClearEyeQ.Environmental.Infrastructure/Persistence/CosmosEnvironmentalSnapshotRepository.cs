using System.Net;
using System.Text.Json;
using ClearEyeQ.Environmental.Application.Interfaces;
using ClearEyeQ.Environmental.Domain.Aggregates;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using Microsoft.Azure.Cosmos;
using PartitionKey = ClearEyeQ.SharedKernel.Domain.ValueObjects.PartitionKey;

namespace ClearEyeQ.Environmental.Infrastructure.Persistence;

public sealed class CosmosEnvironmentalSnapshotRepository : IEnvironmentalSnapshotRepository
{
    private readonly Container _container;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public CosmosEnvironmentalSnapshotRepository(CosmosClient cosmosClient, string databaseName = "ClearEyeQ")
    {
        _container = cosmosClient.GetContainer(databaseName, "environmental-snapshots");
    }

    public async Task<EnvironmentalSnapshot?> GetByIdAsync(Guid snapshotId, TenantId tenantId, CancellationToken ct)
    {
        try
        {
            var response = await _container.ReadItemAsync<JsonElement>(
                snapshotId.ToString(),
                new Microsoft.Azure.Cosmos.PartitionKey(tenantId.Value.ToString()),
                cancellationToken: ct);

            return DeserializeSnapshot(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<EnvironmentalSnapshot?> GetLatestAsync(UserId userId, TenantId tenantId, CancellationToken ct)
    {
        var partitionKey = PartitionKey.ForUserInTenant(tenantId, userId);
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.userId = @userId ORDER BY c.capturedAt DESC OFFSET 0 LIMIT 1")
            .WithParameter("@userId", userId.Value.ToString());

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
                return DeserializeSnapshot(item);
            }
        }

        return null;
    }

    public async Task<IReadOnlyList<EnvironmentalSnapshot>> GetHistoryAsync(
        UserId userId, TenantId tenantId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct)
    {
        var partitionKey = PartitionKey.ForUserInTenant(tenantId, userId);
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.userId = @userId AND c.capturedAt >= @from AND c.capturedAt <= @to ORDER BY c.capturedAt DESC")
            .WithParameter("@userId", userId.Value.ToString())
            .WithParameter("@from", from)
            .WithParameter("@to", to);

        var snapshots = new List<EnvironmentalSnapshot>();

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
                var snapshot = DeserializeSnapshot(item);
                if (snapshot is not null)
                    snapshots.Add(snapshot);
            }
        }

        return snapshots.AsReadOnly();
    }

    public async Task AddAsync(EnvironmentalSnapshot snapshot, CancellationToken ct)
    {
        var document = SerializeSnapshot(snapshot);
        await _container.CreateItemAsync(
            document,
            new Microsoft.Azure.Cosmos.PartitionKey(snapshot.PartitionKey.Value),
            cancellationToken: ct);
    }

    private static object SerializeSnapshot(EnvironmentalSnapshot snapshot)
    {
        return new
        {
            id = snapshot.SnapshotId.ToString(),
            snapshotId = snapshot.SnapshotId,
            userId = snapshot.UserId.Value.ToString(),
            tenantId = snapshot.TenantId.Value.ToString(),
            partitionKey = snapshot.PartitionKey.Value,
            capturedAt = snapshot.CapturedAt,
            airQualityReading = snapshot.AirQualityReading is not null ? new
            {
                aqi = snapshot.AirQualityReading.Aqi,
                pm25 = snapshot.AirQualityReading.Pm25,
                pm10 = snapshot.AirQualityReading.Pm10,
                level = snapshot.AirQualityReading.Level.ToString()
            } : null,
            pollenCount = snapshot.PollenCount is not null ? new
            {
                tree = snapshot.PollenCount.Tree,
                grass = snapshot.PollenCount.Grass,
                weed = snapshot.PollenCount.Weed,
                overallLevel = snapshot.PollenCount.OverallLevel
            } : null,
            uvIndex = snapshot.UvIndex is not null ? new
            {
                value = snapshot.UvIndex.Value,
                riskCategory = snapshot.UvIndex.RiskCategory
            } : null,
            humidityReading = snapshot.HumidityReading is not null ? new
            {
                percentage = snapshot.HumidityReading.Percentage
            } : null,
            screenTimeRecord = snapshot.ScreenTimeRecord is not null ? new
            {
                totalDuration = snapshot.ScreenTimeRecord.TotalDuration,
                appBreakdown = snapshot.ScreenTimeRecord.AppBreakdown
            } : null,
            geoLocation = snapshot.GeoLocation is not null ? new
            {
                latitude = snapshot.GeoLocation.Latitude,
                longitude = snapshot.GeoLocation.Longitude
            } : null,
            audit = new
            {
                createdAt = snapshot.Audit.CreatedAt,
                createdBy = snapshot.Audit.CreatedBy,
                modifiedAt = snapshot.Audit.ModifiedAt,
                modifiedBy = snapshot.Audit.ModifiedBy
            }
        };
    }

    private static EnvironmentalSnapshot? DeserializeSnapshot(JsonElement element)
    {
        var json = element.GetRawText();
        return JsonSerializer.Deserialize<EnvironmentalSnapshot>(json, JsonOptions);
    }
}
