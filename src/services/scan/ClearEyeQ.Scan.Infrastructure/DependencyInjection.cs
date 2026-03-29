using Azure.Storage.Blobs;
using ClearEyeQ.Scan.Application.Interfaces;
using ClearEyeQ.Scan.Infrastructure.BlobStorage;
using ClearEyeQ.Scan.Infrastructure.ML;
using ClearEyeQ.Scan.Infrastructure.ML.Proto;
using ClearEyeQ.Scan.Infrastructure.Persistence;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClearEyeQ.Scan.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddScanInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Cosmos DB
        var cosmosConnectionString = configuration.GetConnectionString("CosmosDb")
            ?? throw new InvalidOperationException("CosmosDb connection string is required.");
        var databaseName = configuration.GetValue<string>("CosmosDb:DatabaseName") ?? "ClearEyeQ";

        services.AddSingleton(sp =>
        {
            var client = new CosmosClient(cosmosConnectionString, new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            });
            return client;
        });

        services.AddSingleton<IScanRepository>(sp =>
            new CosmosScanRepository(sp.GetRequiredService<CosmosClient>(), databaseName));

        services.AddSingleton<IOutboxStore>(sp =>
            new CosmosOutboxStore(sp.GetRequiredService<CosmosClient>(), databaseName));

        // Azure Blob Storage
        var blobConnectionString = configuration.GetConnectionString("BlobStorage")
            ?? throw new InvalidOperationException("BlobStorage connection string is required.");
        services.AddSingleton(new BlobServiceClient(blobConnectionString));
        services.AddSingleton<IImageStore, AzureBlobImageStore>();

        // gRPC ML Inference Client
        var mlServiceUrl = configuration.GetValue<string>("MLService:Url")
            ?? "https://localhost:5101";

        services.AddGrpcClient<ScanMLService.ScanMLServiceClient>(options =>
        {
            options.Address = new Uri(mlServiceUrl);
        });

        services.AddSingleton<IMLInferenceClient, GrpcMLInferenceClient>();

        return services;
    }
}
