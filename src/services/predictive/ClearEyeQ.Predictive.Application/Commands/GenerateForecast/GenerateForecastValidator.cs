using FluentValidation;

namespace ClearEyeQ.Predictive.Application.Commands.GenerateForecast;

public sealed class GenerateForecastValidator : AbstractValidator<GenerateForecastCommand>
{
    public GenerateForecastValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ForecastDays).InclusiveBetween(1, 7);
    }
}
