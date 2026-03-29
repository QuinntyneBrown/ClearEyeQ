using ClearEyeQ.Identity.Domain.Enums;
using MediatR;

namespace ClearEyeQ.Identity.Application.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string DisplayName,
    Role Role) : IRequest<Guid>;
