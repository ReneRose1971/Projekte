using Scriptum.Core;

namespace Scriptum.Engine;

/// <summary>
/// Interpretiert Tastenkombinationen nach dem deutschen QWERTZ-Layout.
/// </summary>
public sealed class DeQwertzInputInterpreter : IInputInterpreter
{
    /// <summary>
    /// Interpretiert eine Tastenkombination und erzeugt ein Eingabeereignis.
    /// </summary>
    /// <param name="chord">Die Tastenkombination.</param>
    /// <param name="timestamp">Der Zeitpunkt der Eingabe.</param>
    /// <returns>Ein Eingabeereignis.</returns>
    public InputEvent Interpret(KeyChord chord, DateTime timestamp)
    {
        var key = chord.Key;
        var modifiers = chord.Modifiers;

        if (key == KeyId.Backspace)
        {
            return new InputEvent(timestamp, chord, InputEventKind.Ruecktaste);
        }

        if (key == KeyId.Enter)
        {
            return new InputEvent(timestamp, chord, InputEventKind.Zeichen, "\n");
        }

        if (key == KeyId.Space)
        {
            return new InputEvent(timestamp, chord, InputEventKind.Zeichen, " ");
        }

        var graphem = MapKeyToGraphem(key, modifiers);
        if (graphem == null)
        {
            return new InputEvent(timestamp, chord, InputEventKind.Ignoriert);
        }

        return new InputEvent(timestamp, chord, InputEventKind.Zeichen, graphem);
    }

    private static string? MapKeyToGraphem(KeyId key, ModifierSet modifiers)
    {
        var hasShift = (modifiers & ModifierSet.Shift) != 0;
        var hasAltGr = (modifiers & ModifierSet.AltGr) != 0;

        if (hasAltGr)
        {
            return key switch
            {
                KeyId.E => "€",
                KeyId.OemPlus => "~",
                _ => null
            };
        }

        if (key >= KeyId.A && key <= KeyId.Z)
        {
            var offset = (int)(key - KeyId.A);
            var baseChar = (char)('a' + offset);
            return hasShift ? char.ToUpperInvariant(baseChar).ToString() : baseChar.ToString();
        }

        if (key >= KeyId.Digit0 && key <= KeyId.Digit9)
        {
            if (hasShift)
            {
                return key switch
                {
                    KeyId.Digit1 => "!",
                    KeyId.Digit2 => "\"",
                    KeyId.Digit3 => "§",
                    KeyId.Digit4 => "$",
                    KeyId.Digit5 => "%",
                    KeyId.Digit6 => "&",
                    KeyId.Digit7 => "/",
                    KeyId.Digit8 => "(",
                    KeyId.Digit9 => ")",
                    KeyId.Digit0 => "=",
                    _ => null
                };
            }
            else
            {
                var offset = (int)(key - KeyId.Digit0);
                return ((char)('0' + offset)).ToString();
            }
        }

        return key switch
        {
            KeyId.OemPlus => hasShift ? "*" : "+",
            KeyId.OemMinus => hasShift ? "_" : "-",
            KeyId.OemComma => hasShift ? ";" : ",",
            KeyId.OemPeriod => hasShift ? ":" : ".",
            
            // Deutsche Umlaute (basierend auf KeyId-Enum-Kommentaren)
            KeyId.Oem1 => hasShift ? "Ü" : "ü",  // Ü
            KeyId.Oem3 => hasShift ? "Ö" : "ö",  // Ö
            KeyId.Oem4 => hasShift ? "Ä" : "ä",  // Ä (war vorher falsch: ß/?)
            KeyId.Oem6 => hasShift ? "?" : "ß",  // ß (war vorher falsch: ´/`)
            
            // Sonstige OEM-Tasten
            KeyId.Oem102 => hasShift ? ">" : "<",  // < > |
            KeyId.Oem2 => hasShift ? "'" : "#",   // #
            KeyId.Oem5 => hasShift ? "°" : "^",   // ^
            KeyId.Oem7 => hasShift ? "`" : "´",   // ´ Akut (rechts neben Ü)
            
            _ => null
        };
    }
}
