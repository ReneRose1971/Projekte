using FluentAssertions;
using TypeTutor.Logic.Data;
using Xunit;

namespace TypeTutor.Logic.Tests.Data;

/// <summary>
/// Tests für LessonDataEqualityComparer.
/// Validiert die LessonId-basierte Equality-Logik.
/// </summary>
public sealed class LessonDataEqualityComparerTests
{
    private readonly LessonDataEqualityComparer _comparer = new();

    [Fact]
    public void Equals_WithSameLessonId_ShouldReturnTrue()
    {
        // Arrange
        var data1 = new LessonData("L0001", "Home Row", "asdf") { Description = "Basic", Difficulty = 1 };
        var data2 = new LessonData("L0001", "Upper Row", "jklö") { Description = "Different", Difficulty = 2 };

        // Act
        var result = _comparer.Equals(data1, data2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentLessonId_ShouldReturnFalse()
    {
        // Arrange
        var data1 = new LessonData("L0001", "Home Row", "content") { Description = "desc", Difficulty = 1 };
        var data2 = new LessonData("L0002", "Upper Row", "content") { Description = "desc", Difficulty = 1 };

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
        var data = new LessonData("L0001", "Test", "content") { Description = "desc", Difficulty = 1 };

        // Act
        var result1 = _comparer.Equals(data, null);
        var result2 = _comparer.Equals(null, data);

        // Assert
        result1.Should().BeFalse();
        result2.Should().BeFalse();
    }

    [Theory]
    [InlineData("L0001", "L0001", true)]
    [InlineData("L0001", "l0001", false)]
    [InlineData("L0001", "L0002", false)]
    [InlineData("", "", true)]
    public void Equals_WithVariousLessonIds_ShouldCompareCorrectly(
        string lessonId1,
        string lessonId2,
        bool expected)
    {
        // Arrange
        var data1 = new LessonData(lessonId1, "Lesson A", "content") { Description = "desc", Difficulty = 1 };
        var data2 = new LessonData(lessonId2, "Lesson B", "content") { Description = "desc", Difficulty = 1 };

        // Act
        var result = _comparer.Equals(data1, data2);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetHashCode_WithSameLessonId_ShouldReturnSameHash()
    {
        // Arrange
        var data1 = new LessonData("L0001", "Test Lesson A", "content1") { Description = "desc1", Difficulty = 1 };
        var data2 = new LessonData("L0001", "Test Lesson B", "content2") { Description = "desc2", Difficulty = 2 };

        // Act
        var hash1 = _comparer.GetHashCode(data1);
        var hash2 = _comparer.GetHashCode(data2);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetHashCode_WithDifferentLessonId_ShouldReturnDifferentHash()
    {
        // Arrange
        var data1 = new LessonData("L0001", "Lesson A", "content") { Description = "desc", Difficulty = 1 };
        var data2 = new LessonData("L0002", "Lesson B", "content") { Description = "desc", Difficulty = 1 };

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
    public void GetHashCode_WithEmptyLessonId_ShouldReturnConsistentHash()
    {
        // Arrange
        var data1 = new LessonData("", "Lesson A", "content1") { Description = "desc1", Difficulty = 1 };
        var data2 = new LessonData("", "Lesson B", "content2") { Description = "desc2", Difficulty = 2 };

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
        var data1 = new LessonData("L0001", "Lesson", "content1") { Description = "desc1", Difficulty = 1 };
        var data2 = new LessonData("L0001", "Other Lesson", "content2") { Description = "desc2", Difficulty = 2 };
        var data3 = new LessonData("L0002", "Other", "content") { Description = "desc", Difficulty = 1 };

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
    public void Comparer_ShouldBeUsableInDictionary()
    {
        // Arrange
        var dict = new Dictionary<LessonData, string>(_comparer);
        var data1 = new LessonData("L0001", "Key A", "content1") { Description = "desc1", Difficulty = 1 };
        var data2 = new LessonData("L0001", "Key B", "content2") { Description = "desc2", Difficulty = 2 };

        // Act
        dict[data1] = "Value1";
        dict[data2] = "Value2"; // Sollte Value1 überschreiben

        // Assert
        dict.Should().HaveCount(1);
        dict[data1].Should().Be("Value2");
    }
}
