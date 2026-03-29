using FluentValidation;

namespace ClearEyeQ.Monitoring.Application.Commands.IngestWearableData;

public sealed class IngestWearableDataValidator : AbstractValidator<IngestWearableDataCommand>
{
    public IngestWearableDataValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Value).Must(v => !double.IsNaN(v) && !double.IsInfinity(v))
            .WithMessage("Value must be a finite number.");
        RuleFor(x => x.Timestamp).NotEmpty()
            .LessThanOrEqualTo(DateTimeOffset.UtcNow.AddMinutes(5))
            .WithMessage("Timestamp cannot be in the future.");
        RuleFor(x => x.Source).IsInEnum();
        RuleFor(x => x.MetricType).IsInEnum();
    }
}
