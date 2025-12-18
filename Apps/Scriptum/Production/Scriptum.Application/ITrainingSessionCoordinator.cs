using Scriptum.Core;
using Scriptum.Engine;
using Scriptum.Progress;

namespace Scriptum.Application;

/// <summary>
/// Schnittstelle für die Sitzungssteuerung im Scriptum-Trainingssystem.
/// </summary>
/// <remarks>
/// <para>
/// Koordiniert die Zusammenarbeit zwischen:
/// </para>
/// <list type="bullet">
/// <item><b>Content</b>: Lektionen und Module</item>
/// <item><b>Engine</b>: Training und Bewertung</item>
/// <item><b>Progress</b>: Sitzungsverlauf und Statistik</item>
/// <item><b>Persistence</b>: Speicherung via DataToolKit</item>
/// </list>
/// <para>
/// Dieser Dienst ist UI-frei und persistenzfähig.
/// </para>
/// </remarks>
public interface ITrainingSessionCoordinator
{
    /// <summary>
    /// Die aktuelle Trainingssitzung (null, wenn keine aktiv).
    /// </summary>
    TrainingSession? CurrentSession { get; }
    
    /// <summary>
    /// Der aktuelle Trainingszustand (null, wenn keine Sitzung aktiv).
    /// </summary>
    TrainingState? CurrentState { get; }
    
    /// <summary>
    /// Gibt an, ob aktuell eine Sitzung läuft.
    /// </summary>
    bool IsSessionRunning { get; }
    
    /// <summary>
    /// Startet eine neue Trainingssitzung.
    /// </summary>
    /// <param name="moduleId">Die ID des Moduls.</param>
    /// <param name="lessonId">Die ID der Lektion.</param>
    /// <exception cref="ArgumentException">
    /// Wird ausgelöst, wenn moduleId oder lessonId leer sind.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Wird ausgelöst, wenn bereits eine Sitzung läuft oder wenn Lektion/Modul nicht gefunden wurden.
    /// </exception>
    void StartSession(string moduleId, string lessonId);
    
    /// <summary>
    /// Verarbeitet eine Benutzereingabe.
    /// </summary>
    /// <param name="chord">Die gedrückte Tastenkombination.</param>
    /// <returns>
    /// Das Bewertungsergebnis oder null, wenn die Eingabe nicht relevant war
    /// (z.B. ignorierte Taste, Backspace ohne Fehler).
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Wird ausgelöst, wenn keine Sitzung aktiv ist.
    /// </exception>
    EvaluationEvent? ProcessInput(KeyChord chord);
}
