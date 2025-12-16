using FluentAssertions;
using TypeTutor.Logic.Core;
using Xunit;

namespace TypeTutor.Logic.Tests.Core.Lesson;

/// <summary>
/// Tests für LessonMetrics.
/// Validiert Metrik-Berechnungen aus Blöcken.
/// </summary>
public sealed class LessonMetricsTests
{
    [Fact]
    public void FromBlocks_WithEmptyList_ShouldReturnEmptyMetrics()
    {
        // Arrange
        var blocks = Array.Empty<string>();

        // Act
        var metrics = LessonMetrics.FromBlocks(blocks);

        // Assert
        metrics.BlockCount.Should().Be(0);
        metrics.CharacterCount.Should().Be(0);
        metrics.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void FromBlocks_WithSingleBlock_ShouldCalculateCorrectly()
    {
        // Arrange
        var blocks = new[] { "test" };

        // Act
        var metrics = LessonMetrics.FromBlocks(blocks);

        // Assert
        metrics.BlockCount.Should().Be(1);
        metrics.CharacterCount.Should().Be(4);
        metrics.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void FromBlocks_WithMultipleBlocks_ShouldIncludeSpaces()
    {
        // Arrange
        var blocks = new[] { "abc", "def" };

        // Act
        var metrics = LessonMetrics.FromBlocks(blocks);

        // Assert
        metrics.BlockCount.Should().Be(2);
        // "abc def" = 3 + 1 (space) + 3 = 7
        metrics.CharacterCount.Should().Be(7);
        metrics.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void FromBlocks_WithThreeBlocks_ShouldCalculateTwoSpaces()
    {
        // Arrange
        var blocks = new[] { "a", "b", "c" };

        // Act
        var metrics = LessonMetrics.FromBlocks(blocks);

        // Assert
        metrics.BlockCount.Should().Be(3);
        // "a b c" = 1 + 1 (space) + 1 + 1 (space) + 1 = 5
        metrics.CharacterCount.Should().Be(5);
    }

    [Fact]
    public void FromBlocks_WithNullList_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => LessonMetrics.FromBlocks(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("blocks");
    }

    [Theory]
    [InlineData(new[] { "test" }, 1, 4, false)]
    [InlineData(new[] { "ab", "cd" }, 2, 5, false)]
    [InlineData(new[] { "a", "b", "c", "d" }, 4, 7, false)]
    public void FromBlocks_WithVariousInputs_ShouldCalculateCorrectly(
        string[] blocks,
        int expectedBlockCount,
        int expectedCharCount,
        bool expectedIsEmpty)
    {
        // Act
        var metrics = LessonMetrics.FromBlocks(blocks);

        // Assert
        metrics.BlockCount.Should().Be(expectedBlockCount);
        metrics.CharacterCount.Should().Be(expectedCharCount);
        metrics.IsEmpty.Should().Be(expectedIsEmpty);
    }

    [Fact]
    public void IsEmpty_WithZeroCharacters_ShouldBeTrue()
    {
        // Arrange
        var metrics = new LessonMetrics(0, 0, true);

        // Assert
        metrics.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WithNonZeroCharacters_ShouldBeFalse()
    {
        // Arrange
        var metrics = new LessonMetrics(1, 5, false);

        // Assert
        metrics.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var metrics1 = new LessonMetrics(2, 10, false);
        var metrics2 = new LessonMetrics(2, 10, false);

        // Act & Assert
        metrics1.Should().Be(metrics2);
        (metrics1 == metrics2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var metrics1 = new LessonMetrics(2, 10, false);
        var metrics2 = new LessonMetrics(3, 10, false);

        // Act & Assert
        metrics1.Should().NotBe(metrics2);
        (metrics1 == metrics2).Should().BeFalse();
    }

    [Fact]
    public void FromBlocks_WithLongBlocks_ShouldCalculateCorrectly()
    {
        // Arrange
        var blocks = new[]
        {
            "The quick brown fox",
            "jumps over",
            "the lazy dog"
        };

        // Act
        var metrics = LessonMetrics.FromBlocks(blocks);

        // Assert
        metrics.BlockCount.Should().Be(3);
        // 19 + 10 + 12 + 2 (spaces) = 43
        var expectedLength = 19 + 10 + 12 + 2;
        metrics.CharacterCount.Should().Be(expectedLength);
    }

    [Fact]
    public void FromBlocks_WithUnicodeCharacters_ShouldCountCorrectly()
    {
        // Arrange
        var blocks = new[] { "äöü", "ß?" };

        // Act
        var metrics = LessonMetrics.FromBlocks(blocks);

        // Assert
        metrics.BlockCount.Should().Be(2);
        // "äöü ß?" = 3 + 1 (space) + 2 = 6
        metrics.CharacterCount.Should().Be(6);
    }

    [Fact]
    public void ValueObject_ShouldSupportDeconstruction()
    {
        // Arrange
        var metrics = new LessonMetrics(3, 15, false);

        // Act
        var (blockCount, charCount, isEmpty) = metrics;

        // Assert
        blockCount.Should().Be(3);
        charCount.Should().Be(15);
        isEmpty.Should().BeFalse();
    }

    [Fact]
    public void ToString_ShouldReturnReadableRepresentation()
    {
        // Arrange
        var metrics = new LessonMetrics(3, 15, false);

        // Act
        var result = metrics.ToString();

        // Assert
        result.Should().Contain("3");
        result.Should().Contain("15");
    }
}
