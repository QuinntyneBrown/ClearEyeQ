using ClearEyeQ.Scan.Domain.Enums;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Scan.Application.Commands.InitiateScan;

public sealed record InitiateScanCommand(
    UserId UserId,
    TenantId TenantId,
    EyeSide EyeSide,
    string DeviceModel) : IRequest<ScanId>;
