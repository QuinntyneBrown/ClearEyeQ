using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ClearEyeQ.Scan.Application.Interfaces;

namespace ClearEyeQ.Scan.Infrastructure.BlobStorage;

public sealed class AzureBlobImageStore : IImageStore
{
    private readonly BlobServiceClient _blobServiceClient;
    private const string ContainerName = "scan-images";

    public AzureBlobImageStore(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> UploadAsync(
        string tenantId,
        string userId,
        string scanId,
        int frameIndex,
        Stream imageStream,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobPath = BuildBlobPath(tenantId, userId, scanId, frameIndex);
        var blobClient = containerClient.GetBlobClient(blobPath);

        await blobClient.UploadAsync(
            imageStream,
            new BlobHttpHeaders { ContentType = "image/webp" },
            cancellationToken: cancellationToken);

        return blobClient.Uri.ToString();
    }

    public Task<string> GetUrlAsync(string blobUri, CancellationToken cancellationToken = default)
    {
        // In production, generate a SAS token URL for time-limited access.
        // For now, return the stored URI directly.
        return Task.FromResult(blobUri);
    }

    public async Task DeleteAsync(string blobUri, CancellationToken cancellationToken = default)
    {
        var uri = new Uri(blobUri);
        var blobClient = new BlobClient(uri);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    private static string BuildBlobPath(string tenantId, string userId, string scanId, int frameIndex)
        => $"{tenantId}/{userId}/{scanId}/{frameIndex}.webp";
}
