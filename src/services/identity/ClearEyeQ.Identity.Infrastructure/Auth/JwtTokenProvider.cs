using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ClearEyeQ.Identity.Application.Interfaces;
using ClearEyeQ.Identity.Domain.Aggregates;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ClearEyeQ.Identity.Infrastructure.Auth;

public sealed class JwtTokenProvider : ITokenProvider
{
    private readonly IConfiguration _configuration;

    public JwtTokenProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(User user)
    {
        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured.");
        var issuer = _configuration["Jwt:Issuer"] ?? "ClearEyeQ";
        var audience = _configuration["Jwt:Audience"] ?? "ClearEyeQ";
        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "15");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("tenantId", user.TenantId.Value.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    public bool ValidateRefreshToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            var bytes = Convert.FromBase64String(token);
            return bytes.Length == 64;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
