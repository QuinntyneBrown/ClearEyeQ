namespace ClearEyeQ.Scan.Application.Interfaces;

public interface IImageStore
{
    Task<string> UploadAsync(string tenantId, string userId, string scanId, int frameIndex, Stream imageStream, CancellationToken cancellationToken = default);
    Task<string> GetUrlAsync(string blobUri, CancellationToken cancellationToken = default);
    Task DeleteAsync(string blobUri, CancellationToken cancellationToken = default);
}
