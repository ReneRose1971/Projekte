using Scriptum.Core;

namespace Scriptum.Engine;

/// <summary>
/// Interpretiert Tastenkombinationen und erzeugt Eingabeereignisse.
/// </summary>
public interface IInputInterpreter
{
    /// <summary>
    /// Interpretiert eine Tastenkombination und erzeugt ein Eingabeereignis.
    /// </summary>
    /// <param name="chord">Die Tastenkombination.</param>
    /// <param name="timestamp">Der Zeitpunkt der Eingabe.</param>
    /// <returns>Ein Eingabeereignis.</returns>
    InputEvent Interpret(KeyChord chord, DateTime timestamp);
}
