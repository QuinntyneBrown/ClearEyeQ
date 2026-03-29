using ClearEyeQ.Identity.Application.Interfaces;
using ClearEyeQ.Identity.Infrastructure.Audit;
using ClearEyeQ.Identity.Infrastructure.Auth;
using ClearEyeQ.Identity.Infrastructure.Persistence;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClearEyeQ.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Cosmos DB
        var cosmosConnectionString = configuration["CosmosDb:ConnectionString"]
            ?? throw new InvalidOperationException("CosmosDb:ConnectionString is not configured.");
        var databaseName = configuration["CosmosDb:DatabaseName"] ?? "cleareyeq-identity";

        services.AddSingleton(_ =>
        {
            var clientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            };
            return new CosmosClient(cosmosConnectionString, clientOptions);
        });

        services.AddSingleton<IUserRepository>(sp =>
        {
            var client = sp.GetRequiredService<CosmosClient>();
            return new CosmosUserRepository(client, databaseName);
        });

        services.AddSingleton<IAuditLogger>(sp =>
        {
            var client = sp.GetRequiredService<CosmosClient>();
            return new CosmosAuditLogRepository(client, databaseName);
        });

        // Auth
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<ITokenProvider, JwtTokenProvider>();

        // OAuth handlers
        services.AddHttpClient<GoogleOAuthHandler>();
        services.AddHttpClient<AppleOAuthHandler>();

        return services;
    }
}
