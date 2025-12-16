using FluentAssertions;
using TypeTutor.Logic.Data;
using Xunit;

namespace TypeTutor.Logic.Tests.Data;

/// <summary>
/// Tests für LessonDataEqualityComparer.
/// Validiert die Title-basierte Equality-Logik.
/// </summary>
public sealed class LessonDataEqualityComparerTests
{
    private readonly LessonDataEqualityComparer _comparer = new();

    [Fact]
    public void Equals_WithSameTitle_ShouldReturnTrue()
    {
        // Arrange
        var data1 = new LessonData("Home Row", "asdf") { Description = "Basic", Difficulty = 1 };
        var data2 = new LessonData("Home Row", "jklö") { Description = "Different", Difficulty = 2 };

        // Act
        var result = _comparer.Equals(data1, data2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentTitle_ShouldReturnFalse()
    {
        // Arrange
        var data1 = new LessonData("Home Row", "content") { Description = "desc", Difficulty = 1 };
        var data2 = new LessonData("Upper Row", "content") { Description = "desc", Difficulty = 1 };

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
        var data = new LessonData("Test", "content") { Description = "desc", Difficulty = 1 };

        // Act
        var result1 = _comparer.Equals(data, null);
        var result2 = _comparer.Equals(null, data);

        // Assert
        result1.Should().BeFalse();
        result2.Should().BeFalse();
    }

    [Theory]
    [InlineData("Home Row", "Home Row", true)]
    [InlineData("Home Row", "home row", false)]
    [InlineData("Lesson A", "Lesson B", false)]
    [InlineData("", "", true)]
    public void Equals_WithVariousTitles_ShouldCompareCorrectly(
        string title1,
        string title2,
        bool expected)
    {
        // Arrange
        var data1 = new LessonData(title1, "content") { Description = "desc", Difficulty = 1 };
        var data2 = new LessonData(title2, "content") { Description = "desc", Difficulty = 1 };

        // Act
        var result = _comparer.Equals(data1, data2);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetHashCode_WithSameTitle_ShouldReturnSameHash()
    {
        // Arrange
        var data1 = new LessonData("Test Lesson", "content1") { Description = "desc1", Difficulty = 1 };
        var data2 = new LessonData("Test Lesson", "content2") { Description = "desc2", Difficulty = 2 };

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
        var data1 = new LessonData("Lesson A", "content") { Description = "desc", Difficulty = 1 };
        var data2 = new LessonData("Lesson B", "content") { Description = "desc", Difficulty = 1 };

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
        var data1 = new LessonData("", "content1") { Description = "desc1", Difficulty = 1 };
        var data2 = new LessonData("", "content2") { Description = "desc2", Difficulty = 2 };

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
        var set = new HashSet<LessonData>(_comparer);
        var data1 = new LessonData("Lesson", "content1") { Description = "desc1", Difficulty = 1 };
        var data2 = new LessonData("Lesson", "content2") { Description = "desc2", Difficulty = 2 };
        var data3 = new LessonData("Other", "content") { Description = "desc", Difficulty = 1 };

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
        var dict = new Dictionary<LessonData, string>(_comparer);
        var data1 = new LessonData("Key", "content1") { Description = "desc1", Difficulty = 1 };
        var data2 = new LessonData("Key", "content2") { Description = "desc2", Difficulty = 2 };

        // Act
        dict[data1] = "Value1";
        dict[data2] = "Value2"; // Sollte Value1 überschreiben

        // Assert
        dict.Should().HaveCount(1);
        dict[data1].Should().Be("Value2");
    }
}
