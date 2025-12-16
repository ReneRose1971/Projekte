using FluentAssertions;
using TypeTutor.Logic.Data;
using TypeTutor.Logic.Tests.Helpers;
using Xunit;

namespace TypeTutor.Logic.Tests.Data;

/// <summary>
/// Tests für LessonGuideData Record.
/// Validiert Konstruktion, Equality und Immutability.
/// </summary>
public sealed class LessonGuideDataTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var data = TestDataBuilder.CreateLessonGuideData(
            title: "Basic Guide",
            bodyMarkdown: "# A guide for beginners\n\nContent here"
        );

        // Assert
        data.Should().NotBeNull();
        data.Title.Should().Be("Basic Guide");
        data.BodyMarkDown.Should().Contain("beginners");
    }

    [Fact]
    public void Equality_WithSameTitle_ShouldBeEqual()
    {
        // Arrange
        var data1 = new LessonGuideData("Guide A", "desc1");
        var data2 = new LessonGuideData("Guide A", "desc2");

        var comparer = new LessonGuideDataEqualityComparer();

        // Act & Assert
        comparer.Equals(data1, data2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentTitle_ShouldNotBeEqual()
    {
        // Arrange
        var data1 = new LessonGuideData("Guide A", "desc");
        var data2 = new LessonGuideData("Guide B", "desc");

        var comparer = new LessonGuideDataEqualityComparer();

        // Act & Assert
        comparer.Equals(data1, data2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameTitle_ShouldReturnSameHash()
    {
        // Arrange
        var data1 = new LessonGuideData("Guide A", "desc1");
        var data2 = new LessonGuideData("Guide A", "desc2");

        var comparer = new LessonGuideDataEqualityComparer();

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
        var data1 = new LessonGuideData("Test", "desc");
        var data2 = new LessonGuideData("Test", "desc");

        // Act & Assert
        data1.Should().Be(data2);
    }

    [Fact]
    public void Record_Immutability_ShouldSupportWith()
    {
        // Arrange
        var original = TestDataBuilder.CreateLessonGuideData(
            title: "Original Guide",
            bodyMarkdown: "Original description"
        );

        // Act
        var modified = original with { BodyMarkDown = "Modified description" };

        // Assert
        original.BodyMarkDown.Should().Be("Original description");
        modified.BodyMarkDown.Should().Be("Modified description");
        modified.Title.Should().Be("Original Guide");
    }

    [Fact]
    public void BodyMarkDown_WithEmptyString_ShouldSucceed()
    {
        // Arrange & Act
        var data = new LessonGuideData("Empty Guide", "");

        // Assert
        data.BodyMarkDown.Should().BeEmpty();
    }

    [Fact]
    public void DefaultConstructor_ShouldCreateInstanceWithEmptyValues()
    {
        // Act
        var data = new LessonGuideData();

        // Assert
        data.Title.Should().BeEmpty();
        data.BodyMarkDown.Should().BeEmpty();
    }

    [Fact]
    public void ParameterizedConstructor_WithNullTitle_ShouldThrow()
    {
        // Act
        Action act = () => new LessonGuideData(null!, "content");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("title");
    }

    [Fact]
    public void ParameterizedConstructor_WithNullBodyMarkDown_ShouldUseEmpty()
    {
        // Act
        var data = new LessonGuideData("Guide", null!);

        // Assert
        data.BodyMarkDown.Should().BeEmpty();
    }

    [Fact]
    public void ToString_ShouldReturnReadableRepresentation()
    {
        // Arrange
        var data = TestDataBuilder.CreateLessonGuideData(title: "My Guide");

        // Act
        var result = data.ToString();

        // Assert
        result.Should().Contain("My Guide");
    }

    [Theory]
    [InlineData("")]
    [InlineData("Guide with markdown")]
    public void BodyMarkDown_ShouldSupportVariousValues(string markdown)
    {
        // Arrange & Act
        var data = new LessonGuideData("Test", markdown);

        // Assert
        data.BodyMarkDown.Should().Be(markdown);
    }

    [Fact]
    public void Comparer_ShouldBeUsableInHashSet()
    {
        // Arrange
        var set = new HashSet<LessonGuideData>(new LessonGuideDataEqualityComparer());
        var data1 = new LessonGuideData("Guide", "desc1");
        var data2 = new LessonGuideData("Guide", "desc2");
        var data3 = new LessonGuideData("Other", "desc");

        // Act
        set.Add(data1);
        set.Add(data2); // Sollte nicht hinzugefügt werden (gleicher Titel)
        set.Add(data3);

        // Assert
        set.Should().HaveCount(2);
        set.Should().Contain(data1);
        set.Should().Contain(data3);
    }

    [Fact]
    public void BodyMarkDown_WithMultilineMarkdown_ShouldPreserveFormatting()
    {
        // Arrange
        var markdown = "# Title\n\n## Section\n\n- Item 1\n- Item 2";

        // Act
        var data = new LessonGuideData("Guide", markdown);

        // Assert
        data.BodyMarkDown.Should().Contain("\n");
        data.BodyMarkDown.Should().Be(markdown);
    }

    [Fact]
    public void InitProperties_ShouldAllowSetting()
    {
        // Act
        var data = new LessonGuideData
        {
            Title = "Test",
            BodyMarkDown = "Content"
        };

        // Assert
        data.Title.Should().Be("Test");
        data.BodyMarkDown.Should().Be("Content");
    }
}
