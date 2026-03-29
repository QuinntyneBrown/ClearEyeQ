using ClearEyeQ.Identity.Application.Commands.Authenticate;
using ClearEyeQ.Identity.Application.Interfaces;
using ClearEyeQ.Identity.Domain.Aggregates;
using ClearEyeQ.Identity.Domain.Enums;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace ClearEyeQ.Identity.Tests.Unit;

public sealed class AuthenticateHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenProvider _tokenProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly AuthenticateHandler _handler;

    public AuthenticateHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _tokenProvider = Substitute.For<ITokenProvider>();
        _auditLogger = Substitute.For<IAuditLogger>();
        _handler = new AuthenticateHandler(_userRepository, _passwordHasher, _tokenProvider, _auditLogger);
    }

    private static User CreateTestUser(string passwordHash = "hashed-pw")
    {
        return User.Register(
            email: "user@example.com",
            passwordHash: passwordHash,
            salt: "salt",
            displayName: "Test User",
            role: Role.Patient,
            tenantId: new TenantId(Guid.Parse("00000000-0000-0000-0000-000000000001")));
    }

    [Fact]
    public async Task Handle_ShouldReturnAuthResult_WhenCredentialsAreValid()
    {
        var user = CreateTestUser();
        var command = new AuthenticateCommand("user@example.com", "correct-password", "device-1");

        _userRepository
            .GetByEmailAsync("user@example.com", Arg.Any<TenantId>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasher
            .VerifyPassword("correct-password", user.PasswordHash)
            .Returns(true);

        _tokenProvider.GenerateAccessToken(user).Returns("jwt-access-token");
        _tokenProvider.GenerateRefreshToken().Returns("raw-refresh-token");
        _tokenProvider.HashToken("raw-refresh-token").Returns("hashed-refresh-token");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.AccessToken.Should().Be("jwt-access-token");
        result.RefreshToken.Should().Be("raw-refresh-token");
        result.ExpiresAt.Should().BeCloseTo(DateTimeOffset.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(5));

        await _userRepository.Received(1).SaveAsync(user, Arg.Any<CancellationToken>());
        await _auditLogger.Received(1).LogAsync(
            "UserAuthenticated",
            Arg.Any<UserId>(),
            Arg.Is<string>(s => s.Contains("device-1")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserNotFound()
    {
        var command = new AuthenticateCommand("nonexistent@example.com", "password", "device-1");

        _userRepository
            .GetByEmailAsync(Arg.Any<string>(), Arg.Any<TenantId>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenPasswordIsWrong()
    {
        var user = CreateTestUser();
        var command = new AuthenticateCommand("user@example.com", "wrong-password", "device-1");

        _userRepository
            .GetByEmailAsync("user@example.com", Arg.Any<TenantId>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasher
            .VerifyPassword("wrong-password", user.PasswordHash)
            .Returns(false);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();

        await _userRepository.Received(1).SaveAsync(user, Arg.Any<CancellationToken>());
        await _auditLogger.Received(1).LogAsync(
            "AuthenticationFailed",
            Arg.Any<UserId>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenAccountIsLocked()
    {
        var user = CreateTestUser();

        for (var i = 0; i < 5; i++)
        {
            user.Authenticate("wrong", "device");
        }

        var command = new AuthenticateCommand("user@example.com", "correct-password", "device-1");

        _userRepository
            .GetByEmailAsync("user@example.com", Arg.Any<TenantId>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasher
            .VerifyPassword("correct-password", user.PasswordHash)
            .Returns(true);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
