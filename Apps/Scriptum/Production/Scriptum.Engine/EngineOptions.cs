namespace Scriptum.Engine;

/// <summary>
/// Optionen für die Trainingsmaschine.
/// </summary>
public sealed class EngineOptions
{
    /// <summary>
    /// Gibt an, ob Fehler korrigiert werden müssen, bevor fortgefahren werden kann.
    /// </summary>
    /// <remarks>
    /// Standard: true (Fehler müssen korrigiert werden).
    /// </remarks>
    public bool MustCorrectErrors { get; init; } = true;
    
    /// <summary>
    /// Gibt an, ob Zeilenumbrüche als Zielzeichen behandelt werden.
    /// </summary>
    /// <remarks>
    /// Standard: true (Zeilenumbrüche sind vollwertige Zielzeichen).
    /// </remarks>
    public bool LineBreaksAreTargets { get; init; } = true;
}
