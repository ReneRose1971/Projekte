using FluentAssertions;
using TypeTutor.Logic.Data;
using Xunit;

namespace TypeTutor.Logic.Tests.Data;

/// <summary>
/// Tests für LessonGuideDataEqualityComparer.
/// Validiert die Title-basierte Equality-Logik für LessonGuides.
/// </summary>
public sealed class LessonGuideDataEqualityComparerTests
{
    private readonly LessonGuideDataEqualityComparer _comparer = new();

    [Fact]
    public void Equals_WithSameTitle_ShouldReturnTrue()
    {
        // Arrange
        var data1 = new LessonGuideData("Beginner Course", "Description 1");
        var data2 = new LessonGuideData("Beginner Course", "Description 2");

        // Act
        var result = _comparer.Equals(data1, data2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentTitle_ShouldReturnFalse()
    {
        // Arrange
        var data1 = new LessonGuideData("Beginner", "desc");
        var data2 = new LessonGuideData("Advanced", "desc");

        // Act
        var result = _comparer.Equals(data1, data2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithBothNull_ShouldReturnTrue()
    {
        // Act
        var result = _comparer.Equals(null, null);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithOneNull_ShouldReturnFalse()
    {
        // Arrange
        var data = new LessonGuideData("Test", "desc");

        // Act
        var result1 = _comparer.Equals(data, null);
        var result2 = _comparer.Equals(null, data);

        // Assert
        result1.Should().BeFalse();
        result2.Should().BeFalse();
    }

    [Theory]
    [InlineData("Guide A", "Guide A", true)]
    [InlineData("Guide A", "guide a", false)]
    [InlineData("Beginner", "Advanced", false)]
    [InlineData("", "", true)]
    public void Equals_WithVariousTitles_ShouldCompareCorrectly(
        string title1,
        string title2,
        bool expected)
    {
        // Arrange
        var data1 = new LessonGuideData(title1, "desc");
        var data2 = new LessonGuideData(title2, "desc");

        // Act
        var result = _comparer.Equals(data1, data2);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetHashCode_WithSameTitle_ShouldReturnSameHash()
    {
        // Arrange
        var data1 = new LessonGuideData("Test Guide", "desc1");
        var data2 = new LessonGuideData("Test Guide", "desc2");

        // Act
        var hash1 = _comparer.GetHashCode(data1);
        var hash2 = _comparer.GetHashCode(data2);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetHashCode_WithDifferentTitle_ShouldReturnDifferentHash()
    {
        // Arrange
        var data1 = new LessonGuideData("Guide A", "desc");
        var data2 = new LessonGuideData("Guide B", "desc");

        // Act
        var hash1 = _comparer.GetHashCode(data1);
        var hash2 = _comparer.GetHashCode(data2);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void GetHashCode_WithNull_ShouldReturnZero()
    {
        // Act
        var hash = _comparer.GetHashCode(null!);

        // Assert
        hash.Should().Be(0);
    }

    [Fact]
    public void GetHashCode_WithEmptyTitle_ShouldReturnConsistentHash()
    {
        // Arrange
        var data1 = new LessonGuideData("", "desc1");
        var data2 = new LessonGuideData("", "desc2");

        // Act
        var hash1 = _comparer.GetHashCode(data1);
        var hash2 = _comparer.GetHashCode(data2);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Comparer_ShouldBeUsableInHashSet()
    {
        // Arrange
        var set = new HashSet<LessonGuideData>(_comparer);
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
    public void Comparer_ShouldBeUsableInDictionary()
    {
        // Arrange
        var dict = new Dictionary<LessonGuideData, string>(_comparer);
        var data1 = new LessonGuideData("Key", "desc1");
        var data2 = new LessonGuideData("Key", "desc2");

        // Act
        dict[data1] = "Value1";
        dict[data2] = "Value2"; // Sollte Value1 überschreiben

        // Assert
        dict.Should().HaveCount(1);
        dict[data1].Should().Be("Value2");
    }
}
