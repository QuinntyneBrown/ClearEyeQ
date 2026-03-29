using System.Net;
using System.Text.Json;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using ClearEyeQ.Treatment.Application.Interfaces;
using ClearEyeQ.Treatment.Domain.Aggregates;
using ClearEyeQ.Treatment.Domain.Enums;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Treatment.Infrastructure.Persistence;

public sealed class CosmosTreatmentPlanRepository : ITreatmentPlanRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosTreatmentPlanRepository> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public CosmosTreatmentPlanRepository(
        CosmosClient cosmosClient,
        ILogger<CosmosTreatmentPlanRepository> logger)
    {
        _container = cosmosClient.GetContainer("ClearEyeQ", "TreatmentPlans");
        _logger = logger;
    }

    public async Task<TreatmentPlan?> GetByIdAsync(Guid planId, TenantId tenantId, CancellationToken ct)
    {
        try
        {
            var response = await _container.ReadItemAsync<TreatmentPlan>(
                planId.ToString(),
                new Microsoft.Azure.Cosmos.PartitionKey(tenantId.Value.ToString()),
                cancellationToken: ct);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogDebug("Treatment plan {PlanId} not found in tenant {TenantId}", planId, tenantId);
            return null;
        }
    }

    public async Task<TreatmentPlan?> GetActivePlanAsync(UserId userId, TenantId tenantId, CancellationToken ct)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.userId.value = @userId AND c.status = @status")
            .WithParameter("@userId", userId.Value.ToString())
            .WithParameter("@status", (int)TreatmentStatus.Active);

        var iterator = _container.GetItemQueryIterator<TreatmentPlan>(
            query,
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = new Microsoft.Azure.Cosmos.PartitionKey(
                    SharedKernel.Domain.ValueObjects.PartitionKey.ForUserInTenant(tenantId, userId).Value)
            });

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(ct);
            return response.FirstOrDefault();
        }

        return null;
    }

    public async Task<IReadOnlyList<TreatmentPlan>> GetActivePlansAsync(TenantId tenantId, CancellationToken ct)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.status = @status")
            .WithParameter("@status", (int)TreatmentStatus.Active);

        var results = new List<TreatmentPlan>();
        var iterator = _container.GetItemQueryIterator<TreatmentPlan>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(ct);
            results.AddRange(response);
        }

        return results.AsReadOnly();
    }

    public async Task AddAsync(TreatmentPlan plan, CancellationToken ct)
    {
        await _container.CreateItemAsync(
            plan,
            new Microsoft.Azure.Cosmos.PartitionKey(plan.PartitionKey.Value),
            cancellationToken: ct);

        _logger.LogInformation("Created treatment plan {PlanId} for tenant {TenantId}",
            plan.PlanId, plan.TenantId);
    }

    public async Task UpdateAsync(TreatmentPlan plan, CancellationToken ct)
    {
        await _container.UpsertItemAsync(
            plan,
            new Microsoft.Azure.Cosmos.PartitionKey(plan.PartitionKey.Value),
            cancellationToken: ct);

        _logger.LogInformation("Updated treatment plan {PlanId} for tenant {TenantId}",
            plan.PlanId, plan.TenantId);
    }
}
