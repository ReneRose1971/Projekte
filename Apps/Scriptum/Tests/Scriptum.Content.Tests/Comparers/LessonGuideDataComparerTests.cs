using FluentAssertions;
using Scriptum.Content.Comparers;
using Scriptum.Content.Data;
using Xunit;

namespace Scriptum.Content.Tests.Comparers;

public sealed class LessonGuideDataComparerTests
{
    private readonly LessonGuideDataComparer _comparer = new();

    [Fact]
    public void Equals_Should_Return_True_ForSameInstance()
    {
        var guide = new LessonGuideData("lesson1", "Guide");

        var result = _comparer.Equals(guide, guide);

        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_Return_True_ForBothNull()
    {
        var result = _comparer.Equals(null, null);

        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_Return_False_WhenFirstIsNull()
    {
        var guide = new LessonGuideData("lesson1", "Guide");

        var result = _comparer.Equals(null, guide);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_Should_Return_False_WhenSecondIsNull()
    {
        var guide = new LessonGuideData("lesson1", "Guide");

        var result = _comparer.Equals(guide, null);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_Should_Return_True_ForSameLessonId()
    {
        var guide1 = new LessonGuideData("lesson1", "Guide 1");
        var guide2 = new LessonGuideData("lesson1", "Guide 2");

        var result = _comparer.Equals(guide1, guide2);

        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_Return_False_ForDifferentLessonIds()
    {
        var guide1 = new LessonGuideData("lesson1", "Guide");
        var guide2 = new LessonGuideData("lesson2", "Guide");

        var result = _comparer.Equals(guide1, guide2);

        result.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_Should_ReturnSameValue_ForSameLessonId()
    {
        var guide1 = new LessonGuideData("lesson1", "Guide 1");
        var guide2 = new LessonGuideData("lesson1", "Guide 2");

        var hash1 = _comparer.GetHashCode(guide1);
        var hash2 = _comparer.GetHashCode(guide2);

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetHashCode_Should_Throw_WhenGuideIsNull()
    {
        var act = () => _comparer.GetHashCode(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("obj");
    }
}
