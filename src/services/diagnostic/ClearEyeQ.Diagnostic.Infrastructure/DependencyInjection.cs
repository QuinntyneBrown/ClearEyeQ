using Azure.Messaging.ServiceBus;
using ClearEyeQ.Diagnostic.Application.Interfaces;
using ClearEyeQ.Diagnostic.Infrastructure.Consumers;
using ClearEyeQ.Diagnostic.Infrastructure.ML;
using ClearEyeQ.Diagnostic.Infrastructure.ML.Proto;
using ClearEyeQ.Diagnostic.Infrastructure.Persistence;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClearEyeQ.Diagnostic.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDiagnosticInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(_ =>
        {
            var connectionString = configuration.GetConnectionString("CosmosDb")
                ?? throw new InvalidOperationException("CosmosDb connection string is required.");
            return new CosmosClient(connectionString);
        });

        services.AddScoped<IDiagnosticSessionRepository, CosmosDiagnosticSessionRepository>();

        services.AddGrpcClient<DiagnosticMLService.DiagnosticMLServiceClient>(options =>
        {
            var endpoint = configuration["ML:DiagnosticEndpoint"]
                ?? throw new InvalidOperationException("ML:DiagnosticEndpoint is required.");
            options.Address = new Uri(endpoint);
        });

        services.AddScoped<IDiagnosticMLClient, GrpcDiagnosticMLClient>();

        services.AddSingleton(_ =>
        {
            var connectionString = configuration.GetConnectionString("ServiceBus")
                ?? throw new InvalidOperationException("ServiceBus connection string is required.");
            return new ServiceBusClient(connectionString);
        });

        services.AddHostedService<ScanCompletedConsumer>();

        return services;
    }
}
