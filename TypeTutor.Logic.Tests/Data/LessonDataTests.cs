using FluentAssertions;
using TypeTutor.Logic.Data;
using TypeTutor.Logic.Tests.Helpers;
using Xunit;

namespace TypeTutor.Logic.Tests.Data;

/// <summary>
/// Tests für LessonData Record.
/// Validiert Konstruktion, Equality, Immutability und Serialisierung.
/// </summary>
public sealed class LessonDataTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var data = TestDataBuilder.CreateLessonData(
            lessonId: "L0001",
            title: "Test Lesson",
            content: "asdf jklö",
            description: "Basic test lesson",
            difficulty: 1
        );

        // Assert
        data.Should().NotBeNull();
        data.LessonId.Should().Be("L0001");
        data.Title.Should().Be("Test Lesson");
        data.Content.Should().Be("asdf jklö");
        data.Description.Should().Be("Basic test lesson");
        data.Difficulty.Should().Be(1);
    }

    [Fact]
    public void Equality_WithSameLessonId_ShouldBeEqual()
    {
        // Arrange
        var data1 = new LessonData("L0001", "Lesson A", "content1") 
        { 
            Description = "desc1", 
            Difficulty = 1 
        };
        var data2 = new LessonData("L0001", "Lesson B", "content2") 
        { 
            Description = "desc2", 
            Difficulty = 2 
        };

        var comparer = new LessonDataEqualityComparer();

        // Act & Assert
        comparer.Equals(data1, data2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentLessonId_ShouldNotBeEqual()
    {
        // Arrange
        var data1 = new LessonData("L0001", "Lesson A", "content") 
        { 
            Description = "desc", 
            Difficulty = 1 
        };
        var data2 = new LessonData("L0002", "Lesson B", "content") 
        { 
            Description = "desc", 
            Difficulty = 1 
        };

        var comparer = new LessonDataEqualityComparer();

        // Act & Assert
        comparer.Equals(data1, data2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameLessonId_ShouldReturnSameHash()
    {
        // Arrange
        var data1 = new LessonData("L0001", "Lesson A", "content1") 
        { 
            Description = "desc1", 
            Difficulty = 1 
        };
        var data2 = new LessonData("L0001", "Lesson B", "content2") 
        { 
            Description = "desc2", 
            Difficulty = 2 
        };

        var comparer = new LessonDataEqualityComparer();

        // Act
        var hash1 = comparer.GetHashCode(data1);
        var hash2 = comparer.GetHashCode(data2);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Record_Equality_WithIdenticalValues_ShouldBeEqual()
    {
        // Arrange
        var data1 = new LessonData("L0001", "Test", "content") 
        { 
            Description = "desc", 
            Difficulty = 1 
        };
        var data2 = new LessonData("L0001", "Test", "content") 
        { 
            Description = "desc", 
            Difficulty = 1 
        };

        // Act & Assert
        data1.Should().Be(data2);
        (data1 == data2).Should().BeTrue();
    }

    [Fact]
    public void Record_Immutability_ShouldSupportWith()
    {
        // Arrange
        var original = TestDataBuilder.CreateLessonData(
            lessonId: "L0001",
            title: "Original",
            difficulty: 1
        );

        // Act
        var modified = original with { Difficulty = 2 };

        // Assert
        original.Difficulty.Should().Be(1);
        modified.Difficulty.Should().Be(2);
        modified.Title.Should().Be("Original");
    }

    [Theory]
    [InlineData("L0001", "", "content", "desc", 1)]
    [InlineData("L0001", "Title", "", "desc", 1)]
    public void Constructor_WithValidEmptyStrings_ShouldSucceed(
        string lessonId,
        string title,
        string content,
        string description,
        int difficulty)
    {
        // Act
        var data = new LessonData(lessonId, title, content)
        {
            Description = description,
            Difficulty = difficulty
        };

        // Assert
        data.Should().NotBeNull();
        data.LessonId.Should().Be(lessonId);
        data.Title.Should().Be(title);
        data.Content.Should().Be(content);
    }

    [Fact]
    public void Content_WithMultipleLines_ShouldPreserveLineBreaks()
    {
        // Arrange
        var content = "Line 1\nLine 2\nLine 3";

        // Act
        var data = new LessonData("L0001", "Test", content) { Description = "desc", Difficulty = 1 };

        // Assert
        data.Content.Should().Contain("\n");
        data.Content.Should().Be(content);
    }

    [Fact]
    public void Difficulty_ShouldAllowPositiveValues()
    {
        // Arrange & Act
        var data = new LessonData("L0001", "Test", "content") { Description = "desc", Difficulty = 5 };

        // Assert
        data.Difficulty.Should().Be(5);
    }

    [Fact]
    public void Difficulty_ShouldAllowZero()
    {
        // Arrange & Act
        var data = new LessonData("L0001", "Test", "content") { Description = "desc", Difficulty = 0 };

        // Assert
        data.Difficulty.Should().Be(0);
    }

    [Fact]
    public void ToString_ShouldReturnReadableRepresentation()
    {
        // Arrange
        var data = TestDataBuilder.CreateLessonData(lessonId: "L0001", title: "My Lesson");

        // Act
        var result = data.ToString();

        // Assert
        result.Should().Contain("My Lesson");
    }
}
