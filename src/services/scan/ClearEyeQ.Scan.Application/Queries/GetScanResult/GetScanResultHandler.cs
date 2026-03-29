using ClearEyeQ.Scan.Application.Interfaces;
using MediatR;

namespace ClearEyeQ.Scan.Application.Queries.GetScanResult;

public sealed class GetScanResultHandler(IScanRepository scanRepository)
    : IRequestHandler<GetScanResultQuery, ScanResultDto>
{
    public async Task<ScanResultDto> Handle(GetScanResultQuery request, CancellationToken cancellationToken)
    {
        var scan = await scanRepository.GetByIdAsync(request.ScanId, request.TenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Scan {request.ScanId} not found.");

        return new ScanResultDto(
            ScanId: scan.ScanId.Value,
            UserId: scan.UserId.Value,
            EyeSide: scan.EyeSide,
            Status: scan.Status,
            CreatedAt: scan.CreatedAt,
            RednessOverall: scan.RednessScore?.Overall,
            RednessConfidence: scan.RednessScore?.Confidence,
            ZoneScores: scan.RednessScore?.ZoneScores,
            TearFilmBreakUpTime: scan.TearFilmMetrics?.BreakUpTime,
            TearFilmLipidLayerGrade: scan.TearFilmMetrics?.LipidLayerGrade,
            TearFilmCoveragePercentage: scan.TearFilmMetrics?.CoveragePercentage,
            FailureReason: scan.FailureReason);
    }
}
