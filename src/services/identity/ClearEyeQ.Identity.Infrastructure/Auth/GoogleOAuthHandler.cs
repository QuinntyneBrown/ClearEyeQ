namespace ClearEyeQ.Identity.Infrastructure.Auth;

/// <summary>
/// Handles validation of Google OAuth tokens and normalization into internal identity representations.
/// </summary>
public sealed class GoogleOAuthHandler
{
    private readonly HttpClient _httpClient;

    public GoogleOAuthHandler(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Validates a Google ID token and returns the associated email and subject identifier.
    /// </summary>
    public async Task<GoogleIdentity?> ValidateTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            $"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var json = System.Text.Json.JsonDocument.Parse(content);
        var root = json.RootElement;

        if (!root.TryGetProperty("email", out var emailElement) ||
            !root.TryGetProperty("sub", out var subElement))
        {
            return null;
        }

        return new GoogleIdentity(
            SubjectId: subElement.GetString()!,
            Email: emailElement.GetString()!,
            EmailVerified: root.TryGetProperty("email_verified", out var ev) && ev.GetString() == "true",
            DisplayName: root.TryGetProperty("name", out var name) ? name.GetString() : null);
    }
}

public sealed record GoogleIdentity(
    string SubjectId,
    string Email,
    bool EmailVerified,
    string? DisplayName);
