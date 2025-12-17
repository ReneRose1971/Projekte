namespace Scriptum.Core;

/// <summary>
/// Kombination aus Taste und Umschalttasten.
/// </summary>
/// <remarks>
/// Dies ist die roheste fachliche Beschreibung einer Eingabe.
/// </remarks>
public readonly record struct KeyChord(KeyId Key, ModifierSet Modifiers)
{
    /// <summary>
    /// Erstellt eine neue Tastenkombination.
    /// </summary>
    /// <param name="Key">Die Taste.</param>
    /// <param name="Modifiers">Die Umschalttasten.</param>
    public KeyChord(KeyId Key) : this(Key, ModifierSet.None)
    {
    }
}
