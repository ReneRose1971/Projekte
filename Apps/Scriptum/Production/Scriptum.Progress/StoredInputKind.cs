namespace Scriptum.Progress;

/// <summary>
/// Art einer gespeicherten Eingabe.
/// </summary>
public enum StoredInputKind
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
