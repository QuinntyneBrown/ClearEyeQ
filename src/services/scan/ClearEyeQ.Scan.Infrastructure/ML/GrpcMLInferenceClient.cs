using ClearEyeQ.Scan.Application.Interfaces;
using ClearEyeQ.Scan.Domain.ValueObjects;
using ClearEyeQ.Scan.Infrastructure.ML.Proto;
using Google.Protobuf;

namespace ClearEyeQ.Scan.Infrastructure.ML;

public sealed class GrpcMLInferenceClient : IMLInferenceClient
{
    private readonly ScanMLService.ScanMLServiceClient _client;

    public GrpcMLInferenceClient(ScanMLService.ScanMLServiceClient client)
    {
        _client = client;
    }

    public async Task<MLInferenceResult> ProcessScanAsync(
        string scanId,
        IReadOnlyList<byte[]> frameData,
        double ambientLightLux,
        string deviceModel,
        CancellationToken cancellationToken = default)
    {
        var request = new ProcessScanRequest
        {
            ScanId = scanId,
            AmbientLightLux = ambientLightLux,
            DeviceModel = deviceModel
        };

        foreach (var frame in frameData)
        {
            request.FrameData.Add(ByteString.CopyFrom(frame));
        }

        var response = await _client.ProcessScanAsync(
            request,
            cancellationToken: cancellationToken);

        var zoneScores = new Dictionary<string, double>(response.ZoneScores);

        var rednessScore = new RednessScore(
            response.RednessScore,
            response.RednessConfidence,
            zoneScores);

        var tearFilmMetrics = new TearFilmMetrics(
            response.TearFilmBreakUpTime,
            response.TearFilmLipidGrade,
            response.TearFilmCoverage);

        return new MLInferenceResult(rednessScore, tearFilmMetrics);
    }
}
