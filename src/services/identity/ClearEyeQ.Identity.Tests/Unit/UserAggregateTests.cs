using ClearEyeQ.Identity.Domain.Aggregates;
using ClearEyeQ.Identity.Domain.Enums;
using ClearEyeQ.Identity.Domain.Events;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using FluentAssertions;

namespace ClearEyeQ.Identity.Tests.Unit;

public sealed class UserAggregateTests
{
    private static User CreateTestUser(
        string email = "test@example.com",
        Role role = Role.Patient)
    {
        return User.Register(
            email: email,
            passwordHash: "hashed-password",
            salt: "test-salt",
            displayName: "Test User",
            role: role,
            tenantId: TenantId.New());
    }

    [Fact]
    public void Register_ShouldCreateUser_WithCorrectProperties()
    {
        var tenantId = TenantId.New();

        var user = User.Register(
            email: "john@example.com",
            passwordHash: "hash123",
            salt: "salt123",
            displayName: "John Doe",
            role: Role.Patient,
            tenantId: tenantId);

        user.Email.Should().Be("john@example.com");
        user.PasswordHash.Should().Be("hash123");
        user.Salt.Should().Be("salt123");
        user.DisplayName.Should().Be("John Doe");
        user.Role.Should().Be(Role.Patient);
        user.AccountStatus.Should().Be(AccountStatus.PendingVerification);
        user.TenantId.Should().Be(tenantId);
        user.MfaEnabled.Should().BeFalse();
        user.FailedLoginAttempts.Should().Be(0);
        user.LockedUntil.Should().BeNull();
    }

    [Fact]
    public void Register_ShouldRaise_UserRegisteredEvent()
    {
        var user = CreateTestUser();

        user.DomainEvents.Should().ContainSingle();
        var domainEvent = user.DomainEvents[0].Should().BeOfType<UserRegisteredEvent>().Subject;
        domainEvent.Email.Should().Be("test@example.com");
        domainEvent.Role.Should().Be(Role.Patient);
    }

    [Fact]
    public void Register_ShouldNormalizeEmail_ToLowerCase()
    {
        var user = User.Register(
            email: "Test@Example.COM",
            passwordHash: "hash",
            salt: "salt",
            displayName: "Test",
            role: Role.Patient,
            tenantId: TenantId.New());

        user.Email.Should().Be("test@example.com");
    }

    [Fact]
    public void Authenticate_ShouldSucceed_WithCorrectPassword()
    {
        var user = CreateTestUser();

        var result = user.Authenticate("hashed-password", "device-1");

        result.Should().BeTrue();
        user.FailedLoginAttempts.Should().Be(0);
        user.DomainEvents.Should().HaveCount(2); // Register + Authenticate
        user.DomainEvents[1].Should().BeOfType<UserAuthenticatedEvent>();
    }

    [Fact]
    public void Authenticate_ShouldFail_WithIncorrectPassword()
    {
        var user = CreateTestUser();

        var result = user.Authenticate("wrong-password", "device-1");

        result.Should().BeFalse();
        user.FailedLoginAttempts.Should().Be(1);
    }

    [Fact]
    public void Authenticate_ShouldLockAccount_AfterMaxFailedAttempts()
    {
        var user = CreateTestUser();

        for (var i = 0; i < 5; i++)
        {
            user.Authenticate("wrong-password", "device-1");
        }

        user.FailedLoginAttempts.Should().Be(5);
        user.LockedUntil.Should().NotBeNull();
        user.IsLockedOut().Should().BeTrue();

        // Should fail even with correct password while locked
        var result = user.Authenticate("hashed-password", "device-1");
        result.Should().BeFalse();
    }

    [Fact]
    public void Unlock_ShouldResetLockoutState()
    {
        var user = CreateTestUser();

        for (var i = 0; i < 5; i++)
        {
            user.Authenticate("wrong-password", "device-1");
        }

        user.Unlock();

        user.FailedLoginAttempts.Should().Be(0);
        user.LockedUntil.Should().BeNull();
        user.IsLockedOut().Should().BeFalse();
    }

    [Fact]
    public void AddConsent_ShouldAddNewConsent()
    {
        var user = CreateTestUser();
        user.ClearDomainEvents();

        var consent = user.AddConsent(ConsentType.DataProcessing);

        consent.ConsentType.Should().Be(ConsentType.DataProcessing);
        consent.IsActive.Should().BeTrue();
        user.Consents.Should().ContainSingle();
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ConsentGrantedEvent>();
    }

    [Fact]
    public void AddConsent_ShouldThrow_WhenDuplicateActiveConsent()
    {
        var user = CreateTestUser();
        user.AddConsent(ConsentType.DataProcessing);

        var act = () => user.AddConsent(ConsentType.DataProcessing);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public void RevokeConsent_ShouldDeactivateConsent()
    {
        var user = CreateTestUser();
        user.AddConsent(ConsentType.DataSharing);
        user.ClearDomainEvents();

        user.RevokeConsent(ConsentType.DataSharing);

        user.Consents[0].IsActive.Should().BeFalse();
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ConsentRevokedEvent>();
    }

    [Fact]
    public void RevokeConsent_ShouldThrow_WhenNoActiveConsent()
    {
        var user = CreateTestUser();

        var act = () => user.RevokeConsent(ConsentType.Research);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No active consent*");
    }

    [Fact]
    public void IssueRefreshToken_ShouldAddToken()
    {
        var user = CreateTestUser();

        var token = user.IssueRefreshToken("hash-1", "device-1");

        token.TokenHash.Should().Be("hash-1");
        token.DeviceFingerprint.Should().Be("device-1");
        token.IsRevoked.Should().BeFalse();
        user.RefreshTokens.Should().ContainSingle();
    }

    [Fact]
    public void IssueRefreshToken_ShouldRevokeExistingTokenForSameDevice()
    {
        var user = CreateTestUser();
        user.IssueRefreshToken("hash-1", "device-1");

        user.IssueRefreshToken("hash-2", "device-1");

        user.RefreshTokens.Should().HaveCount(2);
        user.RefreshTokens[0].IsRevoked.Should().BeTrue();
        user.RefreshTokens[1].IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void RevokeRefreshToken_ShouldRevokeSpecificToken()
    {
        var user = CreateTestUser();
        user.IssueRefreshToken("hash-1", "device-1");

        user.RevokeRefreshToken("hash-1");

        user.RefreshTokens[0].IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void RevokeRefreshToken_ShouldThrow_WhenTokenNotFound()
    {
        var user = CreateTestUser();

        var act = () => user.RevokeRefreshToken("nonexistent");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public void EnableMfa_ShouldSetMfaEnabled()
    {
        var user = CreateTestUser();

        user.EnableMfa();

        user.MfaEnabled.Should().BeTrue();
    }
}
