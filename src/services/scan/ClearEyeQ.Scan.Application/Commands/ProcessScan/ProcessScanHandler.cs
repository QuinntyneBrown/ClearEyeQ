using ClearEyeQ.Scan.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.Events;
using MediatR;

namespace ClearEyeQ.Scan.Application.Commands.ProcessScan;

public sealed class ProcessScanHandler(
    IScanRepository scanRepository,
    IMLInferenceClient mlInferenceClient,
    IImageStore imageStore,
    IOutboxStore outboxStore)
    : IRequestHandler<ProcessScanCommand>
{
    public async Task Handle(ProcessScanCommand request, CancellationToken cancellationToken)
    {
        var scan = await scanRepository.GetByIdAsync(request.ScanId, request.TenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Scan {request.ScanId} not found.");

        scan.MarkProcessing();

        // Collect frame data from blob storage for ML processing
        var frameData = new List<byte[]>();
        foreach (var image in scan.Images)
        {
            var url = await imageStore.GetUrlAsync(image.BlobUri, cancellationToken);
            // In production, we'd download the blob bytes here.
            // For now, use the blob URI as a reference; the ML client handles retrieval.
            frameData.Add(System.Text.Encoding.UTF8.GetBytes(url));
        }

        try
        {
            var result = await mlInferenceClient.ProcessScanAsync(
                scan.ScanId.ToString(),
                frameData,
                scan.CaptureMetadata.AmbientLightLux,
                scan.CaptureMetadata.DeviceModel,
                cancellationToken);

            scan.Complete(result.RednessScore, result.TearFilmMetrics);

            await scanRepository.UpdateAsync(scan, cancellationToken);

            // Publish integration event via transactional outbox
            var envelope = IntegrationEventEnvelope.Create(
                new
                {
                    scan.ScanId,
                    scan.UserId,
                    RednessOverall = result.RednessScore.Overall,
                    RednessConfidence = result.RednessScore.Confidence,
                    TearFilmBreakUpTime = result.TearFilmMetrics.BreakUpTime,
                    TearFilmCoverage = result.TearFilmMetrics.CoveragePercentage
                },
                scan.TenantId,
                subjectId: scan.ScanId.Value,
                correlationId: Guid.NewGuid(),
                causationId: scan.Id);

            await outboxStore.SaveAsync(envelope, cancellationToken);
        }
        catch (Exception ex)
        {
            scan.Fail(ex.Message);
            await scanRepository.UpdateAsync(scan, cancellationToken);
            throw;
        }
    }
}
