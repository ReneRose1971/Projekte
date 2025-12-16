using System;
using System.Windows.Input;
using TypeTutor.Logic.Core;
using ModifierKeys = TypeTutor.Logic.Core.ModifierKeys;

namespace TypeTutor.WPF
{
    /// <summary>
    /// Übersetzt WPF-Events in unser Domainmodell (KeyStroke).
    /// - TextInput: liefert fertige Zeichen (Char-First).
    /// - PreviewKeyDown: nur nicht-druckbare Tasten → KeyStroke; druckbare → null (Vermeidung von Doppelverarbeitung).
    /// </summary>
    public sealed class KeyboardAdapter
    {
        public KeyStroke FromTextInput(TextCompositionEventArgs e)
        {
            char c = e.Text[0];
            return new KeyStroke(
                key: KeyCode.None,
                ch: c,
                mods: ModifierKeys.None,
                timestampUtc: DateTime.UtcNow);
        }

        public KeyStroke? FromKeyDown(KeyEventArgs e)
        {
            var key = e.Key == Key.System ? e.SystemKey : e.Key;

            // Druckbare Tasten werden in TextInput verarbeitet → hier ignorieren
            if (IsTextProducingKey(key))
                return null;

            var code = MapKey(key);
            if (code == KeyCode.None)
                return null;

            var mods = ModifierKeys.None;
            if ((Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Shift) != 0) mods |= ModifierKeys.Shift;
            if ((Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) != 0) mods |= ModifierKeys.Control;
            if ((Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Alt) != 0) mods |= ModifierKeys.Alt;

            return new KeyStroke(code, null, mods, DateTime.UtcNow);
        }

        // Expose mapping so callers can highlight visual keys for any Key (including text-producing)
        public static KeyCode MapKey(Key key)
        {
            if (key >= Key.A && key <= Key.Z)
                return (KeyCode)((int)KeyCode.A + ((int)key - (int)Key.A));

            if (key >= Key.D0 && key <= Key.D9)
                return (KeyCode)((int)KeyCode.D0 + ((int)key - (int)Key.D0));

            return key switch
            {
                Key.Space => KeyCode.Space,
                Key.Enter => KeyCode.Enter,
                Key.Back => KeyCode.Backspace,
                Key.Tab => KeyCode.Tab,
                Key.Escape => KeyCode.Escape,
                Key.OemComma => KeyCode.OemComma,
                Key.OemPeriod => KeyCode.OemPeriod,
                Key.OemMinus => KeyCode.OemMinus,
                Key.OemPlus => KeyCode.OemPlus,
                Key.Oem1 => KeyCode.Oem1,
                Key.Oem3 => KeyCode.Oem3,
                Key.Oem2 => KeyCode.Oem2,
                Key.Oem7 => KeyCode.Oem7,
                Key.Oem5 => KeyCode.Oem5,
                Key.Oem102 => KeyCode.Oem102,
                Key.Decimal => KeyCode.NumPadDecimal,
                _ => KeyCode.None
            };
        }

        // ---- Mapping & Helpers ------------------------------------------------

        private static bool IsTextProducingKey(Key key)
        {
            if (key >= Key.A && key <= Key.Z) return true;
            if (key >= Key.D0 && key <= Key.D9) return true;

            return key switch
            {
                Key.Space => true,
                Key.OemComma => true,
                Key.OemPeriod => true,
                Key.OemMinus => true,
                Key.OemPlus => true,
                Key.Oem1 => true, // Ü (DE)
                Key.Oem3 => true, // Ö (DE)
                Key.Oem2 => true, // '#' or other on some layouts
                Key.Oem7 => true, // Ä (DE)
                Key.Oem5 => true, // ß (DE)
                Key.Oem102 => true, // < > | (DE)
                Key.Decimal => true, // numpad decimal/comma produces text input
                _ => false
            };
        }
    }
}
