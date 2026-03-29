namespace ClearEyeQ.Scan.Domain.Entities;

public sealed class ScanImage
{
    public Guid Id { get; private set; }
    public int FrameIndex { get; private set; }
    public string BlobUri { get; private set; }
    public double QualityScore { get; private set; }
    public bool IsSelected { get; private set; }

    private ScanImage() { BlobUri = string.Empty; }

    public ScanImage(int frameIndex, string blobUri, double qualityScore)
    {
        if (frameIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(frameIndex), "Frame index cannot be negative.");

        if (string.IsNullOrWhiteSpace(blobUri))
            throw new ArgumentException("Blob URI is required.", nameof(blobUri));

        if (qualityScore < 0 || qualityScore > 1)
            throw new ArgumentOutOfRangeException(nameof(qualityScore), "Quality score must be between 0 and 1.");

        Id = Guid.NewGuid();
        FrameIndex = frameIndex;
        BlobUri = blobUri;
        QualityScore = qualityScore;
        IsSelected = false;
    }

    public void Select() => IsSelected = true;

    public void Deselect() => IsSelected = false;
}
