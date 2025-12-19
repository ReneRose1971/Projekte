using FluentAssertions;
using Scriptum.Progress;
using Xunit;

namespace Scriptum.Progress.Tests;

public sealed class TrainingSessionFactoryTests
{
    [Fact]
    public void CreateNew_ShouldCreateSessionWithCorrectLessonId()
    {
        var lessonId = "lesson1";
        var moduleId = "module1";
        var startedAt = DateTimeOffset.Now;

        var session = TrainingSession.CreateNew(lessonId, moduleId, startedAt);

        session.LessonId.Should().Be(lessonId);
    }

    [Fact]
    public void CreateNew_ShouldCreateSessionWithCorrectModuleId()
    {
        var lessonId = "lesson1";
        var moduleId = "module1";
        var startedAt = DateTimeOffset.Now;

        var session = TrainingSession.CreateNew(lessonId, moduleId, startedAt);

        session.ModuleId.Should().Be(moduleId);
    }

    [Fact]
    public void CreateNew_ShouldCreateSessionWithCorrectStartedAt()
    {
        var lessonId = "lesson1";
        var moduleId = "module1";
        var startedAt = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);

        var session = TrainingSession.CreateNew(lessonId, moduleId, startedAt);

        session.StartedAt.Should().Be(startedAt);
    }

    [Fact]
    public void CreateNew_ShouldSetIsCompletedToFalse()
    {
        var lessonId = "lesson1";
        var moduleId = "module1";
        var startedAt = DateTimeOffset.Now;

        var session = TrainingSession.CreateNew(lessonId, moduleId, startedAt);

        session.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void CreateNew_ShouldSetEndedAtToNull()
    {
        var lessonId = "lesson1";
        var moduleId = "module1";
        var startedAt = DateTimeOffset.Now;

        var session = TrainingSession.CreateNew(lessonId, moduleId, startedAt);

        session.EndedAt.Should().BeNull();
    }

    [Fact]
    public void CreateNew_ShouldInitializeEmptyInputsList()
    {
        var lessonId = "lesson1";
        var moduleId = "module1";
        var startedAt = DateTimeOffset.Now;

        var session = TrainingSession.CreateNew(lessonId, moduleId, startedAt);

        session.Inputs.Should().NotBeNull();
        session.Inputs.Should().BeEmpty();
    }

    [Fact]
    public void CreateNew_ShouldInitializeEmptyEvaluationsList()
    {
        var lessonId = "lesson1";
        var moduleId = "module1";
        var startedAt = DateTimeOffset.Now;

        var session = TrainingSession.CreateNew(lessonId, moduleId, startedAt);

        session.Evaluations.Should().NotBeNull();
        session.Evaluations.Should().BeEmpty();
    }

    [Fact]
    public void CreateNew_WithEmptyLessonId_ShouldThrowArgumentException()
    {
        var lessonId = "";
        var moduleId = "module1";
        var startedAt = DateTimeOffset.Now;

        var act = () => TrainingSession.CreateNew(lessonId, moduleId, startedAt);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("lessonId");
    }

    [Fact]
    public void CreateNew_WithWhitespaceLessonId_ShouldThrowArgumentException()
    {
        var lessonId = "   ";
        var moduleId = "module1";
        var startedAt = DateTimeOffset.Now;

        var act = () => TrainingSession.CreateNew(lessonId, moduleId, startedAt);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("lessonId");
    }

    [Fact]
    public void CreateNew_WithEmptyModuleId_ShouldThrowArgumentException()
    {
        var lessonId = "lesson1";
        var moduleId = "";
        var startedAt = DateTimeOffset.Now;

        var act = () => TrainingSession.CreateNew(lessonId, moduleId, startedAt);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("moduleId");
    }

    [Fact]
    public void CreateNew_WithWhitespaceModuleId_ShouldThrowArgumentException()
    {
        var lessonId = "lesson1";
        var moduleId = "   ";
        var startedAt = DateTimeOffset.Now;

        var act = () => TrainingSession.CreateNew(lessonId, moduleId, startedAt);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("moduleId");
    }
}
