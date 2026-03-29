using Azure.Storage.Blobs;
using ClearEyeQ.Fhir.Application.Interfaces;
using ClearEyeQ.Fhir.Infrastructure.Persistence;
using ClearEyeQ.Fhir.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClearEyeQ.Fhir.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFhirInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Azure Blob Storage for FHIR bundles
        services.AddSingleton(_ =>
        {
            var connectionString = configuration.GetConnectionString("BlobStorage")
                ?? throw new InvalidOperationException("BlobStorage connection string is not configured.");
            var containerName = configuration["BlobStorage:ContainerName"] ?? "fhir-bundles";
            var serviceClient = new BlobServiceClient(connectionString);
            return serviceClient.GetBlobContainerClient(containerName);
        });

        services.AddSingleton<IFhirBundleStore, AzureBlobFhirBundleStore>();

        // HTTP clients for cross-context data gathering
        services.AddHttpClient("ClinicalApi", client =>
        {
            client.BaseAddress = new Uri(configuration["ServiceUrls:ClinicalApi"] ?? "http://localhost:5010");
        });

        services.AddHttpClient("IdentityApi", client =>
        {
            client.BaseAddress = new Uri(configuration["ServiceUrls:IdentityApi"] ?? "http://localhost:5001");
        });

        services.AddScoped<IContextDataGatherer, CrossContextDataGatherer>();

        return services;
    }
}
