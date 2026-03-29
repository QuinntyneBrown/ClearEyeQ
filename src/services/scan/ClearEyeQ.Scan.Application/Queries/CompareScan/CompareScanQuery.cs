using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Scan.Application.Queries.CompareScan;

public sealed record CompareScanQuery(
    ScanId ScanId,
    ScanId BaselineId,
    TenantId TenantId) : IRequest<ScanComparisonDto>;
