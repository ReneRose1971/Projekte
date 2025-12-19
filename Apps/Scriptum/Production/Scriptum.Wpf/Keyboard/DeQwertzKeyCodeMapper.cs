using System.Windows.Input;

namespace Scriptum.Wpf.Keyboard;

/// <summary>
/// Standard-Implementierung des Key-Code-Mappers für DE-QWERTZ Layout.
/// </summary>
public sealed class DeQwertzKeyCodeMapper : IKeyCodeMapper
{
    /// <summary>
    /// Mappt einen WPF Key auf das entsprechende Tastatur-Label.
    /// </summary>
    /// <param name="key">Der WPF Key-Code.</param>
    /// <returns>Das Label der Taste oder null, wenn kein Mapping existiert.</returns>
    public string? MapToLabel(Key key)
    {
        if (key >= Key.A && key <= Key.Z)
            return ((char)('A' + (key - Key.A))).ToString();

        if (key >= Key.D0 && key <= Key.D9)
            return ((char)('0' + (key - Key.D0))).ToString();

        return key switch
        {
            Key.Space => "Space",
            Key.Enter or Key.Return => "Enter",
            Key.Back => "Backspace",
            Key.Tab => "Tab",
            Key.Escape => "Esc",
            Key.OemComma => ",",
            Key.OemPeriod => ".",
            Key.OemMinus => "-",
            Key.OemPlus => "+",
            Key.Oem102 => "< > |",
            Key.OemOpenBrackets => "ü",
            Key.OemCloseBrackets => "+",
            Key.Oem1 => "ö",
            Key.Oem3 => "ä",
            Key.Oem5 => "^",
            Key.Oem7 => "ß",
            Key.Oem2 => "#",
            Key.LeftShift or Key.RightShift => "Shift",
            Key.LeftCtrl or Key.RightCtrl => "Ctrl",
            Key.LeftAlt => "Alt",
            Key.RightAlt => "AltGr",
            _ => null
        };
    }
}
