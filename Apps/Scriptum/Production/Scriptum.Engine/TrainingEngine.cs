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
            return (IncrementGesamtEingaben(currentState), null);
        }
        
        var expectedSymbol = currentState.Sequence.Symbols[currentState.CurrentTargetIndex];
        var actualGraphem = inputEvent.Graphem ?? string.Empty;
        var isCorrect = expectedSymbol.Graphem == actualGraphem;
        
        if (isCorrect)
        {
            var newIndex = currentState.CurrentTargetIndex + 1;
            var isCompleted = newIndex >= currentState.Sequence.Length;
            
            var newState = new TrainingState(
                sequence: currentState.Sequence,
                currentTargetIndex: newIndex,
                startTime: currentState.StartTime,
                endTime: isCompleted ? inputEvent.Timestamp : null,
                istFehlerAktiv: false,
                fehlerPosition: currentState.FehlerPosition,
                gesamtEingaben: currentState.GesamtEingaben + 1,
                fehler: currentState.Fehler,
                korrekturen: currentState.Korrekturen,
                ruecktasten: currentState.Ruecktasten);
            
            var evaluation = new EvaluationEvent(
                targetIndex: currentState.CurrentTargetIndex,
                expectedGraphem: expectedSymbol.Graphem,
                actualGraphem: actualGraphem,
                outcome: EvaluationOutcome.Richtig);
            
            return (newState, evaluation);
        }
        else
        {
            var newState = new TrainingState(
                sequence: currentState.Sequence,
                currentTargetIndex: currentState.CurrentTargetIndex,
                startTime: currentState.StartTime,
                endTime: null,
                istFehlerAktiv: true,
                fehlerPosition: currentState.CurrentTargetIndex,
                gesamtEingaben: currentState.GesamtEingaben + 1,
                fehler: currentState.Fehler + 1,
                korrekturen: currentState.Korrekturen,
                ruecktasten: currentState.Ruecktasten);
            
            var evaluation = new EvaluationEvent(
                targetIndex: currentState.CurrentTargetIndex,
                expectedGraphem: expectedSymbol.Graphem,
                actualGraphem: actualGraphem,
                outcome: EvaluationOutcome.Falsch);
            
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
            
            var newState = new TrainingState(
                sequence: currentState.Sequence,
                currentTargetIndex: currentState.CurrentTargetIndex,
                startTime: currentState.StartTime,
                endTime: null,
                istFehlerAktiv: false,
                fehlerPosition: currentState.FehlerPosition,
                gesamtEingaben: currentState.GesamtEingaben + 1,
                fehler: currentState.Fehler,
                korrekturen: currentState.Korrekturen + 1,
                ruecktasten: currentState.Ruecktasten + 1);
            
            var evaluation = new EvaluationEvent(
                targetIndex: currentState.FehlerPosition,
                expectedGraphem: expectedSymbol.Graphem,
                actualGraphem: string.Empty,
                outcome: EvaluationOutcome.Korrigiert);
            
            return (newState, evaluation);
        }
        else
        {
            var newState = new TrainingState(
                sequence: currentState.Sequence,
                currentTargetIndex: currentState.CurrentTargetIndex,
                startTime: currentState.StartTime,
                endTime: null,
                istFehlerAktiv: false,
                fehlerPosition: currentState.FehlerPosition,
                gesamtEingaben: currentState.GesamtEingaben + 1,
                fehler: currentState.Fehler,
                korrekturen: currentState.Korrekturen,
                ruecktasten: currentState.Ruecktasten + 1);
            
            return (newState, null);
        }
    }
    
    private static (TrainingState, EvaluationEvent?) ProcessIgnoredInput(TrainingState currentState)
    {
        return (IncrementGesamtEingaben(currentState), null);
    }
    
    private static TrainingState IncrementGesamtEingaben(TrainingState currentState)
    {
        return new TrainingState(
            sequence: currentState.Sequence,
            currentTargetIndex: currentState.CurrentTargetIndex,
            startTime: currentState.StartTime,
            endTime: currentState.EndTime,
            istFehlerAktiv: currentState.IstFehlerAktiv,
            fehlerPosition: currentState.FehlerPosition,
            gesamtEingaben: currentState.GesamtEingaben + 1,
            fehler: currentState.Fehler,
            korrekturen: currentState.Korrekturen,
            ruecktasten: currentState.Ruecktasten);
    }
}
