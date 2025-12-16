using FluentAssertions;
using TypeTutor.Logic.Core;
using TypeTutor.Logic.Engine;
using TypeTutor.Logic.Tests.Helpers;
using Xunit;

namespace TypeTutor.Logic.Tests.Core.Lesson;

/// <summary>
/// Tests für die LessonFactory.
/// Validiert Lesson-Erstellung, Text-Normalisierung und Word-Wrap-Logik.
/// </summary>
public sealed class LessonFactoryTests
{
    private readonly LessonFactory _factory = new();

    [Fact]
    public void Create_WithValidBlocksAndMetadata_ShouldCreateLesson()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData(title: "Test");
        var blocks = new[] { "block1", "block2" };

        // Act
        var lesson = _factory.Create(meta, blocks);

        // Assert
        lesson.Should().NotBeNull();
        lesson.Meta.Should().Be(meta);
        lesson.Blocks.Should().BeEquivalentTo(blocks);
    }

    [Fact]
    public void Create_WithNullMetadata_ShouldThrowArgumentNullException()
    {
        // Arrange
        var blocks = new[] { "test" };

        // Act
        Action act = () => _factory.Create(null!, blocks);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("meta");
    }

    [Fact]
    public void Create_WithNullBlocks_ShouldThrowArgumentNullException()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();

        // Act
        Action act = () => _factory.Create(meta, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("blocks");
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var meta = new LessonMetaData("");
        var blocks = new[] { "test" };

        // Act
        Action act = () => _factory.Create(meta, blocks);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Title*");
    }

    [Fact]
    public void Create_WithNullBlockElement_ShouldThrowArgumentException()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var blocks = new string?[] { "valid", null };

        // Act
        Action act = () => _factory.Create(meta, blocks);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*null*");
    }

    [Fact]
    public void Create_WithWhitespaceBlocks_ShouldNormalizeAndTrim()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var blocks = new[] { "  block1  ", "  block2  " };

        // Act
        var lesson = _factory.Create(meta, blocks);

        // Assert
        lesson.Blocks.Should().BeEquivalentTo(new[] { "block1", "block2" });
    }

    [Fact]
    public void Create_WithMultipleSpaces_ShouldCollapseToSingleSpace()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var blocks = new[] { "word1   word2", "word3    word4" };

        // Act
        var lesson = _factory.Create(meta, blocks);

        // Assert
        lesson.Blocks.Should().BeEquivalentTo(new[] { "word1 word2", "word3 word4" });
    }

    [Fact]
    public void Create_WithEmptyBlocks_ShouldFilterThemOut()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var blocks = new[] { "valid", "", "  ", "also valid" };

        // Act
        var lesson = _factory.Create(meta, blocks);

        // Assert
        lesson.Blocks.Should().BeEquivalentTo(new[] { "valid", "also valid" });
    }

    [Fact]
    public void Create_WithNewlinesInBlocks_ShouldCollapseToSpace()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var blocks = new[] { "line1\nline2", "line3\r\nline4" };

        // Act
        var lesson = _factory.Create(meta, blocks);

        // Assert
        lesson.Blocks.Should().BeEquivalentTo(new[] { "line1 line2", "line3 line4" });
    }

    [Fact]
    public void FromText_WithSimpleText_ShouldCreateLesson()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var text = "asdf jklö";

        // Act
        var lesson = _factory.FromText(meta, text);

        // Assert
        lesson.Should().NotBeNull();
        lesson.TargetText.Should().Contain("asdf");
        lesson.TargetText.Should().Contain("jklö");
    }

    [Fact]
    public void FromText_WithNullText_ShouldCreateEmptyLesson()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();

        // Act
        var lesson = _factory.FromText(meta, null);

        // Assert
        lesson.Blocks.Should().BeEmpty();
        lesson.TargetText.Should().BeEmpty();
    }

    [Fact]
    public void FromText_WithEmptyText_ShouldCreateEmptyLesson()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();

        // Act
        var lesson = _factory.FromText(meta, "");

        // Assert
        lesson.Blocks.Should().BeEmpty();
    }

    [Fact]
    public void FromText_WithWhitespaceOnly_ShouldCreateEmptyLesson()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();

        // Act
        var lesson = _factory.FromText(meta, "   \n\t  ");

        // Assert
        lesson.Blocks.Should().BeEmpty();
    }

    [Fact]
    public void FromText_ShouldNormalizeWhitespace()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var text = "word1   word2\n\nword3";

        // Act
        var lesson = _factory.FromText(meta, text);

        // Assert
        lesson.TargetText.Should().Be("word1 word2 word3");
    }

    [Theory]
    [InlineData("short", 10, 1)]
    [InlineData("word1 word2 word3", 10, 2)]
    [InlineData("a b c d e f g", 5, 7)]
    public void FromText_WithMaxBlockLen_ShouldWrapCorrectly(
        string text,
        int maxBlockLen,
        int expectedBlockCount)
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();

        // Act
        var lesson = _factory.FromText(meta, text, maxBlockLen);

        // Assert
        lesson.Blocks.Should().HaveCount(expectedBlockCount);
    }

    [Fact]
    public void FromText_WithLongWord_ShouldSplitHard()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var text = "Superkalifragilistikexpialigetisch";

        // Act
        var lesson = _factory.FromText(meta, text, maxBlockLen: 10);

        // Assert
        lesson.Blocks.Should().HaveCountGreaterThan(1);
        lesson.Blocks.Should().OnlyContain(b => b.Length <= 10);
    }

    [Fact]
    public void FromText_SoftWrap_ShouldNotBreakWords()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var text = "Das ist ein Test";

        // Act
        var lesson = _factory.FromText(meta, text, maxBlockLen: 10);

        // Assert
        // Sollte versuchen, Wörter zusammenzuhalten
        lesson.Blocks.Should().OnlyContain(b => !b.EndsWith(" ") && !b.StartsWith(" "));
    }

    [Fact]
    public void FromText_WithMaxBlockLenLessThanOne_ShouldTreatAsOne()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var text = "test";

        // Act
        var lesson = _factory.FromText(meta, text, maxBlockLen: 0);

        // Assert
        lesson.Should().NotBeNull();
        lesson.Blocks.Should().NotBeEmpty();
    }

    [Fact]
    public void FromText_WithExactMaxBlockLen_ShouldFitInOneBlock()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var text = "asdf jklö";

        // Act
        var lesson = _factory.FromText(meta, text, maxBlockLen: 9);

        // Assert
        lesson.Blocks.Should().HaveCount(1);
        lesson.Blocks[0].Should().Be("asdf jklö");
    }

    [Fact]
    public void FromText_WithJustOverMaxBlockLen_ShouldWrapToTwoBlocks()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var text = "asdf jklö";

        // Act
        var lesson = _factory.FromText(meta, text, maxBlockLen: 8);

        // Assert
        lesson.Blocks.Should().HaveCount(2);
    }

    [Theory]
    [InlineData("a b c", 5, "a b c")]
    [InlineData("a b c", 3, "a b c")]
    [InlineData("word1 word2", 20, "word1 word2")]
    public void FromText_ShouldPreserveContentInTargetText(
        string text,
        int maxBlockLen,
        string expectedTarget)
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();

        // Act
        var lesson = _factory.FromText(meta, text, maxBlockLen);

        // Assert
        lesson.TargetText.Should().Be(expectedTarget);
    }

    [Fact]
    public void FromText_WithMultipleSpacesAndNewlines_ShouldNormalize()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var text = "line1\n\n\nline2    line3\t\tline4";

        // Act
        var lesson = _factory.FromText(meta, text);

        // Assert
        lesson.TargetText.Should().Be("line1 line2 line3 line4");
    }

    [Fact]
    public void Create_AndFromText_ShouldProduceSameResultForSimpleCase()
    {
        // Arrange
        var meta = TestDataBuilder.CreateLessonMetaData();
        var blocks = new[] { "asdf", "jklö" };
        var text = "asdf jklö";

        // Act
        var lesson1 = _factory.Create(meta, blocks);
        var lesson2 = _factory.FromText(meta, text, maxBlockLen: 100);

        // Assert
        lesson1.TargetText.Should().Be(lesson2.TargetText);
    }

    [Fact]
    public void FromText_WithNullMetadata_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _factory.FromText(null!, "text");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("meta");
    }

    [Fact]
    public void FromText_WithEmptyTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var meta = new LessonMetaData("");

        // Act
        Action act = () => _factory.FromText(meta, "text");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Title*");
    }
}
