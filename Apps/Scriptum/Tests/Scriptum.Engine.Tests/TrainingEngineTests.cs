using FluentAssertions;
using Scriptum.Core;
using Scriptum.Engine;
using Xunit;

namespace Scriptum.Engine.Tests;

public class TrainingEngineTests
{
    private readonly TrainingEngine _engine = new();
    
    private static TargetSequence CreateSequence(params string[] graphemes)
    {
        return new TargetSequence(graphemes);
    }
    
    private static InputEvent CreateCharacterInput(DateTime timestamp, string graphem)
    {
        var chord = new KeyChord(KeyId.A, ModifierSet.None);
        return new InputEvent(timestamp, chord, InputEventKind.Zeichen, graphem);
    }
    
    private static InputEvent CreateBackspaceInput(DateTime timestamp)
    {
        var chord = new KeyChord(KeyId.Backspace, ModifierSet.None);
        return new InputEvent(timestamp, chord, InputEventKind.Ruecktaste);
    }
    
    private static InputEvent CreateIgnoredInput(DateTime timestamp)
    {
        var chord = new KeyChord(KeyId.Escape, ModifierSet.None);
        return new InputEvent(timestamp, chord, InputEventKind.Ignoriert);
    }
    
    [Fact]
    public void CreateInitialState_ShouldReturnStateWithCorrectStartValues()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        
        var state = _engine.CreateInitialState(sequence, startTime);
        
        state.Sequence.Should().BeSameAs(sequence);
    }
    
    [Fact]
    public void CreateInitialState_ShouldSetCurrentTargetIndexToZero()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        
        var state = _engine.CreateInitialState(sequence, startTime);
        
        state.CurrentTargetIndex.Should().Be(0);
    }
    
    [Fact]
    public void CreateInitialState_ShouldSetStartTime()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        
        var state = _engine.CreateInitialState(sequence, startTime);
        
        state.StartTime.Should().Be(startTime);
    }
    
    [Fact]
    public void CreateInitialState_ShouldNotSetEndTime()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        
        var state = _engine.CreateInitialState(sequence, startTime);
        
        state.EndTime.Should().BeNull();
    }
    
    [Fact]
    public void CreateInitialState_ShouldNotSetErrorStatus()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        
        var state = _engine.CreateInitialState(sequence, startTime);
        
        state.IstFehlerAktiv.Should().BeFalse();
    }
    
    [Fact]
    public void CreateInitialState_ShouldInitializeCountersToZero()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        
        var state = _engine.CreateInitialState(sequence, startTime);
        
        state.GesamtEingaben.Should().Be(0);
    }
    
    [Fact]
    public void CreateInitialState_WithNullSequence_ShouldThrowArgumentNullException()
    {
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        
        var act = () => _engine.CreateInitialState(null!, startTime);
        
        act.Should().Throw<ArgumentNullException>().WithParameterName("sequence");
    }
    
    [Fact]
    public void ProcessInput_WithCorrectCharacter_ShouldIncreasePosition()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        var inputTime = startTime.AddSeconds(1);
        var input = CreateCharacterInput(inputTime, "a");
        
        var (newState, _) = _engine.ProcessInput(state, input);
        
        newState.CurrentTargetIndex.Should().Be(1);
    }
    
    [Fact]
    public void ProcessInput_WithCorrectCharacter_ShouldReturnRichtigEvaluation()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        var inputTime = startTime.AddSeconds(1);
        var input = CreateCharacterInput(inputTime, "a");
        
        var (_, evaluation) = _engine.ProcessInput(state, input);
        
        evaluation!.Outcome.Should().Be(EvaluationOutcome.Richtig);
    }
    
    [Fact]
    public void ProcessInput_WithCorrectCharacter_ShouldIncrementGesamtEingaben()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        var inputTime = startTime.AddSeconds(1);
        var input = CreateCharacterInput(inputTime, "a");
        
        var (newState, _) = _engine.ProcessInput(state, input);
        
        newState.GesamtEingaben.Should().Be(1);
    }
    
    [Fact]
    public void ProcessInput_WithCorrectCharacter_ShouldNotIncrementFehler()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        var inputTime = startTime.AddSeconds(1);
        var input = CreateCharacterInput(inputTime, "a");
        
        var (newState, _) = _engine.ProcessInput(state, input);
        
        newState.Fehler.Should().Be(0);
    }
    
    [Fact]
    public void ProcessInput_WithIncorrectCharacter_ShouldSetErrorStatus()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        var inputTime = startTime.AddSeconds(1);
        var input = CreateCharacterInput(inputTime, "x");
        
        var (newState, _) = _engine.ProcessInput(state, input);
        
        newState.IstFehlerAktiv.Should().BeTrue();
    }
    
    [Fact]
    public void ProcessInput_WithIncorrectCharacter_ShouldNotChangePosition()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        var inputTime = startTime.AddSeconds(1);
        var input = CreateCharacterInput(inputTime, "x");
        
        var (newState, _) = _engine.ProcessInput(state, input);
        
        newState.CurrentTargetIndex.Should().Be(0);
    }
    
    [Fact]
    public void ProcessInput_WithIncorrectCharacter_ShouldReturnFalschEvaluation()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        var inputTime = startTime.AddSeconds(1);
        var input = CreateCharacterInput(inputTime, "x");
        
        var (_, evaluation) = _engine.ProcessInput(state, input);
        
        evaluation!.Outcome.Should().Be(EvaluationOutcome.Falsch);
    }
    
    [Fact]
    public void ProcessInput_WithIncorrectCharacter_ShouldIncrementFehler()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        var inputTime = startTime.AddSeconds(1);
        var input = CreateCharacterInput(inputTime, "x");
        
        var (newState, _) = _engine.ProcessInput(state, input);
        
        newState.Fehler.Should().Be(1);
    }
    
    [Fact]
    public void ProcessInput_WithBackspaceAfterError_ShouldClearErrorStatus()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        
        var errorInput = CreateCharacterInput(startTime.AddSeconds(1), "x");
        var (stateAfterError, _) = _engine.ProcessInput(state, errorInput);
        
        var backspaceInput = CreateBackspaceInput(startTime.AddSeconds(2));
        var (newState, _) = _engine.ProcessInput(stateAfterError, backspaceInput);
        
        newState.IstFehlerAktiv.Should().BeFalse();
    }
    
    [Fact]
    public void ProcessInput_WithBackspaceAfterError_ShouldReturnKorrigiertEvaluation()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        
        var errorInput = CreateCharacterInput(startTime.AddSeconds(1), "x");
        var (stateAfterError, _) = _engine.ProcessInput(state, errorInput);
        
        var backspaceInput = CreateBackspaceInput(startTime.AddSeconds(2));
        var (_, evaluation) = _engine.ProcessInput(stateAfterError, backspaceInput);
        
        evaluation!.Outcome.Should().Be(EvaluationOutcome.Korrigiert);
    }
    
    [Fact]
    public void ProcessInput_WithBackspaceAfterError_ShouldNotChangePosition()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        
        var errorInput = CreateCharacterInput(startTime.AddSeconds(1), "x");
        var (stateAfterError, _) = _engine.ProcessInput(state, errorInput);
        
        var backspaceInput = CreateBackspaceInput(startTime.AddSeconds(2));
        var (newState, _) = _engine.ProcessInput(stateAfterError, backspaceInput);
        
        newState.CurrentTargetIndex.Should().Be(0);
    }
    
    [Fact]
    public void ProcessInput_WithBackspaceAfterError_ShouldIncrementKorrekturen()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        
        var errorInput = CreateCharacterInput(startTime.AddSeconds(1), "x");
        var (stateAfterError, _) = _engine.ProcessInput(state, errorInput);
        
        var backspaceInput = CreateBackspaceInput(startTime.AddSeconds(2));
        var (newState, _) = _engine.ProcessInput(stateAfterError, backspaceInput);
        
        newState.Korrekturen.Should().Be(1);
    }
    
    [Fact]
    public void ProcessInput_WithBackspaceWithoutError_ShouldNotReturnEvaluation()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        var backspaceInput = CreateBackspaceInput(startTime.AddSeconds(1));
        
        var (_, evaluation) = _engine.ProcessInput(state, backspaceInput);
        
        evaluation.Should().BeNull();
    }
    
    [Fact]
    public void ProcessInput_WithBackspaceWithoutError_ShouldNotChangePosition()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        var backspaceInput = CreateBackspaceInput(startTime.AddSeconds(1));
        
        var (newState, _) = _engine.ProcessInput(state, backspaceInput);
        
        newState.CurrentTargetIndex.Should().Be(0);
    }
    
    [Fact]
    public void ProcessInput_WithBackspaceWithoutError_ShouldIncrementRuecktasten()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        var backspaceInput = CreateBackspaceInput(startTime.AddSeconds(1));
        
        var (newState, _) = _engine.ProcessInput(state, backspaceInput);
        
        newState.Ruecktasten.Should().Be(1);
    }
    
    [Fact]
    public void ProcessInput_WithEnterAsTargetCharacter_ShouldBeEvaluatedCorrectly()
    {
        var sequence = CreateSequence("a", "\n", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        
        var firstInput = CreateCharacterInput(startTime.AddSeconds(1), "a");
        var (stateAfterFirst, _) = _engine.ProcessInput(state, firstInput);
        
        var enterInput = CreateCharacterInput(startTime.AddSeconds(2), "\n");
        var (newState, evaluation) = _engine.ProcessInput(stateAfterFirst, enterInput);
        
        evaluation!.Outcome.Should().Be(EvaluationOutcome.Richtig);
    }
    
    [Fact]
    public void ProcessInput_WithEnterAsTargetCharacter_ShouldAdvancePosition()
    {
        var sequence = CreateSequence("a", "\n", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        
        var firstInput = CreateCharacterInput(startTime.AddSeconds(1), "a");
        var (stateAfterFirst, _) = _engine.ProcessInput(state, firstInput);
        
        var enterInput = CreateCharacterInput(startTime.AddSeconds(2), "\n");
        var (newState, _) = _engine.ProcessInput(stateAfterFirst, enterInput);
        
        newState.CurrentTargetIndex.Should().Be(2);
    }
    
    [Fact]
    public void ProcessInput_WhenCompletingLastCharacter_ShouldSetEndTime()
    {
        var sequence = CreateSequence("a", "b");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        
        var firstInput = CreateCharacterInput(startTime.AddSeconds(1), "a");
        var (stateAfterFirst, _) = _engine.ProcessInput(state, firstInput);
        
        var lastInputTime = startTime.AddSeconds(2);
        var lastInput = CreateCharacterInput(lastInputTime, "b");
        var (newState, _) = _engine.ProcessInput(stateAfterFirst, lastInput);
        
        newState.EndTime.Should().Be(lastInputTime);
    }
    
    [Fact]
    public void ProcessInput_WhenCompletingLastCharacter_ShouldSetIstAbgeschlossenToTrue()
    {
        var sequence = CreateSequence("a", "b");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        
        var firstInput = CreateCharacterInput(startTime.AddSeconds(1), "a");
        var (stateAfterFirst, _) = _engine.ProcessInput(state, firstInput);
        
        var lastInput = CreateCharacterInput(startTime.AddSeconds(2), "b");
        var (newState, _) = _engine.ProcessInput(stateAfterFirst, lastInput);
        
        newState.IstAbgeschlossen.Should().BeTrue();
    }
    
    [Fact]
    public void ProcessInput_WhenAlreadyCompleted_ShouldReturnSameState()
    {
        var sequence = CreateSequence("a");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        
        var input = CreateCharacterInput(startTime.AddSeconds(1), "a");
        var (completedState, _) = _engine.ProcessInput(state, input);
        
        var additionalInput = CreateCharacterInput(startTime.AddSeconds(2), "x");
        var (newState, _) = _engine.ProcessInput(completedState, additionalInput);
        
        newState.Should().BeSameAs(completedState);
    }
    
    [Fact]
    public void ProcessInput_WhenAlreadyCompleted_ShouldNotReturnEvaluation()
    {
        var sequence = CreateSequence("a");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        
        var input = CreateCharacterInput(startTime.AddSeconds(1), "a");
        var (completedState, _) = _engine.ProcessInput(state, input);
        
        var additionalInput = CreateCharacterInput(startTime.AddSeconds(2), "x");
        var (_, evaluation) = _engine.ProcessInput(completedState, additionalInput);
        
        evaluation.Should().BeNull();
    }
    
    [Fact]
    public void ProcessInput_WithIgnoredInput_ShouldNotReturnEvaluation()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        var ignoredInput = CreateIgnoredInput(startTime.AddSeconds(1));
        
        var (_, evaluation) = _engine.ProcessInput(state, ignoredInput);
        
        evaluation.Should().BeNull();
    }
    
    [Fact]
    public void ProcessInput_WithIgnoredInput_ShouldIncrementGesamtEingaben()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        var ignoredInput = CreateIgnoredInput(startTime.AddSeconds(1));
        
        var (newState, _) = _engine.ProcessInput(state, ignoredInput);
        
        newState.GesamtEingaben.Should().Be(1);
    }
    
    [Fact]
    public void ProcessInput_WithCharacterWhileErrorActive_ShouldNotReturnEvaluation()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        
        var errorInput = CreateCharacterInput(startTime.AddSeconds(1), "x");
        var (stateAfterError, _) = _engine.ProcessInput(state, errorInput);
        
        var additionalInput = CreateCharacterInput(startTime.AddSeconds(2), "y");
        var (_, evaluation) = _engine.ProcessInput(stateAfterError, additionalInput);
        
        evaluation.Should().BeNull();
    }
    
    [Fact]
    public void ProcessInput_WithCharacterWhileErrorActive_ShouldNotChangeErrorStatus()
    {
        var sequence = CreateSequence("a", "b", "c");
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var state = _engine.CreateInitialState(sequence, startTime);
        
        var errorInput = CreateCharacterInput(startTime.AddSeconds(1), "x");
        var (stateAfterError, _) = _engine.ProcessInput(state, errorInput);
        
        var additionalInput = CreateCharacterInput(startTime.AddSeconds(2), "y");
        var (newState, _) = _engine.ProcessInput(stateAfterError, additionalInput);
        
        newState.IstFehlerAktiv.Should().BeTrue();
    }
    
    [Fact]
    public void ProcessInput_WithNullState_ShouldThrowArgumentNullException()
    {
        var input = CreateCharacterInput(DateTime.Now, "a");
        
        var act = () => _engine.ProcessInput(null!, input);
        
        act.Should().Throw<ArgumentNullException>().WithParameterName("currentState");
    }
    
    [Fact]
    public void ProcessInput_WithNullInput_ShouldThrowArgumentNullException()
    {
        var sequence = CreateSequence("a", "b", "c");
        var state = _engine.CreateInitialState(sequence, DateTime.Now);
        
        var act = () => _engine.ProcessInput(state, null!);
        
        act.Should().Throw<ArgumentNullException>().WithParameterName("inputEvent");
    }
}
