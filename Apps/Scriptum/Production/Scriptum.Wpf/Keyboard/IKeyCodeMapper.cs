using System.Windows.Input;

namespace Scriptum.Wpf.Keyboard;

/// <summary>
/// Mappt WPF Key-Codes auf Tastatur-Labels für DE-QWERTZ Layout.
/// </summary>
public interface IKeyCodeMapper
{
    /// <summary>
    /// Mappt einen WPF Key auf das entsprechende Tastatur-Label.
    /// </summary>
    /// <param name="key">Der WPF Key-Code.</param>
    /// <returns>Das Label der Taste oder null, wenn kein Mapping existiert.</returns>
    string? MapToLabel(Key key);
}
