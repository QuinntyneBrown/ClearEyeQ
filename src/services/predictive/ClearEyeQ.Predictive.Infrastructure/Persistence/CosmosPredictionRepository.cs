using System.Net;
using ClearEyeQ.Predictive.Application.Interfaces;
using ClearEyeQ.Predictive.Domain.Aggregates;
using ClearEyeQ.Predictive.Domain.Enums;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Predictive.Infrastructure.Persistence;

public sealed class CosmosPredictionRepository : IPredictionRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosPredictionRepository> _logger;

    public CosmosPredictionRepository(CosmosClient cosmosClient, ILogger<CosmosPredictionRepository> logger)
    {
        _container = cosmosClient.GetContainer("ClearEyeQ", "Predictions");
        _logger = logger;
    }

    public async Task<Prediction?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken ct)
    {
        try
        {
            var response = await _container.ReadItemAsync<Prediction>(
                id.ToString(),
                new Microsoft.Azure.Cosmos.PartitionKey(tenantId.Value.ToString()),
                cancellationToken: ct);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Prediction?> GetLatestByUserAsync(UserId userId, TenantId tenantId, CancellationToken ct)
    {
        var partitionKey = SharedKernel.Domain.ValueObjects.PartitionKey.ForUserInTenant(tenantId, userId);

        var query = new QueryDefinition(
            "SELECT TOP 1 * FROM c WHERE c.status = @status ORDER BY c.generatedAt DESC")
            .WithParameter("@status", (int)PredictionStatus.Completed);

        var iterator = _container.GetItemQueryIterator<Prediction>(
            query,
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = new Microsoft.Azure.Cosmos.PartitionKey(partitionKey.Value)
            });

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(ct);
            var prediction = response.FirstOrDefault();
            if (prediction is not null)
                return prediction;
        }

        return null;
    }

    public async Task AddAsync(Prediction entity, CancellationToken ct)
    {
        await _container.CreateItemAsync(
            entity,
            new Microsoft.Azure.Cosmos.PartitionKey(entity.PartitionKey.Value),
            cancellationToken: ct);

        _logger.LogInformation(
            "Persisted prediction {PredictionId} for tenant {TenantId}",
            entity.PredictionId, entity.TenantId);
    }

    public async Task UpdateAsync(Prediction entity, CancellationToken ct)
    {
        await _container.UpsertItemAsync(
            entity,
            new Microsoft.Azure.Cosmos.PartitionKey(entity.PartitionKey.Value),
            cancellationToken: ct);

        _logger.LogInformation(
            "Updated prediction {PredictionId} for tenant {TenantId}",
            entity.PredictionId, entity.TenantId);
    }
}
