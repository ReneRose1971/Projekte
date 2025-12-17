namespace Scriptum.Engine;

/// <summary>
/// Standardimplementierung der Uhr, die die Systemzeit verwendet.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <summary>
    /// Gibt die aktuelle Systemzeit zurück.
    /// </summary>
    public DateTime Now => DateTime.Now;
}
