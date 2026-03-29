using FluentValidation;

namespace ClearEyeQ.Scan.Application.Commands.ProcessScan;

public sealed class ProcessScanValidator : AbstractValidator<ProcessScanCommand>
{
    public ProcessScanValidator()
    {
        RuleFor(x => x.ScanId.Value)
            .NotEmpty()
            .WithMessage("Scan ID is required.");

        RuleFor(x => x.TenantId.Value)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");
    }
}
