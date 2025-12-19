using Scriptum.Core;

namespace Scriptum.Engine;

/// <summary>
/// Beschreibt die Beziehung zwischen einer Eingabe und dem Zielzeichen.
/// </summary>
/// <remarks>
/// EvaluationEvents entstehen nur, wenn eine Eingabe fachlich relevant ist.
/// </remarks>
public sealed class EvaluationEvent
{
    /// <summary>
    /// Die Zielposition.
    /// </summary>
    public int TargetIndex { get; }
    
    /// <summary>
    /// Das erwartete Graphem.
    /// </summary>
    public string ExpectedGraphem { get; }
    
    /// <summary>
    /// Das tatsächlich erzeugte Graphem (kann leer sein bei Rücktaste).
    /// </summary>
    public string ActualGraphem { get; }
    
    /// <summary>
    /// Das Bewertungsergebnis.
    /// </summary>
    public EvaluationOutcome Outcome { get; }
    
    /// <summary>
    /// Erstellt ein neues Bewertungsereignis.
    /// </summary>
    /// <param name="targetIndex">Die Zielposition (muss >= 0 sein).</param>
    /// <param name="expectedGraphem">Das erwartete Graphem (darf nicht null oder leer sein).</param>
    /// <param name="actualGraphem">Das tatsächlich erzeugte Graphem.</param>
    /// <param name="outcome">Das Bewertungsergebnis.</param>
    /// <exception cref="ArgumentOutOfRangeException">targetIndex ist negativ.</exception>
    /// <exception cref="ArgumentException">expectedGraphem ist null oder leer.</exception>
    /// <exception cref="ArgumentNullException">actualGraphem ist null.</exception>
    public EvaluationEvent(int targetIndex, string expectedGraphem, string actualGraphem, EvaluationOutcome outcome)
    {
        if (targetIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(targetIndex), "targetIndex muss >= 0 sein.");
        
        if (string.IsNullOrEmpty(expectedGraphem))
            throw new ArgumentException("expectedGraphem darf nicht null oder leer sein.", nameof(expectedGraphem));
        
        if (actualGraphem is null)
            throw new ArgumentNullException(nameof(actualGraphem));
        
        TargetIndex = targetIndex;
        ExpectedGraphem = expectedGraphem;
        ActualGraphem = actualGraphem;
        Outcome = outcome;
    }
    
    /// <summary>
    /// Erstellt ein Bewertungsereignis für eine korrekte Eingabe.
    /// </summary>
    /// <param name="targetIndex">Die Zielposition.</param>
    /// <param name="expectedGraphem">Das erwartete Graphem.</param>
    /// <param name="actualGraphem">Das tatsächlich eingegebene Graphem.</param>
    /// <returns>Ein neues EvaluationEvent mit Outcome.Richtig.</returns>
    public static EvaluationEvent CreateCorrect(int targetIndex, string expectedGraphem, string actualGraphem)
    {
        return new EvaluationEvent(targetIndex, expectedGraphem, actualGraphem, EvaluationOutcome.Richtig);
    }
    
    /// <summary>
    /// Erstellt ein Bewertungsereignis für eine fehlerhafte Eingabe.
    /// </summary>
    /// <param name="targetIndex">Die Zielposition.</param>
    /// <param name="expectedGraphem">Das erwartete Graphem.</param>
    /// <param name="actualGraphem">Das tatsächlich eingegebene Graphem.</param>
    /// <returns>Ein neues EvaluationEvent mit Outcome.Falsch.</returns>
    public static EvaluationEvent CreateIncorrect(int targetIndex, string expectedGraphem, string actualGraphem)
    {
        return new EvaluationEvent(targetIndex, expectedGraphem, actualGraphem, EvaluationOutcome.Falsch);
    }
    
    /// <summary>
    /// Erstellt ein Bewertungsereignis für eine Korrektur (Rücktaste bei aktivem Fehler).
    /// </summary>
    /// <param name="targetIndex">Die Zielposition.</param>
    /// <param name="expectedGraphem">Das erwartete Graphem.</param>
    /// <returns>Ein neues EvaluationEvent mit Outcome.Korrigiert und leerem ActualGraphem.</returns>
    public static EvaluationEvent CreateCorrected(int targetIndex, string expectedGraphem)
    {
        return new EvaluationEvent(targetIndex, expectedGraphem, string.Empty, EvaluationOutcome.Korrigiert);
    }
}
