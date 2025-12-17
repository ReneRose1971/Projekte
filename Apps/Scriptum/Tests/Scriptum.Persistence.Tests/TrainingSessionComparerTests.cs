using FluentAssertions;
using Scriptum.Progress;
using Scriptum.Core;
using System;
using System.Collections.Generic;
using Xunit;

namespace Scriptum.Persistence.Tests;

public sealed class TrainingSessionComparerTests
{
    private readonly TrainingSessionComparer _comparer = new();

    [Fact]
    public void Equals_Should_Return_True_ForSameInstance()
    {
        var session = CreateSession(1);

        var result = _comparer.Equals(session, session);

        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_Return_True_ForBothNull()
    {
        var result = _comparer.Equals(null, null);

        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_Return_False_WhenOneIsNull()
    {
        var session = CreateSession(1);

        _comparer.Equals(session, null).Should().BeFalse();
        _comparer.Equals(null, session).Should().BeFalse();
    }

    [Fact]
    public void Equals_Should_Return_True_ForIdenticalSessions()
    {
        var session1 = CreateSession(1);
        var session2 = CreateSession(1);

        var result = _comparer.Equals(session1, session2);

        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_Return_False_WhenIdDiffers()
    {
        var session1 = CreateSession(1);
        var session2 = CreateSession(2);

        var result = _comparer.Equals(session1, session2);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_Should_Return_False_WhenLessonIdDiffers()
    {
        var session1 = CreateSession(1, lessonId: "Lesson1");
        var session2 = CreateSession(1, lessonId: "Lesson2");

        var result = _comparer.Equals(session1, session2);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_Should_Return_False_WhenModuleIdDiffers()
    {
        var session1 = CreateSession(1, moduleId: "Module1");
        var session2 = CreateSession(1, moduleId: "Module2");

        var result = _comparer.Equals(session1, session2);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_Should_Return_False_WhenIsCompletedDiffers()
    {
        var session1 = CreateSession(1, isCompleted: true);
        var session2 = CreateSession(1, isCompleted: false);

        var result = _comparer.Equals(session1, session2);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_Should_Return_False_WhenInputsCountDiffers()
    {
        var session1 = CreateSession(1);
        session1.Inputs.Add(new StoredInput 
        { 
            Art = StoredInputKind.Zeichen,
            Zeitpunkt = DateTimeOffset.UtcNow,
            Taste = KeyId.A,
            Umschalter = ModifierSet.None,
            ErzeugtesGraphem = "A"
        });

        var session2 = CreateSession(1);

        var result = _comparer.Equals(session1, session2);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_Should_Return_False_WhenEvaluationsCountDiffers()
    {
        var session1 = CreateSession(1);
        session1.Evaluations.Add(new StoredEvaluation 
        { 
            TokenIndex = 0,
            Erwartet = "A",
            Tatsaechlich = "B",
            Ergebnis = EvaluationOutcome.Falsch
        });

        var session2 = CreateSession(1);

        var result = _comparer.Equals(session1, session2);

        result.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_Should_ReturnSameValue_ForIdenticalSessions()
    {
        var session1 = CreateSession(1);
        var session2 = CreateSession(1);

        var hash1 = _comparer.GetHashCode(session1);
        var hash2 = _comparer.GetHashCode(session2);

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetHashCode_Should_Throw_WhenSessionIsNull()
    {
        Action act = () => _comparer.GetHashCode(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    private static TrainingSession CreateSession(
        int id,
        string lessonId = "Lesson1",
        string moduleId = "Module1",
        bool isCompleted = false)
    {
        return new TrainingSession
        {
            Id = id,
            LessonId = lessonId,
            ModuleId = moduleId,
            StartedAt = DateTimeOffset.UtcNow,
            IsCompleted = isCompleted,
            Inputs = new List<StoredInput>(),
            Evaluations = new List<StoredEvaluation>()
        };
    }
}
