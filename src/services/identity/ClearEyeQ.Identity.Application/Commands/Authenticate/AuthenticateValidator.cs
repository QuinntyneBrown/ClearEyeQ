using FluentValidation;

namespace ClearEyeQ.Identity.Application.Commands.Authenticate;

public sealed class AuthenticateValidator : AbstractValidator<AuthenticateCommand>
{
    public AuthenticateValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");

        RuleFor(x => x.DeviceFingerprint)
            .NotEmpty().WithMessage("Device fingerprint is required.");
    }
}
