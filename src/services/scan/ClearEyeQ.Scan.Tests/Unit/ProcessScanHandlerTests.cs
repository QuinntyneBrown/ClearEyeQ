using ClearEyeQ.Scan.Application.Commands.ProcessScan;
using ClearEyeQ.Scan.Application.Interfaces;
using ClearEyeQ.Scan.Domain.Entities;
using ClearEyeQ.Scan.Domain.Enums;
using ClearEyeQ.Scan.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.Events;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using ScanAggregate = ClearEyeQ.Scan.Domain.Aggregates.Scan;

namespace ClearEyeQ.Scan.Tests.Unit;

public sealed class ProcessScanHandlerTests
{
    private readonly IScanRepository _scanRepository = Substitute.For<IScanRepository>();
    private readonly IMLInferenceClient _mlClient = Substitute.For<IMLInferenceClient>();
    private readonly IImageStore _imageStore = Substitute.For<IImageStore>();
    private readonly IOutboxStore _outboxStore = Substitute.For<IOutboxStore>();
    private readonly ProcessScanHandler _handler;

    public ProcessScanHandlerTests()
    {
        _handler = new ProcessScanHandler(_scanRepository, _mlClient, _imageStore, _outboxStore);
    }

    [Fact]
    public async Task Handle_HappyPath_ShouldCompleteScanAndPublishOutboxEvent()
    {
        // Arrange
        var userId = UserId.New();
        var tenantId = TenantId.New();
        var metadata = new CaptureMetadata("iPhone 15 Pro", 10, TimeSpan.FromMilliseconds(500), 350.0);
        var scan = ScanAggregate.Initiate(userId, tenantId, EyeSide.Left, metadata);
        scan.AddImage(new ScanImage(0, "https://blob/0.webp", 0.9));
        scan.AddImage(new ScanImage(1, "https://blob/1.webp", 0.85));

        var command = new ProcessScanCommand(scan.ScanId, tenantId);

        _scanRepository.GetByIdAsync(scan.ScanId, tenantId, Arg.Any<CancellationToken>())
            .Returns(scan);

        _imageStore.GetUrlAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("https://blob/signed-url");

        var rednessScore = new RednessScore(42.5, 0.92, new Dictionary<string, double>
        {
            { "nasal", 45.0 },
            { "temporal", 38.0 }
        });
        var tearFilmMetrics = new TearFilmMetrics(8.5, "Grade 2", 78.3);
        var mlResult = new MLInferenceResult(rednessScore, tearFilmMetrics);

        _mlClient.ProcessScanAsync(
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<byte[]>>(),
                Arg.Any<double>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(mlResult);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        scan.Status.Should().Be(ScanStatus.Completed);
        scan.RednessScore.Should().NotBeNull();
        scan.RednessScore!.Overall.Should().Be(42.5);
        scan.TearFilmMetrics.Should().NotBeNull();
        scan.TearFilmMetrics!.BreakUpTime.Should().Be(8.5);

        await _scanRepository.Received(1).UpdateAsync(scan, Arg.Any<CancellationToken>());
        await _outboxStore.Received(1).SaveAsync(Arg.Any<IntegrationEventEnvelope>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenScanNotFound_ShouldThrow()
    {
        var command = new ProcessScanCommand(ScanId.New(), TenantId.New());

        _scanRepository.GetByIdAsync(Arg.Any<ScanId>(), Arg.Any<TenantId>(), Arg.Any<CancellationToken>())
            .Returns((ScanAggregate?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_WhenMLClientFails_ShouldFailScanAndRethrow()
    {
        // Arrange
        var userId = UserId.New();
        var tenantId = TenantId.New();
        var metadata = new CaptureMetadata("iPhone 15 Pro", 10, TimeSpan.FromMilliseconds(500), 350.0);
        var scan = ScanAggregate.Initiate(userId, tenantId, EyeSide.Left, metadata);
        scan.AddImage(new ScanImage(0, "https://blob/0.webp", 0.9));

        var command = new ProcessScanCommand(scan.ScanId, tenantId);

        _scanRepository.GetByIdAsync(scan.ScanId, tenantId, Arg.Any<CancellationToken>())
            .Returns(scan);

        _imageStore.GetUrlAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("https://blob/signed-url");

        _mlClient.ProcessScanAsync(
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<byte[]>>(),
                Arg.Any<double>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("ML service unavailable"));

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("ML service unavailable");
        scan.Status.Should().Be(ScanStatus.Failed);
        scan.FailureReason.Should().Be("ML service unavailable");
        await _scanRepository.Received(1).UpdateAsync(scan, Arg.Any<CancellationToken>());
    }
}
