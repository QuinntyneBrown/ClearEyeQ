using ClearEyeQ.Identity.Domain.Enums;

namespace ClearEyeQ.Identity.Application.Queries.GetUserProfile;

public sealed record UserProfileDto(
    Guid UserId,
    string Email,
    string DisplayName,
    Role Role,
    AccountStatus AccountStatus,
    bool MfaEnabled,
    IReadOnlyList<ConsentDto> Consents);

public sealed record ConsentDto(
    ConsentType ConsentType,
    DateTimeOffset GrantedAt,
    bool IsActive);
