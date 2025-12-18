using FluentAssertions;
using Scriptum.Content.Data;
using Xunit;

namespace Scriptum.Content.Tests.Data;

public sealed class LessonGuideDataTests
{
    [Fact]
    public void Constructor_Should_Throw_WhenLessonIdIsEmpty()
    {
        var act = () => new LessonGuideData("");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("lessonId");
    }

    [Fact]
    public void Constructor_Should_Throw_WhenLessonIdIsWhitespace()
    {
        var act = () => new LessonGuideData("   ");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("lessonId");
    }

    [Fact]
    public void Constructor_Should_SetGuideTextMarkdownToEmptyString_WhenNotProvided()
    {
        var guide = new LessonGuideData("lesson1");

        guide.GuideTextMarkdown.Should().Be(string.Empty);
    }

    [Fact]
    public void Constructor_Should_SetGuideTextMarkdownToEmptyString_WhenNull()
    {
        var guide = new LessonGuideData("lesson1", guideTextMarkdown: null!);

        guide.GuideTextMarkdown.Should().Be(string.Empty);
    }

    [Fact]
    public void Constructor_Should_CreateValidInstance_WithAllParameters()
    {
        var guide = new LessonGuideData(
            lessonId: "lesson1",
            guideTextMarkdown: "# Anleitung\n\nDies ist eine **Anleitung**.");

        guide.LessonId.Should().Be("lesson1");
        guide.GuideTextMarkdown.Should().Be("# Anleitung\n\nDies ist eine **Anleitung**.");
    }
}
