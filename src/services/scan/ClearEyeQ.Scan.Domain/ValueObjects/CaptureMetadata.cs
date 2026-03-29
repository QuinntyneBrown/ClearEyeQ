namespace ClearEyeQ.Scan.Domain.ValueObjects;

public sealed record CaptureMetadata
{
    public string DeviceModel { get; }
    public int FrameCount { get; }
    public TimeSpan BurstDuration { get; }
    public double AmbientLightLux { get; }

    public CaptureMetadata(string deviceModel, int frameCount, TimeSpan burstDuration, double ambientLightLux)
    {
        if (string.IsNullOrWhiteSpace(deviceModel))
            throw new ArgumentException("Device model is required.", nameof(deviceModel));

        if (frameCount < 0)
            throw new ArgumentOutOfRangeException(nameof(frameCount), "Frame count cannot be negative.");

        if (ambientLightLux < 0)
            throw new ArgumentOutOfRangeException(nameof(ambientLightLux), "Ambient light lux cannot be negative.");

        DeviceModel = deviceModel;
        FrameCount = frameCount;
        BurstDuration = burstDuration;
        AmbientLightLux = ambientLightLux;
    }
}
