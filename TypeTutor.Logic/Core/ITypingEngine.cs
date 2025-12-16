using TypeTutor.Logic.Core;
using System;

namespace TypeTutor.Logic.Core;

/// <summary>
/// Definiert die öffentliche API der TypingEngine.
/// Die Engine verarbeitet KeyStrokes, hält einen internen Input-Puffer
/// und berechnet einen TypingEngineState.
/// 
/// Wichtig:
///  - Die Engine enthält Logik zur Bewertung (korrekt / falsch / fertig).
///  - Die Engine ist zustandsbehaftet.
///  - Die UI konsumiert nur den State.
/// </summary>
public interface ITypingEngine
{
    /// <summary>
    /// Der aktuelle abgeleitete Zustand der Tipp-Session (immutable Snapshot).
    /// </summary>
    TypingEngineState State { get; }

    /// <summary>
    /// Verarbeitet einen neuen Tastenanschlag und aktualisiert den State.
    /// </summary>
    void Process(KeyStroke stroke);

    /// <summary>
    /// Setzt die Engine für eine neue Lesson zurück.
    /// </summary>
    void Reset(string targetText);

    /// <summary>
    /// Wird ausgelöst, wenn eine Lesson einmalig als abgeschlossen gilt.
    /// Der Parameter signalisiert, ob die Lektion erfolgreich (true) oder mit Fehlern (false) abgeschlossen wurde.
    /// </summary>
    event Action<bool>? LessonCompleted;
}
