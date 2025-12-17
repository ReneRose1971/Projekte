using FluentAssertions;
using Scriptum.Core;
using Scriptum.Progress;
using Xunit;

namespace Scriptum.Progress.Tests;

/// <summary>
/// Tests für <see cref="StoredEvaluation"/>.
/// </summary>
public sealed class StoredEvaluationTests
{
    [Fact]
    public void Constructor_WithValidValues_CreatesInstance()
    {
        var evaluation = new StoredEvaluation
        {
            TokenIndex = 0,
            Erwartet = "a",
            Tatsaechlich = "a",
            Ergebnis = EvaluationOutcome.Richtig
        };

        evaluation.TokenIndex.Should().Be(0);
    }

    [Fact]
    public void TokenIndex_NegativeValue_ThrowsArgumentException()
    {
        var act = () => new StoredEvaluation
        {
            TokenIndex = -1,
            Erwartet = "a",
            Tatsaechlich = "a",
            Ergebnis = EvaluationOutcome.Richtig
        };

        act.Should().Throw<ArgumentException>()
            .WithMessage("*TokenIndex*");
    }

    [Fact]
    public void TokenIndex_Zero_IsAllowed()
    {
        var evaluation = new StoredEvaluation
        {
            TokenIndex = 0,
            Erwartet = "a",
            Tatsaechlich = "a",
            Ergebnis = EvaluationOutcome.Richtig
        };

        evaluation.TokenIndex.Should().Be(0);
    }

    [Fact]
    public void TokenIndex_PositiveValue_IsAllowed()
    {
        var evaluation = new StoredEvaluation
        {
            TokenIndex = 42,
            Erwartet = "a",
            Tatsaechlich = "a",
            Ergebnis = EvaluationOutcome.Richtig
        };

        evaluation.TokenIndex.Should().Be(42);
    }

    [Fact]
    public void Erwartet_Null_ThrowsArgumentException()
    {
        var act = () => new StoredEvaluation
        {
            TokenIndex = 0,
            Erwartet = null!,
            Tatsaechlich = "a",
            Ergebnis = EvaluationOutcome.Richtig
        };

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Erwartet*");
    }

    [Fact]
    public void Erwartet_EmptyString_ThrowsArgumentException()
    {
        var act = () => new StoredEvaluation
        {
            TokenIndex = 0,
            Erwartet = string.Empty,
            Tatsaechlich = "a",
            Ergebnis = EvaluationOutcome.Richtig
        };

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Erwartet*");
    }

    [Fact]
    public void Tatsaechlich_Null_ThrowsArgumentException()
    {
        var act = () => new StoredEvaluation
        {
            TokenIndex = 0,
            Erwartet = "a",
            Tatsaechlich = null!,
            Ergebnis = EvaluationOutcome.Richtig
        };

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Tatsaechlich*");
    }

    [Fact]
    public void Tatsaechlich_EmptyString_IsAllowed()
    {
        var evaluation = new StoredEvaluation
        {
            TokenIndex = 0,
            Erwartet = "a",
            Tatsaechlich = string.Empty,
            Ergebnis = EvaluationOutcome.Falsch
        };

        evaluation.Tatsaechlich.Should().BeEmpty();
    }

    [Fact]
    public void Ergebnis_SetCorrectly()
    {
        var evaluation = new StoredEvaluation
        {
            TokenIndex = 0,
            Erwartet = "a",
            Tatsaechlich = "b",
            Ergebnis = EvaluationOutcome.Falsch
        };

        evaluation.Ergebnis.Should().Be(EvaluationOutcome.Falsch);
    }

    [Fact]
    public void Record_Equality_WithSameValues_AreEqual()
    {
        var eval1 = new StoredEvaluation
        {
            TokenIndex = 5,
            Erwartet = "test",
            Tatsaechlich = "test",
            Ergebnis = EvaluationOutcome.Richtig
        };
        
        var eval2 = new StoredEvaluation
        {
            TokenIndex = 5,
            Erwartet = "test",
            Tatsaechlich = "test",
            Ergebnis = EvaluationOutcome.Richtig
        };

        eval1.Should().Be(eval2);
    }

    [Fact]
    public void Record_Equality_WithDifferentValues_AreNotEqual()
    {
        var eval1 = new StoredEvaluation
        {
            TokenIndex = 5,
            Erwartet = "test",
            Tatsaechlich = "test",
            Ergebnis = EvaluationOutcome.Richtig
        };
        
        var eval2 = new StoredEvaluation
        {
            TokenIndex = 6,
            Erwartet = "test",
            Tatsaechlich = "test",
            Ergebnis = EvaluationOutcome.Richtig
        };

        eval1.Should().NotBe(eval2);
    }

    [Fact]
    public void Ergebnis_Korrigiert_IsAllowed()
    {
        var evaluation = new StoredEvaluation
        {
            TokenIndex = 0,
            Erwartet = "a",
            Tatsaechlich = "",
            Ergebnis = EvaluationOutcome.Korrigiert
        };

        evaluation.Ergebnis.Should().Be(EvaluationOutcome.Korrigiert);
    }
}
