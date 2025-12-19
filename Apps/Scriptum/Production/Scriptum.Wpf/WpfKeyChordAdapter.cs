using System.Windows.Input;
using Scriptum.Core;

namespace Scriptum.Wpf;

/// <summary>
/// Implementierung des Key-Adapters für WPF ? Scriptum.Core.
/// </summary>
public sealed class WpfKeyChordAdapter : IKeyChordAdapter
{
    public bool TryCreateChord(KeyEventArgs e, out KeyChord chord)
    {
        chord = default;

        if (!TryMapKey(e.Key, out var keyId))
            return false;

        if (IsControlKeyPressed(e.KeyboardDevice))
            return false;

        var modifiers = GetModifiers(e.KeyboardDevice);
        chord = new KeyChord(keyId, modifiers);
        return true;
    }

    private static bool TryMapKey(Key key, out KeyId keyId)
    {
        keyId = key switch
        {
            Key.A => KeyId.A,
            Key.B => KeyId.B,
            Key.C => KeyId.C,
            Key.D => KeyId.D,
            Key.E => KeyId.E,
            Key.F => KeyId.F,
            Key.G => KeyId.G,
            Key.H => KeyId.H,
            Key.I => KeyId.I,
            Key.J => KeyId.J,
            Key.K => KeyId.K,
            Key.L => KeyId.L,
            Key.M => KeyId.M,
            Key.N => KeyId.N,
            Key.O => KeyId.O,
            Key.P => KeyId.P,
            Key.Q => KeyId.Q,
            Key.R => KeyId.R,
            Key.S => KeyId.S,
            Key.T => KeyId.T,
            Key.U => KeyId.U,
            Key.V => KeyId.V,
            Key.W => KeyId.W,
            Key.X => KeyId.X,
            Key.Y => KeyId.Y,
            Key.Z => KeyId.Z,

            Key.D0 => KeyId.Digit0,
            Key.D1 => KeyId.Digit1,
            Key.D2 => KeyId.Digit2,
            Key.D3 => KeyId.Digit3,
            Key.D4 => KeyId.Digit4,
            Key.D5 => KeyId.Digit5,
            Key.D6 => KeyId.Digit6,
            Key.D7 => KeyId.Digit7,
            Key.D8 => KeyId.Digit8,
            Key.D9 => KeyId.Digit9,

            Key.OemComma => KeyId.OemComma,
            Key.OemPeriod => KeyId.OemPeriod,
            Key.OemMinus => KeyId.OemMinus,
            Key.OemPlus => KeyId.OemPlus,
            Key.Oem102 => KeyId.Oem102,
            
            // DE-QWERTZ Umlaute (KORRIGIERT basierend auf Benutzer-Feedback)
            // Benutzer drückt ö ? WPF Key.Oem3 ? sollte ö ergeben
            // Benutzer drückt ä ? WPF Key.OemQuotes ? sollte ä ergeben
            // Benutzer drückt ü ? WPF Key.Oem1 ? sollte ü ergeben
            // Benutzer drückt ß ? WPF Key.OemOpenBrackets ? sollte ß ergeben
            Key.Oem3 => KeyId.Oem3,             // Ö (Benutzer drückt ö ? Key.Oem3 ? KeyId.Oem3 ? ö)
            Key.OemQuotes => KeyId.Oem4,        // Ä (Benutzer drückt ä ? Key.OemQuotes ? KeyId.Oem4 ? ä)
            Key.Oem1 => KeyId.Oem1,             // Ü (Benutzer drückt ü ? Key.Oem1 ? KeyId.Oem1 ? ü)
            Key.OemOpenBrackets => KeyId.Oem6,  // ß (Benutzer drückt ß ? Key.OemOpenBrackets ? KeyId.Oem6 ? ß)
            
            // Sonstige OEM-Tasten
            Key.Oem5 => KeyId.Oem5,             // ^
            Key.Oem2 => KeyId.Oem2,             // #
            Key.OemCloseBrackets => KeyId.Oem7, // ´

            Key.Space => KeyId.Space,
            Key.Enter or Key.Return => KeyId.Enter,
            Key.Back => KeyId.Backspace,
            Key.Tab => KeyId.Tab,
            Key.Escape => KeyId.Escape,

            Key.LeftShift => KeyId.LeftShift,
            Key.RightShift => KeyId.RightShift,
            Key.LeftAlt => KeyId.LeftAlt,
            Key.RightAlt => KeyId.RightAlt,
            Key.LeftCtrl => KeyId.LeftCtrl,
            Key.RightCtrl => KeyId.RightCtrl,

            _ => KeyId.None
        };

        return keyId != KeyId.None;
    }

    private static ModifierSet GetModifiers(KeyboardDevice keyboard)
    {
        var modifiers = ModifierSet.None;

        if (keyboard.IsKeyDown(Key.LeftShift) || keyboard.IsKeyDown(Key.RightShift))
            modifiers |= ModifierSet.Shift;

        if (keyboard.IsKeyDown(Key.RightAlt))
            modifiers |= ModifierSet.AltGr;

        return modifiers;
    }

    private static bool IsControlKeyPressed(KeyboardDevice keyboard)
    {
        return keyboard.IsKeyDown(Key.LeftCtrl) 
            || keyboard.IsKeyDown(Key.RightCtrl) 
            || keyboard.IsKeyDown(Key.LeftAlt);
    }
}
