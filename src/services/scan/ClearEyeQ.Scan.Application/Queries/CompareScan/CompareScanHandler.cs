using ClearEyeQ.Scan.Application.Interfaces;
using MediatR;

namespace ClearEyeQ.Scan.Application.Queries.CompareScan;

public sealed class CompareScanHandler(IScanRepository scanRepository)
    : IRequestHandler<CompareScanQuery, ScanComparisonDto>
{
    public async Task<ScanComparisonDto> Handle(CompareScanQuery request, CancellationToken cancellationToken)
    {
        var scan = await scanRepository.GetByIdAsync(request.ScanId, request.TenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Scan {request.ScanId} not found.");

        var baseline = await scanRepository.GetByIdAsync(request.BaselineId, request.TenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Baseline scan {request.BaselineId} not found.");

        if (scan.RednessScore is null)
            throw new InvalidOperationException("Current scan does not have redness results.");

        if (baseline.RednessScore is null)
            throw new InvalidOperationException("Baseline scan does not have redness results.");

        scan.CompareWith(request.BaselineId, baseline.RednessScore);

        return new ScanComparisonDto(
            ScanId: scan.ScanId.Value,
            BaselineScanId: baseline.ScanId.Value,
            RednessDelta: scan.Comparison!.RednessDelta,
            TearFilmDelta: scan.Comparison.TearFilmDelta,
            CurrentRednessOverall: scan.RednessScore.Overall,
            BaselineRednessOverall: baseline.RednessScore.Overall);
    }
}
