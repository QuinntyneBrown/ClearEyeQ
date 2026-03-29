using ClearEyeQ.Identity.Domain.Aggregates;

namespace ClearEyeQ.Identity.Application.Interfaces;

public interface ITokenProvider
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    bool ValidateRefreshToken(string token);
    string HashToken(string token);
}
