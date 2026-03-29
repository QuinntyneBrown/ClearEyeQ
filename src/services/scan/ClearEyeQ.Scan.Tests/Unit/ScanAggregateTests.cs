using ClearEyeQ.Scan.Domain.Entities;
using ClearEyeQ.Scan.Domain.Enums;
using ClearEyeQ.Scan.Domain.Events;
using ClearEyeQ.Scan.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using Xunit;
using ScanAggregate = ClearEyeQ.Scan.Domain.Aggregates.Scan;

namespace ClearEyeQ.Scan.Tests.Unit;

public sealed class ScanAggregateTests
{
    private static readonly UserId TestUserId = UserId.New();
    private static readonly TenantId TestTenantId = TenantId.New();

    private static CaptureMetadata CreateMetadata() =>
        new("iPhone 15 Pro", 10, TimeSpan.FromMilliseconds(500), 350.0);

    private static RednessScore CreateRednessScore(double overall = 42.5) =>
        new(overall, 0.92, new Dictionary<string, double>
        {
            { "nasal", 45.0 },
            { "temporal", 38.0 },
            { "superior", 41.0 },
            { "inferior", 46.0 }
        });

    private static TearFilmMetrics CreateTearFilmMetrics() =>
        new(8.5, "Grade 2", 78.3);

    [Fact]
    public void Initiate_ShouldCreateScanWithInitiatedStatus()
    {
        var scan = ScanAggregate.Initiate(TestUserId, TestTenantId, EyeSide.Left, CreateMetadata());

        scan.ScanId.Value.Should().NotBeEmpty();
        scan.UserId.Should().Be(TestUserId);
        scan.TenantId.Should().Be(TestTenantId);
        scan.EyeSide.Should().Be(EyeSide.Left);
        scan.Status.Should().Be(ScanStatus.Initiated);
        scan.CaptureMetadata.DeviceModel.Should().Be("iPhone 15 Pro");
        scan.Images.Should().BeEmpty();
        scan.RednessScore.Should().BeNull();
        scan.TearFilmMetrics.Should().BeNull();
        scan.Comparison.Should().BeNull();
        scan.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void AddImage_ShouldAddImageAndTransitionToCapturing()
    {
        var scan = ScanAggregate.Initiate(TestUserId, TestTenantId, EyeSide.Right, CreateMetadata());
        var image = new ScanImage(0, "https://blob/tenant/user/scan/0.webp", 0.85);

        scan.AddImage(image);

        scan.Status.Should().Be(ScanStatus.Capturing);
        scan.Images.Should().HaveCount(1);
        scan.Images[0].FrameIndex.Should().Be(0);
        scan.Images[0].IsSelected.Should().BeTrue();
    }

    [Fact]
    public void AddImage_ShouldSelectHighestQualityImage()
    {
        var scan = ScanAggregate.Initiate(TestUserId, TestTenantId, EyeSide.Left, CreateMetadata());
        var lowQuality = new ScanImage(0, "https://blob/0.webp", 0.5);
        var highQuality = new ScanImage(1, "https://blob/1.webp", 0.95);
        var midQuality = new ScanImage(2, "https://blob/2.webp", 0.7);

        scan.AddImage(lowQuality);
        scan.AddImage(highQuality);
        scan.AddImage(midQuality);

        scan.Images.Count(i => i.IsSelected).Should().Be(1);
        scan.Images.Single(i => i.IsSelected).FrameIndex.Should().Be(1);
    }

    [Fact]
    public void Complete_ShouldSetResultsAndRaiseScanCompletedEvent()
    {
        var scan = ScanAggregate.Initiate(TestUserId, TestTenantId, EyeSide.Left, CreateMetadata());
        scan.AddImage(new ScanImage(0, "https://blob/0.webp", 0.9));
        scan.MarkProcessing();

        scan.Complete(CreateRednessScore(), CreateTearFilmMetrics());

        scan.Status.Should().Be(ScanStatus.Completed);
        scan.RednessScore.Should().NotBeNull();
        scan.RednessScore!.Overall.Should().Be(42.5);
        scan.TearFilmMetrics.Should().NotBeNull();
        scan.TearFilmMetrics!.BreakUpTime.Should().Be(8.5);

        scan.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ScanCompletedEvent>()
            .Which.ScanId.Should().Be(scan.ScanId);
    }

    [Fact]
    public void Fail_ShouldSetFailureReasonAndRaiseScanFailedEvent()
    {
        var scan = ScanAggregate.Initiate(TestUserId, TestTenantId, EyeSide.Left, CreateMetadata());
        scan.AddImage(new ScanImage(0, "https://blob/0.webp", 0.9));
        scan.MarkProcessing();

        scan.Fail("ML inference timed out");

        scan.Status.Should().Be(ScanStatus.Failed);
        scan.FailureReason.Should().Be("ML inference timed out");
        scan.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ScanFailedEvent>()
            .Which.Reason.Should().Be("ML inference timed out");
    }

    [Fact]
    public void CompareWith_ShouldComputeRednessDelta()
    {
        var scan = ScanAggregate.Initiate(TestUserId, TestTenantId, EyeSide.Left, CreateMetadata());
        scan.AddImage(new ScanImage(0, "https://blob/0.webp", 0.9));
        scan.MarkProcessing();
        scan.Complete(CreateRednessScore(60.0), CreateTearFilmMetrics());

        var baselineScanId = ScanId.New();
        var baselineRedness = CreateRednessScore(45.0);

        scan.CompareWith(baselineScanId, baselineRedness);

        scan.Comparison.Should().NotBeNull();
        scan.Comparison!.BaselineScanId.Should().Be(baselineScanId);
        scan.Comparison.RednessDelta.Should().BeApproximately(15.0, 0.001);
    }

    [Fact]
    public void MarkProcessing_WithNoImages_ShouldThrow()
    {
        var scan = ScanAggregate.Initiate(TestUserId, TestTenantId, EyeSide.Left, CreateMetadata());

        var act = () => scan.MarkProcessing();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*no images*");
    }

    [Fact]
    public void Complete_WhenNotProcessing_ShouldThrow()
    {
        var scan = ScanAggregate.Initiate(TestUserId, TestTenantId, EyeSide.Left, CreateMetadata());

        var act = () => scan.Complete(CreateRednessScore(), CreateTearFilmMetrics());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CompareWith_WhenNotCompleted_ShouldThrow()
    {
        var scan = ScanAggregate.Initiate(TestUserId, TestTenantId, EyeSide.Left, CreateMetadata());

        var act = () => scan.CompareWith(ScanId.New(), CreateRednessScore());

        act.Should().Throw<InvalidOperationException>();
    }
}
