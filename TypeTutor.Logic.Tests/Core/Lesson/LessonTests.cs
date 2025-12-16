using FluentAssertions;
using TypeTutor.Logic.Core;
using TypeTutor.Logic.Tests.Helpers;
using Xunit;

namespace TypeTutor.Logic.Tests.Core.Lesson;

/// <summary>
/// Tests für die Lesson-Klasse.
/// Validiert Konstruktion, Validierung, TargetText-Generierung und Metrics.
/// </summary>
public sealed class LessonTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateLesson()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData(title: "Test Lesson");
        var blocks = new[] { "block1", "block2", "block3" };

        // Act
        var lesson = new TypeTutor.Logic.Core.Lesson(meta, blocks);

        // Assert
        lesson.Should().NotBeNull();
        lesson.Meta.Should().Be(meta);
        lesson.Blocks.Should().BeEquivalentTo(blocks);
        lesson.TargetText.Should().Be("block1 block2 block3");
    }

    [Fact]
    public void Constructor_WithNullMetadata_ShouldThrowArgumentNullException()
    {
        // Arrange
        var blocks = new[] { "test" };

        // Act
        Action act = () => new TypeTutor.Logic.Core.Lesson(null!, blocks);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("meta");
    }

    [Fact]
    public void Constructor_WithNullBlocks_ShouldThrowArgumentNullException()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();

        // Act
        Action act = () => new TypeTutor.Logic.Core.Lesson(meta, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("blocks");
    }

    [Fact]
    public void Constructor_WithEmptyTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var meta = new LessonMetaData("");
        var blocks = new[] { "test" };

        // Act
        Action act = () => new TypeTutor.Logic.Core.Lesson(meta, blocks);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Title*");
    }

    [Fact]
    public void Constructor_WithNullBlockElement_ShouldThrowArgumentException()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var blocks = new[] { "valid", null!, "valid" };

        // Act
        Action act = () => new TypeTutor.Logic.Core.Lesson(meta, blocks);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*null*");
    }

    [Fact]
    public void Constructor_WithEmptyBlockElement_ShouldThrowArgumentException()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var blocks = new[] { "valid", "", "valid" };

        // Act
        Action act = () => new TypeTutor.Logic.Core.Lesson(meta, blocks);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*empty*");
    }

    [Fact]
    public void TargetText_ShouldJoinBlocksWithSingleSpace()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var blocks = new[] { "asdf", "jklö", "gh" };

        // Act
        var lesson = new TypeTutor.Logic.Core.Lesson(meta, blocks);

        // Assert
        lesson.TargetText.Should().Be("asdf jklö gh");
    }

    [Fact]
    public void TargetText_WithSingleBlock_ShouldNotAddSpaces()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var blocks = new[] { "singleblock" };

        // Act
        var lesson = new TypeTutor.Logic.Core.Lesson(meta, blocks);

        // Assert
        lesson.TargetText.Should().Be("singleblock");
        lesson.TargetText.Should().NotContain(" ");
    }

    [Fact]
    public void Metrics_ShouldCalculateCorrectBlockCount()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var blocks = new[] { "one", "two", "three" };

        // Act
        var lesson = new TypeTutor.Logic.Core.Lesson(meta, blocks);

        // Assert
        lesson.Metrics.BlockCount.Should().Be(3);
    }

    [Fact]
    public void Metrics_ShouldCalculateCorrectCharacterCount()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var blocks = new[] { "abc", "def" };

        // Act
        var lesson = new TypeTutor.Logic.Core.Lesson(meta, blocks);

        // Assert
        // "abc def" = 7 Zeichen (6 Buchstaben + 1 Leerzeichen)
        lesson.Metrics.CharacterCount.Should().Be(7);
    }

    [Fact]
    public void Metrics_WithEmptyBlocks_ShouldCalculateZeroBlocks()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var blocks = Array.Empty<string>();

        // Act
        var lesson = new TypeTutor.Logic.Core.Lesson(meta, blocks);

        // Assert
        lesson.Metrics.BlockCount.Should().Be(0);
        lesson.Metrics.CharacterCount.Should().Be(0);
        lesson.Metrics.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Blocks_ShouldBeImmutable()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var originalBlocks = new[] { "block1", "block2" };
        var lesson = new TypeTutor.Logic.Core.Lesson(meta, originalBlocks);

        // Act - Versuche die ursprüngliche Liste zu ändern
        originalBlocks[0] = "modified";

        // Assert - Lesson sollte unverändert sein
        lesson.Blocks[0].Should().Be("block1");
    }

    [Fact]
    public void ToString_ShouldReturnInformativeString()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData(title: "My Test");
        var blocks = new[] { "a", "b", "c" };
        var lesson = new TypeTutor.Logic.Core.Lesson(meta, blocks);

        // Act
        var result = lesson.ToString();

        // Assert
        result.Should().Contain("My Test");
        result.Should().Contain("Blocks=3");
    }

    [Theory]
    [InlineData(new[] { "a" }, "a")]
    [InlineData(new[] { "a", "b" }, "a b")]
    [InlineData(new[] { "asdf", "jklö" }, "asdf jklö")]
    [InlineData(new[] { "one", "two", "three" }, "one two three")]
    public void TargetText_WithVariousBlocks_ShouldJoinCorrectly(
        string[] blocks,
        string expectedTarget)
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();

        // Act
        var lesson = new TypeTutor.Logic.Core.Lesson(meta, blocks);

        // Assert
        lesson.TargetText.Should().Be(expectedTarget);
    }

    [Fact]
    public void Constructor_WithComplexMetadata_ShouldPreserveAllProperties()
    {
        // Arrange
        var meta = new LessonMetaData("Complex Lesson")
        {
            Description = "A complex description",
            Difficulty = 3,
            Tags = new[] { "advanced", "german" },
            ModuleId = "M01"
        };
        var blocks = new[] { "test" };

        // Act
        var lesson = new TypeTutor.Logic.Core.Lesson(meta, blocks);

        // Assert
        lesson.Meta.Title.Should().Be("Complex Lesson");
        lesson.Meta.Description.Should().Be("A complex description");
        lesson.Meta.Difficulty.Should().Be(3);
        lesson.Meta.Tags.Should().Contain(new[] { "advanced", "german" });
        lesson.Meta.ModuleId.Should().Be("M01");
    }

    [Fact]
    public void Metrics_IsEmpty_ShouldBeTrueForEmptyBlocks()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var blocks = Array.Empty<string>();

        // Act
        var lesson = new TypeTutor.Logic.Core.Lesson(meta, blocks);

        // Assert
        lesson.Metrics.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Metrics_IsEmpty_ShouldBeFalseForNonEmptyBlocks()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var blocks = new[] { "test" };

        // Act
        var lesson = new TypeTutor.Logic.Core.Lesson(meta, blocks);

        // Assert
        lesson.Metrics.IsEmpty.Should().BeFalse();
    }
}
