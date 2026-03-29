using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace ClearEyeQ.SharedKernel.Tests.Domain;

public sealed class ConfidenceScoreTests
{
    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(-100)]
    [InlineData(2.0)]
    public void Constructor_WithValueOutOfRange_ThrowsArgumentOutOfRangeException(double value)
    {
        var act = () => new ConfidenceScore(value);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("value");
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void Constructor_WithValidValue_SetsValue(double value)
    {
        var score = new ConfidenceScore(value);

        score.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(0.0, "Low")]
    [InlineData(0.10, "Low")]
    [InlineData(0.24, "Low")]
    [InlineData(0.25, "Medium")]
    [InlineData(0.49, "Medium")]
    [InlineData(0.50, "High")]
    [InlineData(0.74, "High")]
    [InlineData(0.75, "VeryHigh")]
    [InlineData(1.0, "VeryHigh")]
    public void Label_ReturnsCorrectLabel(double value, string expectedLabel)
    {
        var score = new ConfidenceScore(value);

        score.Label.Should().Be(expectedLabel);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var score = new ConfidenceScore(0.85);

        score.ToString().Should().Be("0.85 (VeryHigh)");
    }

    [Fact]
    public void Equality_TwoScoresWithSameValue_AreEqual()
    {
        var score1 = new ConfidenceScore(0.5);
        var score2 = new ConfidenceScore(0.5);

        score1.Should().Be(score2);
    }

    [Fact]
    public void Equality_TwoScoresWithDifferentValues_AreNotEqual()
    {
        var score1 = new ConfidenceScore(0.5);
        var score2 = new ConfidenceScore(0.6);

        score1.Should().NotBe(score2);
    }
}
