using ClearEyeQ.Notifications.Application.Commands.SendNotification;
using ClearEyeQ.Notifications.Application.Interfaces;
using ClearEyeQ.Notifications.Domain.Aggregates;
using ClearEyeQ.Notifications.Domain.Entities;
using ClearEyeQ.Notifications.Domain.Enums;
using ClearEyeQ.Notifications.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace ClearEyeQ.Notifications.Tests.Unit;

public sealed class SendNotificationHandlerTests
{
    private readonly INotificationRepository _notificationRepository = Substitute.For<INotificationRepository>();
    private readonly IPreferenceRepository _preferenceRepository = Substitute.For<IPreferenceRepository>();
    private readonly IChannelSender _pushSender = Substitute.For<IChannelSender>();
    private readonly IChannelSender _inAppSender = Substitute.For<IChannelSender>();
    private readonly ILogger<SendNotificationHandler> _logger = Substitute.For<ILogger<SendNotificationHandler>>();
    private readonly SendNotificationHandler _handler;

    public SendNotificationHandlerTests()
    {
        _pushSender.Channel.Returns(NotificationChannel.Push);
        _inAppSender.Channel.Returns(NotificationChannel.InApp);

        _handler = new SendNotificationHandler(
            _notificationRepository,
            _preferenceRepository,
            new[] { _pushSender, _inAppSender },
            _logger);
    }

    [Fact]
    public async Task Handle_ShouldCreateAndSendNotification()
    {
        var userId = UserId.New();
        var tenantId = TenantId.New();

        _preferenceRepository.GetAsync(userId, tenantId, NotificationChannel.Push, Arg.Any<CancellationToken>())
            .Returns((NotificationPreference?)null);

        _pushSender.SendAsync(userId, Arg.Any<NotificationContent>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new SendNotificationCommand(
            UserId: userId.Value,
            TenantId: tenantId.Value,
            Category: NotificationCategory.ScanResult,
            Title: "Scan Complete",
            Body: "Your eye scan results are ready.",
            ActionUrl: "/scans/latest",
            PreferredChannel: NotificationChannel.Push);

        var notificationId = await _handler.Handle(command, CancellationToken.None);

        notificationId.Should().NotBeEmpty();
        await _notificationRepository.Received(1).AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
        await _notificationRepository.Received(1).UpdateAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
        await _pushSender.Received(1).SendAsync(userId, Arg.Any<NotificationContent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenChannelDisabled_ShouldFallBackToInApp()
    {
        var userId = UserId.New();
        var tenantId = TenantId.New();

        var disabledPreference = NotificationPreference.Create(
            userId, tenantId, NotificationChannel.Push, enabled: false);
        _preferenceRepository.GetAsync(userId, tenantId, NotificationChannel.Push, Arg.Any<CancellationToken>())
            .Returns(disabledPreference);

        _preferenceRepository.GetAsync(userId, tenantId, NotificationChannel.InApp, Arg.Any<CancellationToken>())
            .Returns((NotificationPreference?)null);

        _inAppSender.SendAsync(userId, Arg.Any<NotificationContent>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new SendNotificationCommand(
            UserId: userId.Value,
            TenantId: tenantId.Value,
            Category: NotificationCategory.TreatmentReminder,
            Title: "Reminder",
            Body: "Time for your eye drops.",
            ActionUrl: null,
            PreferredChannel: NotificationChannel.Push);

        await _handler.Handle(command, CancellationToken.None);

        await _inAppSender.Received(1).SendAsync(userId, Arg.Any<NotificationContent>(), Arg.Any<CancellationToken>());
        await _pushSender.DidNotReceive().SendAsync(Arg.Any<UserId>(), Arg.Any<NotificationContent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenQuietHoursActive_ShouldFallBackToInApp()
    {
        var userId = UserId.New();
        var tenantId = TenantId.New();

        var preference = NotificationPreference.Create(
            userId, tenantId, NotificationChannel.Push, enabled: true,
            quietHoursPolicy: new QuietHoursPolicy(
                new TimeOnly(0, 0),
                new TimeOnly(23, 59),
                "UTC"));

        _preferenceRepository.GetAsync(userId, tenantId, NotificationChannel.Push, Arg.Any<CancellationToken>())
            .Returns(preference);

        _preferenceRepository.GetAsync(userId, tenantId, NotificationChannel.InApp, Arg.Any<CancellationToken>())
            .Returns((NotificationPreference?)null);

        _inAppSender.SendAsync(userId, Arg.Any<NotificationContent>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new SendNotificationCommand(
            UserId: userId.Value,
            TenantId: tenantId.Value,
            Category: NotificationCategory.FlareUpAlert,
            Title: "Alert",
            Body: "Flare-up detected",
            ActionUrl: null,
            PreferredChannel: NotificationChannel.Push);

        await _handler.Handle(command, CancellationToken.None);

        await _inAppSender.Received(1).SendAsync(userId, Arg.Any<NotificationContent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSenderFails_ShouldMarkAttemptAsFailed()
    {
        var userId = UserId.New();
        var tenantId = TenantId.New();

        _preferenceRepository.GetAsync(userId, tenantId, NotificationChannel.InApp, Arg.Any<CancellationToken>())
            .Returns((NotificationPreference?)null);

        _inAppSender.SendAsync(userId, Arg.Any<NotificationContent>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new SendNotificationCommand(
            UserId: userId.Value,
            TenantId: tenantId.Value,
            Category: NotificationCategory.SystemAlert,
            Title: "System Alert",
            Body: "Test notification",
            ActionUrl: null,
            PreferredChannel: NotificationChannel.InApp);

        await _handler.Handle(command, CancellationToken.None);

        await _notificationRepository.Received(1).UpdateAsync(
            Arg.Is<Notification>(n => n.DeliveryAttempts.Count == 1),
            Arg.Any<CancellationToken>());
    }
}
