namespace Scriptum.Core;

/// <summary>
/// Bewertungsergebnis einer Eingabe.
/// </summary>
public enum EvaluationOutcome
{
    /// <summary>
    /// Die Eingabe war richtig.
    /// </summary>
    Richtig,
    
    /// <summary>
    /// Die Eingabe war falsch.
    /// </summary>
    Falsch,
    
    /// <summary>
    /// Ein Fehler wurde korrigiert (Rücktaste bei aktivem Fehler).
    /// </summary>
    Korrigiert
}
