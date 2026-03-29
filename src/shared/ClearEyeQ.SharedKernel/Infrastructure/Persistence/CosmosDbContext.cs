using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.SharedKernel.Infrastructure.Persistence;

/// <summary>
/// Wraps the Azure Cosmos DB client, providing container access and
/// database/container initialization on startup.
/// </summary>
public sealed class CosmosDbContext : IAsyncDisposable
{
    private readonly CosmosClient _client;
    private readonly string _databaseName;
    private readonly ILogger<CosmosDbContext> _logger;
    private Database? _database;

    public CosmosDbContext(
        CosmosClient client,
        string databaseName,
        ILogger<CosmosDbContext> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _databaseName = !string.IsNullOrWhiteSpace(databaseName)
            ? databaseName
            : throw new ArgumentException("Database name is required.", nameof(databaseName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initializes the database and a set of containers. Call during application startup.
    /// </summary>
    /// <param name="containerDefinitions">
    /// A dictionary of container name to partition key path (e.g., "/tenantId").
    /// </param>
    public async Task InitializeAsync(
        IDictionary<string, string> containerDefinitions,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(containerDefinitions);

        _logger.LogInformation("Initializing Cosmos DB database {DatabaseName}", _databaseName);

        var databaseResponse = await _client.CreateDatabaseIfNotExistsAsync(_databaseName, cancellationToken: ct);
        _database = databaseResponse.Database;

        foreach (var (containerName, partitionKeyPath) in containerDefinitions)
        {
            _logger.LogInformation(
                "Ensuring container {ContainerName} with partition key {PartitionKeyPath}",
                containerName, partitionKeyPath);

            await _database.CreateContainerIfNotExistsAsync(
                new ContainerProperties(containerName, partitionKeyPath),
                cancellationToken: ct);
        }

        _logger.LogInformation("Cosmos DB initialization complete for {DatabaseName}", _databaseName);
    }

    /// <summary>
    /// Gets a reference to a Cosmos DB container by name.
    /// </summary>
    public Container GetContainer(string name)
    {
        if (_database is null)
        {
            throw new InvalidOperationException(
                "CosmosDbContext has not been initialized. Call InitializeAsync first.");
        }

        return _database.GetContainer(name);
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await Task.CompletedTask;
    }
}
