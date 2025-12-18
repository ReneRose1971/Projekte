using FluentAssertions;
using Scriptum.Content.Comparers;
using Scriptum.Content.Data;
using Xunit;

namespace Scriptum.Content.Tests.Comparers;

public sealed class LessonDataComparerTests
{
    private readonly LessonDataComparer _comparer = new();

    [Fact]
    public void Equals_Should_Return_True_ForSameInstance()
    {
        var lesson = new LessonData("lesson1", "module1", "Titel", uebungstext: "Text");

        var result = _comparer.Equals(lesson, lesson);

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
        var lesson = new LessonData("lesson1", "module1", "Titel", uebungstext: "Text");

        var result = _comparer.Equals(null, lesson);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_Should_Return_False_WhenSecondIsNull()
    {
        var lesson = new LessonData("lesson1", "module1", "Titel", uebungstext: "Text");

        var result = _comparer.Equals(lesson, null);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_Should_Return_True_ForSameLessonId()
    {
        var lesson1 = new LessonData("lesson1", "module1", "Titel 1", uebungstext: "Text1");
        var lesson2 = new LessonData("lesson1", "module2", "Titel 2", uebungstext: "Text2");

        var result = _comparer.Equals(lesson1, lesson2);

        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_Return_False_ForDifferentLessonIds()
    {
        var lesson1 = new LessonData("lesson1", "module1", "Titel", uebungstext: "Text");
        var lesson2 = new LessonData("lesson2", "module1", "Titel", uebungstext: "Text");

        var result = _comparer.Equals(lesson1, lesson2);

        result.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_Should_ReturnSameValue_ForSameLessonId()
    {
        var lesson1 = new LessonData("lesson1", "module1", "Titel 1", uebungstext: "Text1");
        var lesson2 = new LessonData("lesson1", "module2", "Titel 2", uebungstext: "Text2");

        var hash1 = _comparer.GetHashCode(lesson1);
        var hash2 = _comparer.GetHashCode(lesson2);

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetHashCode_Should_Throw_WhenLessonIsNull()
    {
        var act = () => _comparer.GetHashCode(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("obj");
    }
}
