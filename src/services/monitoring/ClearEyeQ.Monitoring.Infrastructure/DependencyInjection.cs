using ClearEyeQ.Monitoring.Application.Interfaces;
using ClearEyeQ.Monitoring.Infrastructure.Persistence;
using ClearEyeQ.Monitoring.Infrastructure.Scoring;
using ClearEyeQ.Monitoring.Infrastructure.Wearables;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClearEyeQ.Monitoring.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMonitoringInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Cosmos DB
        services.AddSingleton(sp =>
        {
            var connectionString = configuration.GetConnectionString("CosmosDb")
                ?? throw new InvalidOperationException("CosmosDb connection string is not configured.");
            return new CosmosClient(connectionString, new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            });
        });

        services.AddScoped<IMonitoringRepository>(sp =>
        {
            var cosmosClient = sp.GetRequiredService<CosmosClient>();
            var databaseName = configuration["CosmosDb:DatabaseName"] ?? "ClearEyeQ";
            return new CosmosMonitoringRepository(cosmosClient, databaseName);
        });

        // Sleep scoring
        services.AddSingleton<ISleepScorer, CompositeSleepScorer>();

        // Wearable adapters
        services.AddHttpClient<HealthKitAdapter>();
        services.AddHttpClient<GoogleFitAdapter>();
        services.AddHttpClient<OuraAdapter>();

        services.AddScoped<IWearableAdapter, HealthKitAdapter>(sp =>
            sp.GetRequiredService<HealthKitAdapter>());
        services.AddScoped<IWearableAdapter, GoogleFitAdapter>(sp =>
            sp.GetRequiredService<GoogleFitAdapter>());
        services.AddScoped<IWearableAdapter, OuraAdapter>(sp =>
            sp.GetRequiredService<OuraAdapter>());

        return services;
    }
}
