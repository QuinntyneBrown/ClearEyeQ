using ClearEyeQ.Environmental.Application.Interfaces;
using ClearEyeQ.Environmental.Infrastructure.ExternalApis;
using ClearEyeQ.Environmental.Infrastructure.Persistence;
using ClearEyeQ.Environmental.Infrastructure.Workers;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClearEyeQ.Environmental.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddEnvironmentalInfrastructure(
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

        services.AddScoped<IEnvironmentalSnapshotRepository>(sp =>
        {
            var cosmosClient = sp.GetRequiredService<CosmosClient>();
            var databaseName = configuration["CosmosDb:DatabaseName"] ?? "ClearEyeQ";
            return new CosmosEnvironmentalSnapshotRepository(cosmosClient, databaseName);
        });

        // External API clients
        services.AddHttpClient<OpenWeatherMapClient>();
        services.AddScoped<IAirQualityClient>(sp => sp.GetRequiredService<OpenWeatherMapClient>());
        services.AddScoped<IWeatherClient>(sp => sp.GetRequiredService<OpenWeatherMapClient>());

        services.AddHttpClient<AmbeePollenClient>();
        services.AddScoped<IPollenClient>(sp => sp.GetRequiredService<AmbeePollenClient>());

        // Background collection worker
        services.AddHostedService<EnvironmentalCollectionWorker>();

        return services;
    }
}
