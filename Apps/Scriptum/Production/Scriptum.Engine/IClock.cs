namespace Scriptum.Engine;

/// <summary>
/// Abstraktion für die Zeitsteuerung (für deterministische Tests).
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gibt die aktuelle Zeit zurück.
    /// </summary>
    DateTime Now { get; }
}
