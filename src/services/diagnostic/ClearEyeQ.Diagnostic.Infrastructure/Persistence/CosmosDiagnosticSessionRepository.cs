using System.Net;
using System.Text.Json;
using ClearEyeQ.Diagnostic.Application.Interfaces;
using ClearEyeQ.Diagnostic.Domain.Aggregates;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Diagnostic.Infrastructure.Persistence;

public sealed class CosmosDiagnosticSessionRepository : IDiagnosticSessionRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosDiagnosticSessionRepository> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public CosmosDiagnosticSessionRepository(CosmosClient cosmosClient, ILogger<CosmosDiagnosticSessionRepository> logger)
    {
        _container = cosmosClient.GetContainer("ClearEyeQ", "DiagnosticSessions");
        _logger = logger;
    }

    public async Task<DiagnosticSession?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken ct)
    {
        try
        {
            var response = await _container.ReadItemAsync<DiagnosticSession>(
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

    public async Task<DiagnosticSession?> GetBySessionIdAsync(Guid sessionId, TenantId tenantId, CancellationToken ct)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
            .WithParameter("@id", sessionId.ToString());

        var iterator = _container.GetItemQueryIterator<DiagnosticSession>(
            query,
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = new Microsoft.Azure.Cosmos.PartitionKey(tenantId.Value.ToString())
            });

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(ct);
            var session = response.FirstOrDefault();
            if (session is not null)
                return session;
        }

        return null;
    }

    public async Task AddAsync(DiagnosticSession entity, CancellationToken ct)
    {
        await _container.CreateItemAsync(
            entity,
            new Microsoft.Azure.Cosmos.PartitionKey(entity.PartitionKey.Value),
            cancellationToken: ct);

        _logger.LogInformation(
            "Persisted diagnostic session {SessionId} for tenant {TenantId}",
            entity.SessionId, entity.TenantId);
    }

    public async Task UpdateAsync(DiagnosticSession entity, CancellationToken ct)
    {
        await _container.UpsertItemAsync(
            entity,
            new Microsoft.Azure.Cosmos.PartitionKey(entity.PartitionKey.Value),
            cancellationToken: ct);

        _logger.LogInformation(
            "Updated diagnostic session {SessionId} for tenant {TenantId}",
            entity.SessionId, entity.TenantId);
    }
}
