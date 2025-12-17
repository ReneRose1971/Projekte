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
            lessonId: "L0001",
            bodyMarkdown: "# A guide for beginners\n\nContent here"
        );

        // Assert
        data.Should().NotBeNull();
        data.LessonId.Should().Be("L0001");
        data.BodyMarkdown.Should().Contain("beginners");
    }

    [Fact]
    public void Equality_WithSameLessonId_ShouldBeEqual()
    {
        // Arrange
        var data1 = new LessonGuideData("L0001", "desc1");
        var data2 = new LessonGuideData("L0001", "desc2");

        var comparer = new LessonGuideDataEqualityComparer();

        // Act & Assert
        comparer.Equals(data1, data2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentLessonId_ShouldNotBeEqual()
    {
        // Arrange
        var data1 = new LessonGuideData("L0001", "desc");
        var data2 = new LessonGuideData("L0002", "desc");

        var comparer = new LessonGuideDataEqualityComparer();

        // Act & Assert
        comparer.Equals(data1, data2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameLessonId_ShouldReturnSameHash()
    {
        // Arrange
        var data1 = new LessonGuideData("L0001", "desc1");
        var data2 = new LessonGuideData("L0001", "desc2");

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
        var data1 = new LessonGuideData("L0001", "desc");
        var data2 = new LessonGuideData("L0001", "desc");

        // Act & Assert
        data1.Should().Be(data2);
    }

    [Fact]
    public void Record_Immutability_ShouldSupportWith()
    {
        // Arrange
        var original = TestDataBuilder.CreateLessonGuideData(
            lessonId: "L0001",
            bodyMarkdown: "Original description"
        );

        // Act
        var modified = original with { BodyMarkdown = "Modified description" };

        // Assert
        original.BodyMarkdown.Should().Be("Original description");
        modified.BodyMarkdown.Should().Be("Modified description");
        modified.LessonId.Should().Be("L0001");
    }

    [Fact]
    public void BodyMarkdown_WithEmptyString_ShouldSucceed()
    {
        // Arrange & Act
        var data = new LessonGuideData("L0001", "");

        // Assert
        data.BodyMarkdown.Should().BeEmpty();
    }

    [Fact]
    public void DefaultConstructor_ShouldCreateInstanceWithEmptyValues()
    {
        // Act
        var data = new LessonGuideData();

        // Assert
        data.LessonId.Should().BeEmpty();
        data.BodyMarkdown.Should().BeEmpty();
    }

    [Fact]
    public void ParameterizedConstructor_WithNullLessonId_ShouldThrow()
    {
        // Act
        Action act = () => new LessonGuideData(null!, "content");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("lessonId");
    }

    [Fact]
    public void ParameterizedConstructor_WithNullBodyMarkdown_ShouldUseEmpty()
    {
        // Act
        var data = new LessonGuideData("L0001", null!);

        // Assert
        data.BodyMarkdown.Should().BeEmpty();
    }

    [Fact]
    public void ToString_ShouldReturnReadableRepresentation()
    {
        // Arrange
        var data = TestDataBuilder.CreateLessonGuideData(lessonId: "L0001");

        // Act
        var result = data.ToString();

        // Assert
        result.Should().Contain("L0001");
    }

    [Theory]
    [InlineData("")]
    [InlineData("Guide with markdown")]
    public void BodyMarkdown_ShouldSupportVariousValues(string markdown)
    {
        // Arrange & Act
        var data = new LessonGuideData("L0001", markdown);

        // Assert
        data.BodyMarkdown.Should().Be(markdown);
    }

    [Fact]
    public void Comparer_ShouldBeUsableInHashSet()
    {
        // Arrange
        var set = new HashSet<LessonGuideData>(new LessonGuideDataEqualityComparer());
        var data1 = new LessonGuideData("L0001", "desc1");
        var data2 = new LessonGuideData("L0001", "desc2");
        var data3 = new LessonGuideData("L0002", "desc");

        // Act
        set.Add(data1);
        set.Add(data2); // Sollte nicht hinzugefügt werden (gleiche LessonId)
        set.Add(data3);

        // Assert
        set.Should().HaveCount(2);
        set.Should().Contain(data1);
        set.Should().Contain(data3);
    }

    [Fact]
    public void BodyMarkdown_WithMultilineMarkdown_ShouldPreserveFormatting()
    {
        // Arrange
        var markdown = "# Title\n\n## Section\n\n- Item 1\n- Item 2";

        // Act
        var data = new LessonGuideData("L0001", markdown);

        // Assert
        data.BodyMarkdown.Should().Contain("\n");
        data.BodyMarkdown.Should().Be(markdown);
    }

    [Fact]
    public void InitProperties_ShouldAllowSetting()
    {
        // Act
        var data = new LessonGuideData
        {
            LessonId = "L0001",
            BodyMarkdown = "Content"
        };

        // Assert
        data.LessonId.Should().Be("L0001");
        data.BodyMarkdown.Should().Be("Content");
    }
}
