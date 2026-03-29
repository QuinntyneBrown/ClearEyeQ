using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Scan.Application.Commands.ProcessScan;

public sealed record ProcessScanCommand(
    ScanId ScanId,
    TenantId TenantId) : IRequest;
