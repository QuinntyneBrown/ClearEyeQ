using ClearEyeQ.Treatment.Application.Interfaces;
using ClearEyeQ.Treatment.Infrastructure.Consumers;
using ClearEyeQ.Treatment.Infrastructure.Persistence;
using ClearEyeQ.Treatment.Infrastructure.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ClearEyeQ.Treatment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTreatmentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(sp =>
        {
            var connectionString = configuration.GetConnectionString("CosmosDb")
                ?? throw new InvalidOperationException("CosmosDb connection string is required.");
            return new CosmosClient(connectionString, new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            });
        });

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var connectionString = configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException("Redis connection string is required.");
            return ConnectionMultiplexer.Connect(connectionString);
        });

        services.AddScoped<ITreatmentPlanRepository, CosmosTreatmentPlanRepository>();
        services.AddScoped<IEfficacyCalculator, EfficacyCalculator>();

        services.AddScoped<DiagnosisCompletedConsumer>();
        services.AddScoped<ScanCompletedConsumer>();
        services.AddScoped<TreatmentApprovalConsumer>();

        return services;
    }
}
