using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Scan.Application.Queries.GetScanResult;

public sealed record GetScanResultQuery(
    ScanId ScanId,
    TenantId TenantId) : IRequest<ScanResultDto>;
