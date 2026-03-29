using Azure.Messaging.ServiceBus;
using ClearEyeQ.Clinical.Application.Interfaces;
using ClearEyeQ.Clinical.Infrastructure.Messaging;
using ClearEyeQ.Clinical.Infrastructure.Persistence;
using ClearEyeQ.Clinical.Infrastructure.Projectors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ClearEyeQ.Clinical.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddClinicalInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // PostgreSQL read model store
        services.AddDbContext<ClinicalDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("ClinicalDb"),
                npgsql => npgsql.MigrationsAssembly(typeof(ClinicalDbContext).Assembly.FullName)));

        // Redis for inbox deduplication
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(
                configuration.GetConnectionString("Redis") ?? "localhost:6379"));

        // Repositories and stores
        services.AddScoped<IPatientReadModelStore, EfPatientReadModelStore>();
        services.AddScoped<IReferralRepository, EfReferralRepository>();
        services.AddScoped<IClinicalNoteRepository, EfClinicalNoteRepository>();

        // Azure Service Bus
        services.AddSingleton(sp =>
        {
            var connectionString = configuration.GetConnectionString("ServiceBus")
                ?? throw new InvalidOperationException("ServiceBus connection string is not configured.");
            return new ServiceBusClient(connectionString);
        });

        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            var topicName = configuration["ServiceBus:TopicName"] ?? "clinical-events";
            return client.CreateSender(topicName);
        });

        services.AddSingleton<IIntegrationEventPublisher, ServiceBusIntegrationEventPublisher>();

        // Projectors
        services.AddScoped<ScanCompletedProjector>();
        services.AddScoped<DiagnosisCompletedProjector>();
        services.AddScoped<TreatmentPlanProposedProjector>();
        services.AddScoped<EscalationProjector>();

        return services;
    }
}
