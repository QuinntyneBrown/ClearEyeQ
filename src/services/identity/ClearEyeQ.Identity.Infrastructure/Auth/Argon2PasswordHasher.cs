using System.Security.Cryptography;
using ClearEyeQ.Identity.Application.Interfaces;
using Isopoh.Cryptography.Argon2;

namespace ClearEyeQ.Identity.Infrastructure.Auth;

public sealed class Argon2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;

    public (string Hash, string Salt) HashPassword(string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        var salt = Convert.ToBase64String(saltBytes);
        var hash = Argon2.Hash(password, salt);
        return (hash, salt);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return Argon2.Verify(hash, password);
    }
}
