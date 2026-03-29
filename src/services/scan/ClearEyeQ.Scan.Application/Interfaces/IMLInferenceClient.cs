using ClearEyeQ.Scan.Domain.ValueObjects;

namespace ClearEyeQ.Scan.Application.Interfaces;

public record MLInferenceResult(RednessScore RednessScore, TearFilmMetrics TearFilmMetrics);

public interface IMLInferenceClient
{
    Task<MLInferenceResult> ProcessScanAsync(
        string scanId,
        IReadOnlyList<byte[]> frameData,
        double ambientLightLux,
        string deviceModel,
        CancellationToken cancellationToken = default);
}
