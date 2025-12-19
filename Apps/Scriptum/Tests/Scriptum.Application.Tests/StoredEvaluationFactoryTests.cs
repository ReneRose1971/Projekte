using FluentAssertions;
using Scriptum.Application.Factories;
using Scriptum.Core;
using Scriptum.Engine;
using Scriptum.Progress;
using Xunit;

namespace Scriptum.Application.Tests;

public sealed class StoredEvaluationFactoryTests
{
    [Fact]
    public void FromEvaluationEvent_ShouldCreateStoredEvaluationWithCorrectTokenIndex()
    {
        var evaluation = new EvaluationEvent(5, "a", "a", EvaluationOutcome.Richtig);

        var storedEvaluation = StoredEvaluationFactory.FromEvaluationEvent(evaluation);

        storedEvaluation.TokenIndex.Should().Be(5);
    }

    [Fact]
    public void FromEvaluationEvent_ShouldCreateStoredEvaluationWithCorrectErwartet()
    {
        var evaluation = new EvaluationEvent(0, "a", "a", EvaluationOutcome.Richtig);

        var storedEvaluation = StoredEvaluationFactory.FromEvaluationEvent(evaluation);

        storedEvaluation.Erwartet.Should().Be("a");
    }

    [Fact]
    public void FromEvaluationEvent_ShouldCreateStoredEvaluationWithCorrectTatsaechlich()
    {
        var evaluation = new EvaluationEvent(0, "a", "b", EvaluationOutcome.Falsch);

        var storedEvaluation = StoredEvaluationFactory.FromEvaluationEvent(evaluation);

        storedEvaluation.Tatsaechlich.Should().Be("b");
    }

    [Fact]
    public void FromEvaluationEvent_WithRichtig_ShouldStoreRichtig()
    {
        var evaluation = new EvaluationEvent(0, "a", "a", EvaluationOutcome.Richtig);

        var storedEvaluation = StoredEvaluationFactory.FromEvaluationEvent(evaluation);

        storedEvaluation.Ergebnis.Should().Be(EvaluationOutcome.Richtig);
    }

    [Fact]
    public void FromEvaluationEvent_WithFalsch_ShouldStoreFalsch()
    {
        var evaluation = new EvaluationEvent(0, "a", "b", EvaluationOutcome.Falsch);

        var storedEvaluation = StoredEvaluationFactory.FromEvaluationEvent(evaluation);

        storedEvaluation.Ergebnis.Should().Be(EvaluationOutcome.Falsch);
    }

    [Fact]
    public void FromEvaluationEvent_WithKorrigiert_ShouldStoreKorrigiert()
    {
        var evaluation = new EvaluationEvent(0, "a", "", EvaluationOutcome.Korrigiert);

        var storedEvaluation = StoredEvaluationFactory.FromEvaluationEvent(evaluation);

        storedEvaluation.Ergebnis.Should().Be(EvaluationOutcome.Korrigiert);
    }

    [Fact]
    public void FromEvaluationEvent_WithEmptyTatsaechlich_ShouldStoreEmptyString()
    {
        var evaluation = new EvaluationEvent(0, "a", "", EvaluationOutcome.Korrigiert);

        var storedEvaluation = StoredEvaluationFactory.FromEvaluationEvent(evaluation);

        storedEvaluation.Tatsaechlich.Should().BeEmpty();
    }

    [Fact]
    public void FromEvaluationEvent_WithNullEvaluation_ShouldThrowArgumentNullException()
    {
        var act = () => StoredEvaluationFactory.FromEvaluationEvent(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("evaluation");
    }
}
