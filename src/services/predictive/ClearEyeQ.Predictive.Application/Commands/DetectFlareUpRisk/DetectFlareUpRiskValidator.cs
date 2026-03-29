using FluentValidation;

namespace ClearEyeQ.Predictive.Application.Commands.DetectFlareUpRisk;

public sealed class DetectFlareUpRiskValidator : AbstractValidator<DetectFlareUpRiskCommand>
{
    public DetectFlareUpRiskValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
    }
}
