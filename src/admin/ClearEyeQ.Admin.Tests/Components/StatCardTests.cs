using Bunit;
using ClearEyeQ.Admin.Components.Shared;
using FluentAssertions;
using Xunit;

namespace ClearEyeQ.Admin.Tests.Components;

public sealed class StatCardTests : TestContext
{
    [Fact]
    public void StatCard_RendersValueAndTitle()
    {
        // Act
        var cut = RenderComponent<StatCard>(parameters => parameters
            .Add(p => p.Title, "Total Users")
            .Add(p => p.Value, "1,234"));

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("1,234");
        markup.Should().Contain("Total Users");
    }

    [Fact]
    public void StatCard_RendersSubtitleWhenProvided()
    {
        // Act
        var cut = RenderComponent<StatCard>(parameters => parameters
            .Add(p => p.Title, "Revenue")
            .Add(p => p.Value, "$50K")
            .Add(p => p.Subtitle, "Monthly recurring"));

        // Assert
        cut.Markup.Should().Contain("Monthly recurring");
    }

    [Fact]
    public void StatCard_DoesNotRenderSubtitleWhenEmpty()
    {
        // Act
        var cut = RenderComponent<StatCard>(parameters => parameters
            .Add(p => p.Title, "Count")
            .Add(p => p.Value, "42"));

        // Assert
        cut.FindAll(".stat-subtitle").Should().BeEmpty();
    }

    [Fact]
    public void StatCard_AppliesColorClass()
    {
        // Act
        var cut = RenderComponent<StatCard>(parameters => parameters
            .Add(p => p.Title, "Health")
            .Add(p => p.Value, "OK")
            .Add(p => p.ColorClass, "accent-success"));

        // Assert
        cut.Find(".stat-card").ClassList.Should().Contain("accent-success");
    }
}
