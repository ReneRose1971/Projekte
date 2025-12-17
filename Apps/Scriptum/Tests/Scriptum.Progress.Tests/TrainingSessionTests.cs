using FluentAssertions;
using Scriptum.Progress;
using Xunit;

namespace Scriptum.Progress.Tests;

/// <summary>
/// Tests für <see cref="TrainingSession"/>.
/// </summary>
public sealed class TrainingSessionTests
{
    [Fact]
    public void Constructor_CreatesInstance_WithDefaultValues()
    {
        var session = new TrainingSession();

        session.Should().NotBeNull();
    }

    [Fact]
    public void LessonId_Null_ThrowsArgumentNullException()
    {
        var session = new TrainingSession();
        
        var act = () => session.LessonId = null!;

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*LessonId*");
    }

    [Fact]
    public void LessonId_EmptyString_IsAllowed()
    {
        var session = new TrainingSession();
        
        session.LessonId = string.Empty;

        session.LessonId.Should().BeEmpty();
    }

    [Fact]
    public void LessonId_ValidValue_SetCorrectly()
    {
        var session = new TrainingSession();
        
        session.LessonId = "lesson-001";

        session.LessonId.Should().Be("lesson-001");
    }

    [Fact]
    public void ModuleId_Null_ThrowsArgumentNullException()
    {
        var session = new TrainingSession();
        
        var act = () => session.ModuleId = null!;

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*ModuleId*");
    }

    [Fact]
    public void ModuleId_EmptyString_IsAllowed()
    {
        var session = new TrainingSession();
        
        session.ModuleId = string.Empty;

        session.ModuleId.Should().BeEmpty();
    }

    [Fact]
    public void ModuleId_ValidValue_SetCorrectly()
    {
        var session = new TrainingSession();
        
        session.ModuleId = "module-01";

        session.ModuleId.Should().Be("module-01");
    }

    [Fact]
    public void StartedAt_DefaultValue_IsAllowed()
    {
        var session = new TrainingSession();
        
        session.StartedAt = default;

        session.StartedAt.Should().Be(default);
    }

    [Fact]
    public void StartedAt_ValidValue_SetCorrectly()
    {
        var session = new TrainingSession();
        var timestamp = DateTimeOffset.UtcNow;
        
        session.StartedAt = timestamp;

        session.StartedAt.Should().Be(timestamp);
    }

    [Fact]
    public void EndedAt_SetWhenIsCompletedFalse_ThrowsInvalidOperationException()
    {
        var session = new TrainingSession
        {
            IsCompleted = false
        };
        
        var act = () => session.EndedAt = DateTimeOffset.UtcNow;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*IsCompleted*");
    }

    [Fact]
    public void EndedAt_SetWhenIsCompletedTrue_SetCorrectly()
    {
        var session = new TrainingSession
        {
            IsCompleted = true
        };
        var timestamp = DateTimeOffset.UtcNow;
        
        session.EndedAt = timestamp;

        session.EndedAt.Should().Be(timestamp);
    }

    [Fact]
    public void EndedAt_SetToNullWhenIsCompletedTrue_SetCorrectly()
    {
        var session = new TrainingSession
        {
            IsCompleted = true,
            EndedAt = DateTimeOffset.UtcNow
        };
        
        session.EndedAt = null;

        session.EndedAt.Should().BeNull();
    }

    [Fact]
    public void IsCompleted_DefaultValue_IsFalse()
    {
        var session = new TrainingSession();

        session.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void IsCompleted_SetToTrue_SetCorrectly()
    {
        var session = new TrainingSession();
        
        session.IsCompleted = true;

        session.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void IsCompleted_SetToFalse_ResetsEndedAt()
    {
        var session = new TrainingSession
        {
            IsCompleted = true,
            EndedAt = DateTimeOffset.UtcNow
        };
        
        session.IsCompleted = false;

        session.EndedAt.Should().BeNull();
    }

    [Fact]
    public void Inputs_DefaultValue_IsNotNull()
    {
        var session = new TrainingSession();

        session.Inputs.Should().NotBeNull();
    }

    [Fact]
    public void Inputs_DefaultValue_IsEmpty()
    {
        var session = new TrainingSession();

        session.Inputs.Should().BeEmpty();
    }

    [Fact]
    public void Inputs_SetToNull_ThrowsArgumentNullException()
    {
        var session = new TrainingSession();
        
        var act = () => session.Inputs = null!;

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*Inputs*");
    }

    [Fact]
    public void Inputs_SetToValidList_SetCorrectly()
    {
        var session = new TrainingSession();
        var inputs = new List<StoredInput>
        {
            new StoredInput
            {
                Zeitpunkt = DateTimeOffset.UtcNow,
                Taste = Core.KeyId.A,
                Umschalter = Core.ModifierSet.None,
                Art = StoredInputKind.Zeichen,
                ErzeugtesGraphem = "a"
            }
        };
        
        session.Inputs = inputs;

        session.Inputs.Should().HaveCount(1);
    }

    [Fact]
    public void Evaluations_DefaultValue_IsNotNull()
    {
        var session = new TrainingSession();

        session.Evaluations.Should().NotBeNull();
    }

    [Fact]
    public void Evaluations_DefaultValue_IsEmpty()
    {
        var session = new TrainingSession();

        session.Evaluations.Should().BeEmpty();
    }

    [Fact]
    public void Evaluations_SetToNull_ThrowsArgumentNullException()
    {
        var session = new TrainingSession();
        
        var act = () => session.Evaluations = null!;

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*Evaluations*");
    }

    [Fact]
    public void Evaluations_SetToValidList_SetCorrectly()
    {
        var session = new TrainingSession();
        var evaluations = new List<StoredEvaluation>
        {
            new StoredEvaluation
            {
                TokenIndex = 0,
                Erwartet = "a",
                Tatsaechlich = "a",
                Ergebnis = Core.EvaluationOutcome.Richtig
            }
        };
        
        session.Evaluations = evaluations;

        session.Evaluations.Should().HaveCount(1);
    }

    [Fact]
    public void Id_InheritsFromEntityBase()
    {
        var session = new TrainingSession();

        session.Id.Should().Be(0);
    }

    [Fact]
    public void Id_CanBeSet()
    {
        var session = new TrainingSession
        {
            Id = 42
        };

        session.Id.Should().Be(42);
    }
}
