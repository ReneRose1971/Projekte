namespace Scriptum.Engine;

/// <summary>
/// Art der Eingabe.
/// </summary>
public enum InputEventKind
{
    /// <summary>
    /// Ein Zeichen wurde eingegeben.
    /// </summary>
    Zeichen,
    
    /// <summary>
    /// Die Rücktaste wurde gedrückt.
    /// </summary>
    Ruecktaste,
    
    /// <summary>
    /// Die Eingabe wurde ignoriert (z.B. Strg+C).
    /// </summary>
    Ignoriert
}
