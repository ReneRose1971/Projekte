using Scriptum.Core;

namespace Scriptum.Engine;

/// <summary>
/// Implementierung der Trainingsmaschine.
/// </summary>
/// <remarks>
/// Deterministisch, UI-frei, zustandsbasiert. Die Engine speichert nichts selbst.
/// </remarks>
public sealed class TrainingEngine : ITrainingEngine
{
    /// <summary>
    /// Erstellt einen neuen Startzustand für eine Übung.
    /// </summary>
    /// <param name="sequence">Die Zielsequenz.</param>
    /// <param name="startTime">Der Startzeitpunkt.</param>
    /// <returns>Ein neuer Trainingszustand.</returns>
    /// <exception cref="ArgumentNullException">sequence ist null.</exception>
    public TrainingState CreateInitialState(TargetSequence sequence, DateTime startTime)
    {
        if (sequence is null)
            throw new ArgumentNullException(nameof(sequence));
        
        return new TrainingState(
            sequence: sequence,
            currentTargetIndex: 0,
            startTime: startTime);
    }
    
    /// <summary>
    /// Wendet ein Eingabeereignis auf den aktuellen Zustand an.
    /// </summary>
    /// <param name="currentState">Der aktuelle Zustand.</param>
    /// <param name="inputEvent">Das Eingabeereignis.</param>
    /// <returns>
    /// Ein Tuple aus:
    /// - Neuer Trainingszustand
    /// - Optional ein Bewertungsereignis (null, wenn die Eingabe nicht relevant war)
    /// </returns>
    /// <exception cref="ArgumentNullException">currentState oder inputEvent ist null.</exception>
    public (TrainingState NewState, EvaluationEvent? Evaluation) ProcessInput(
        TrainingState currentState,
        InputEvent inputEvent)
    {
        if (currentState is null)
            throw new ArgumentNullException(nameof(currentState));
        
        if (inputEvent is null)
            throw new ArgumentNullException(nameof(inputEvent));
        
        if (currentState.IstAbgeschlossen)
            return (currentState, null);
        
        return inputEvent.Kind switch
        {
            InputEventKind.Zeichen => ProcessCharacterInput(currentState, inputEvent),
            InputEventKind.Ruecktaste => ProcessBackspaceInput(currentState, inputEvent),
            InputEventKind.Ignoriert => ProcessIgnoredInput(currentState),
            _ => throw new ArgumentException($"Unbekannte InputEventKind: {inputEvent.Kind}", nameof(inputEvent))
        };
    }
    
    private static (TrainingState, EvaluationEvent?) ProcessCharacterInput(
        TrainingState currentState,
        InputEvent inputEvent)
    {
        if (currentState.IstFehlerAktiv)
        {
            return (TrainingState.WithIncrementedInput(currentState), null);
        }
        
        var expectedSymbol = currentState.Sequence.Symbols[currentState.CurrentTargetIndex];
        var actualGraphem = inputEvent.Graphem ?? string.Empty;
        var isCorrect = expectedSymbol.Graphem == actualGraphem;
        
        if (isCorrect)
        {
            var newIndex = currentState.CurrentTargetIndex + 1;
            var isCompleted = newIndex >= currentState.Sequence.Length;
            
            var newState = TrainingState.WithCorrectInput(
                currentState,
                newIndex,
                isCompleted ? inputEvent.Timestamp : null);
            
            var evaluation = EvaluationEvent.CreateCorrect(
                currentState.CurrentTargetIndex,
                expectedSymbol.Graphem,
                actualGraphem);
            
            return (newState, evaluation);
        }
        else
        {
            var newState = TrainingState.WithIncorrectInput(currentState);
            
            var evaluation = EvaluationEvent.CreateIncorrect(
                currentState.CurrentTargetIndex,
                expectedSymbol.Graphem,
                actualGraphem);
            
            return (newState, evaluation);
        }
    }
    
    private static (TrainingState, EvaluationEvent?) ProcessBackspaceInput(
        TrainingState currentState,
        InputEvent inputEvent)
    {
        if (currentState.IstFehlerAktiv)
        {
            var expectedSymbol = currentState.Sequence.Symbols[currentState.FehlerPosition];
            
            var newState = TrainingState.WithCorrectionInput(currentState);
            
            var evaluation = EvaluationEvent.CreateCorrected(
                currentState.FehlerPosition,
                expectedSymbol.Graphem);
            
            return (newState, evaluation);
        }
        else
        {
            var newState = TrainingState.WithBackspaceInput(currentState);
            
            return (newState, null);
        }
    }
    
    private static (TrainingState, EvaluationEvent?) ProcessIgnoredInput(TrainingState currentState)
    {
        return (TrainingState.WithIncrementedInput(currentState), null);
    }
}
