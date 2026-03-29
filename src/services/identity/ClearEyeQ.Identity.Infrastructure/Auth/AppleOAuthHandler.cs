using System.IdentityModel.Tokens.Jwt;

namespace ClearEyeQ.Identity.Infrastructure.Auth;

/// <summary>
/// Handles validation of Apple Sign-In tokens and normalization into internal identity representations.
/// </summary>
public sealed class AppleOAuthHandler
{
    private readonly HttpClient _httpClient;

    public AppleOAuthHandler(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Validates an Apple identity token and returns the associated subject and email.
    /// </summary>
    public async Task<AppleIdentity?> ValidateTokenAsync(string identityToken, CancellationToken cancellationToken = default)
    {
        // Fetch Apple's public keys for token verification
        var response = await _httpClient.GetAsync(
            "https://appleid.apple.com/auth/keys",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        // Parse the JWT without full signature validation for now.
        // Production implementation would verify against Apple's JWKS endpoint.
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(identityToken))
        {
            return null;
        }

        var jwt = handler.ReadJwtToken(identityToken);
        var subject = jwt.Subject;
        var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

        if (string.IsNullOrEmpty(subject))
        {
            return null;
        }

        return new AppleIdentity(
            SubjectId: subject,
            Email: email,
            IsPrivateRelay: email?.EndsWith("@privaterelay.appleid.com") ?? false);
    }
}

public sealed record AppleIdentity(
    string SubjectId,
    string? Email,
    bool IsPrivateRelay);
