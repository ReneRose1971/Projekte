namespace Scriptum.Core;

/// <summary>
/// Kombination von Umschalttasten bei einer Tastatureingabe.
/// </summary>
[Flags]
public enum ModifierSet
{
    /// <summary>
    /// Keine Umschalttaste gedrückt.
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Umschalttaste (Shift) gedrückt.
    /// </summary>
    Shift = 1,
    
    /// <summary>
    /// AltGr-Taste gedrückt.
    /// </summary>
    AltGr = 2
}
