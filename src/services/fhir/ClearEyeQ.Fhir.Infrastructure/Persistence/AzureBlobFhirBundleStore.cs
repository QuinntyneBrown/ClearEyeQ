using System.Text;
using Azure.Storage.Blobs;
using ClearEyeQ.Fhir.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Fhir.Infrastructure.Persistence;

/// <summary>
/// Stores FHIR export bundles in Azure Blob Storage.
/// </summary>
public sealed class AzureBlobFhirBundleStore : IFhirBundleStore
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<AzureBlobFhirBundleStore> _logger;

    public AzureBlobFhirBundleStore(
        BlobContainerClient containerClient,
        ILogger<AzureBlobFhirBundleStore> logger)
    {
        _containerClient = containerClient;
        _logger = logger;
    }

    public async Task<string> StoreAsync(Guid tenantId, Guid patientId, string bundleJson, CancellationToken cancellationToken = default)
    {
        var blobName = $"{tenantId}/{patientId}/export-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}.json";
        var blobClient = _containerClient.GetBlobClient(blobName);

        var content = new BinaryData(Encoding.UTF8.GetBytes(bundleJson));
        await blobClient.UploadAsync(content, overwrite: true, cancellationToken);

        _logger.LogInformation(
            "Stored FHIR bundle for patient {PatientId} in tenant {TenantId} at {BlobName}",
            patientId, tenantId, blobName);

        return blobName;
    }

    public async Task<string?> RetrieveAsync(Guid tenantId, string blobName, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            _logger.LogWarning("FHIR bundle blob {BlobName} not found for tenant {TenantId}", blobName, tenantId);
            return null;
        }

        var response = await blobClient.DownloadContentAsync(cancellationToken);
        return response.Value.Content.ToString();
    }
}
