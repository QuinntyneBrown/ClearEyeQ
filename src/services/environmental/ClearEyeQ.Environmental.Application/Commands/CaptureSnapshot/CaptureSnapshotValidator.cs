using FluentValidation;

namespace ClearEyeQ.Environmental.Application.Commands.CaptureSnapshot;

public sealed class CaptureSnapshotValidator : AbstractValidator<CaptureSnapshotCommand>
{
    public CaptureSnapshotValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Latitude).InclusiveBetween(-90.0, 90.0);
        RuleFor(x => x.Longitude).InclusiveBetween(-180.0, 180.0);
    }
}
