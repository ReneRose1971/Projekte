using Scriptum.Engine;
using Scriptum.Progress;

namespace Scriptum.Application.Factories;

/// <summary>
/// Factory zur Erstellung von <see cref="StoredEvaluation"/> aus <see cref="EvaluationEvent"/>.
/// </summary>
public static class StoredEvaluationFactory
{
    /// <summary>
    /// Erstellt ein StoredEvaluation aus einem EvaluationEvent.
    /// </summary>
    /// <param name="evaluation">Das EvaluationEvent von der Engine.</param>
    /// <returns>Ein neues StoredEvaluation.</returns>
    /// <exception cref="ArgumentNullException">Wenn evaluation null ist.</exception>
    public static StoredEvaluation FromEvaluationEvent(EvaluationEvent evaluation)
    {
        if (evaluation == null)
            throw new ArgumentNullException(nameof(evaluation));
        
        return new StoredEvaluation
        {
            TokenIndex = evaluation.TargetIndex,
            Erwartet = evaluation.ExpectedGraphem,
            Tatsaechlich = evaluation.ActualGraphem,
            Ergebnis = evaluation.Outcome
        };
    }
}
