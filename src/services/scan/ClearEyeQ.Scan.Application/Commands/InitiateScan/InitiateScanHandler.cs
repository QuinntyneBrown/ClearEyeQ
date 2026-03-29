using ClearEyeQ.Scan.Application.Interfaces;
using ClearEyeQ.Scan.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;
using ScanAggregate = ClearEyeQ.Scan.Domain.Aggregates.Scan;

namespace ClearEyeQ.Scan.Application.Commands.InitiateScan;

public sealed class InitiateScanHandler(IScanRepository scanRepository)
    : IRequestHandler<InitiateScanCommand, ScanId>
{
    public async Task<ScanId> Handle(InitiateScanCommand request, CancellationToken cancellationToken)
    {
        var captureMetadata = new CaptureMetadata(
            deviceModel: request.DeviceModel,
            frameCount: 0,
            burstDuration: TimeSpan.Zero,
            ambientLightLux: 0);

        var scan = ScanAggregate.Initiate(
            request.UserId,
            request.TenantId,
            request.EyeSide,
            captureMetadata);

        await scanRepository.SaveAsync(scan, cancellationToken);

        return scan.ScanId;
    }
}
