using Azure.Messaging.ServiceBus;
using ClearEyeQ.Predictive.Application.Interfaces;
using ClearEyeQ.Predictive.Infrastructure.Cache;
using ClearEyeQ.Predictive.Infrastructure.Consumers;
using ClearEyeQ.Predictive.Infrastructure.ML;
using ClearEyeQ.Predictive.Infrastructure.ML.Proto;
using ClearEyeQ.Predictive.Infrastructure.Persistence;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ClearEyeQ.Predictive.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPredictiveInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(_ =>
        {
            var connectionString = configuration.GetConnectionString("CosmosDb")
                ?? throw new InvalidOperationException("CosmosDb connection string is required.");
            return new CosmosClient(connectionString);
        });

        services.AddScoped<IPredictionRepository, CosmosPredictionRepository>();

        services.AddGrpcClient<PredictiveMLService.PredictiveMLServiceClient>(options =>
        {
            var endpoint = configuration["ML:PredictiveEndpoint"]
                ?? throw new InvalidOperationException("ML:PredictiveEndpoint is required.");
            options.Address = new Uri(endpoint);
        });

        services.AddScoped<IPredictiveMLClient, GrpcPredictiveMLClient>();

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var connectionString = configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException("Redis connection string is required.");
            return ConnectionMultiplexer.Connect(connectionString);
        });

        services.AddScoped<IForecastCache, RedisForecastCache>();

        services.AddSingleton(_ =>
        {
            var connectionString = configuration.GetConnectionString("ServiceBus")
                ?? throw new InvalidOperationException("ServiceBus connection string is required.");
            return new ServiceBusClient(connectionString);
        });

        services.AddHostedService<ScanCompletedConsumer>();
        services.AddHostedService<DiagnosisCompletedConsumer>();

        return services;
    }
}
