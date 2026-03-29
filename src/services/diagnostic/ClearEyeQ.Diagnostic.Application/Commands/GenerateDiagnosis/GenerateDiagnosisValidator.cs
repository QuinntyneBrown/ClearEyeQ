using FluentValidation;

namespace ClearEyeQ.Diagnostic.Application.Commands.GenerateDiagnosis;

public sealed class GenerateDiagnosisValidator : AbstractValidator<GenerateDiagnosisCommand>
{
    public GenerateDiagnosisValidator()
    {
        RuleFor(x => x.ScanId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
    }
}
