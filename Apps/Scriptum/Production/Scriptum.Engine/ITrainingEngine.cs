using Scriptum.Core;

namespace Scriptum.Engine;

/// <summary>
/// Die Trainingsmaschine: Deterministisch, UI-frei, zustandsbasiert.
/// </summary>
/// <remarks>
/// Die Engine speichert nichts selbst. Sie erzeugt Startzustände und wendet Eingaben an.
/// </remarks>
public interface ITrainingEngine
{
    /// <summary>
    /// Erstellt einen neuen Startzustand für eine Übung.
    /// </summary>
    /// <param name="sequence">Die Zielsequenz.</param>
    /// <param name="startTime">Der Startzeitpunkt.</param>
    /// <returns>Ein neuer Trainingszustand.</returns>
    TrainingState CreateInitialState(TargetSequence sequence, DateTime startTime);
    
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
    (TrainingState NewState, EvaluationEvent? Evaluation) ProcessInput(
        TrainingState currentState, 
        InputEvent inputEvent);
}
