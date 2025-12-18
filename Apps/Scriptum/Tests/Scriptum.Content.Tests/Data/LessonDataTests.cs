using FluentAssertions;
using Scriptum.Content.Data;
using Xunit;

namespace Scriptum.Content.Tests.Data;

public sealed class LessonDataTests
{
    [Fact]
    public void Constructor_Should_Throw_WhenLessonIdIsEmpty()
    {
        var act = () => new LessonData("", "module1", "Titel", uebungstext: "Text");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("lessonId");
    }

    [Fact]
    public void Constructor_Should_Throw_WhenLessonIdIsWhitespace()
    {
        var act = () => new LessonData("   ", "module1", "Titel", uebungstext: "Text");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("lessonId");
    }

    [Fact]
    public void Constructor_Should_Throw_WhenModuleIdIsEmpty()
    {
        var act = () => new LessonData("lesson1", "", "Titel", uebungstext: "Text");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("moduleId");
    }

    [Fact]
    public void Constructor_Should_Throw_WhenModuleIdIsWhitespace()
    {
        var act = () => new LessonData("lesson1", "   ", "Titel", uebungstext: "Text");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("moduleId");
    }

    [Fact]
    public void Constructor_Should_Throw_WhenTitelIsEmpty()
    {
        var act = () => new LessonData("lesson1", "module1", "", uebungstext: "Text");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("titel");
    }

    [Fact]
    public void Constructor_Should_Throw_WhenTitelIsWhitespace()
    {
        var act = () => new LessonData("lesson1", "module1", "   ", uebungstext: "Text");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("titel");
    }

    [Fact]
    public void Constructor_Should_Throw_WhenUebungstextIsEmpty()
    {
        var act = () => new LessonData("lesson1", "module1", "Titel", uebungstext: "");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("uebungstext");
    }

    [Fact]
    public void Constructor_Should_Throw_WhenUebungstextIsWhitespace()
    {
        var act = () => new LessonData("lesson1", "module1", "Titel", uebungstext: "   ");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("uebungstext");
    }

    [Fact]
    public void Constructor_Should_Throw_WhenUebungstextIsNull()
    {
        var act = () => new LessonData("lesson1", "module1", "Titel", uebungstext: null);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("uebungstext");
    }

    [Fact]
    public void Constructor_Should_SetBeschreibungToEmptyString_WhenNotProvided()
    {
        var lesson = new LessonData("lesson1", "module1", "Titel", uebungstext: "Text");

        lesson.Beschreibung.Should().Be(string.Empty);
    }

    [Fact]
    public void Constructor_Should_SetBeschreibungToEmptyString_WhenNull()
    {
        var lesson = new LessonData("lesson1", "module1", "Titel", beschreibung: null!, uebungstext: "Text");

        lesson.Beschreibung.Should().Be(string.Empty);
    }

    [Fact]
    public void Constructor_Should_SetSchwierigkeitToZero_WhenNotProvided()
    {
        var lesson = new LessonData("lesson1", "module1", "Titel", uebungstext: "Text");

        lesson.Schwierigkeit.Should().Be(0);
    }

    [Fact]
    public void Constructor_Should_SetTagsToEmptyList_WhenNotProvided()
    {
        var lesson = new LessonData("lesson1", "module1", "Titel", uebungstext: "Text");

        lesson.Tags.Should().NotBeNull();
        lesson.Tags.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_Should_SetTagsToEmptyList_WhenNull()
    {
        var lesson = new LessonData("lesson1", "module1", "Titel", tags: null, uebungstext: "Text");

        lesson.Tags.Should().NotBeNull();
        lesson.Tags.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_Should_CreateValidInstance_WithAllParameters()
    {
        var tags = new[] { "anfänger", "buchstaben" };
        var lesson = new LessonData(
            lessonId: "lesson1",
            moduleId: "module1",
            titel: "Lektion 1",
            beschreibung: "Erste Lektion",
            schwierigkeit: 1,
            tags: tags,
            uebungstext: "aaa\nbbb");

        lesson.LessonId.Should().Be("lesson1");
        lesson.ModuleId.Should().Be("module1");
        lesson.Titel.Should().Be("Lektion 1");
        lesson.Beschreibung.Should().Be("Erste Lektion");
        lesson.Schwierigkeit.Should().Be(1);
        lesson.Tags.Should().BeEquivalentTo(tags);
        lesson.Uebungstext.Should().Be("aaa\nbbb");
    }
}
