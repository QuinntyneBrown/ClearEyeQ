using ClearEyeQ.Identity.Application.Commands.RegisterUser;
using ClearEyeQ.Identity.Application.Interfaces;
using ClearEyeQ.Identity.Domain.Enums;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace ClearEyeQ.Identity.Tests.Unit;

public sealed class RegisterUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditLogger _auditLogger;
    private readonly RegisterUserHandler _handler;

    public RegisterUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _auditLogger = Substitute.For<IAuditLogger>();
        _handler = new RegisterUserHandler(_userRepository, _passwordHasher, _auditLogger);
    }

    [Fact]
    public async Task Handle_ShouldRegisterUser_AndReturnUserId()
    {
        var command = new RegisterUserCommand(
            Email: "new@example.com",
            Password: "Str0ng!Pass",
            DisplayName: "New User",
            Role: Role.Patient);

        _userRepository
            .ExistsAsync(Arg.Any<string>(), Arg.Any<TenantId>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _passwordHasher
            .HashPassword(Arg.Any<string>())
            .Returns(("hashed-password", "generated-salt"));

        var userId = await _handler.Handle(command, CancellationToken.None);

        userId.Should().NotBeEmpty();

        await _userRepository.Received(1).SaveAsync(
            Arg.Is<Domain.Aggregates.User>(u =>
                u.Email == "new@example.com" &&
                u.DisplayName == "New User" &&
                u.Role == Role.Patient),
            Arg.Any<CancellationToken>());

        await _auditLogger.Received(1).LogAsync(
            "UserRegistered",
            Arg.Any<UserId>(),
            Arg.Is<string>(s => s.Contains("new@example.com")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenEmailAlreadyExists()
    {
        var command = new RegisterUserCommand(
            Email: "existing@example.com",
            Password: "Str0ng!Pass",
            DisplayName: "Existing User",
            Role: Role.Patient);

        _userRepository
            .ExistsAsync(Arg.Any<string>(), Arg.Any<TenantId>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");

        await _userRepository.DidNotReceive().SaveAsync(
            Arg.Any<Domain.Aggregates.User>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldHashPassword_BeforeCreatingUser()
    {
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            Password: "MyP@ssw0rd",
            DisplayName: "Test",
            Role: Role.Clinician);

        _userRepository
            .ExistsAsync(Arg.Any<string>(), Arg.Any<TenantId>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _passwordHasher
            .HashPassword("MyP@ssw0rd")
            .Returns(("argon2-hash", "random-salt"));

        await _handler.Handle(command, CancellationToken.None);

        _passwordHasher.Received(1).HashPassword("MyP@ssw0rd");

        await _userRepository.Received(1).SaveAsync(
            Arg.Is<Domain.Aggregates.User>(u =>
                u.PasswordHash == "argon2-hash" &&
                u.Salt == "random-salt"),
            Arg.Any<CancellationToken>());
    }
}
