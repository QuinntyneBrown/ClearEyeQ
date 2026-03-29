using MediatR;

namespace ClearEyeQ.Identity.Application.Queries.GetUserProfile;

public sealed record GetUserProfileQuery(Guid UserId) : IRequest<UserProfileDto>;
