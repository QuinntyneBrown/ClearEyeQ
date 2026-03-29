using ClearEyeQ.Identity.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Identity.Application.Queries.GetUserProfile;

public sealed class GetUserProfileHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly IUserRepository _userRepository;

    private static readonly TenantId DefaultTenantId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    public GetUserProfileHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, DefaultTenantId, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException($"User '{request.UserId}' not found.");
        }

        var consents = user.Consents
            .Select(c => new ConsentDto(c.ConsentType, c.GrantedAt, c.IsActive))
            .ToList();

        return new UserProfileDto(
            UserId: user.Id,
            Email: user.Email,
            DisplayName: user.DisplayName,
            Role: user.Role,
            AccountStatus: user.AccountStatus,
            MfaEnabled: user.MfaEnabled,
            Consents: consents);
    }
}
