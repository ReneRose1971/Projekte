using Scriptum.Engine;

namespace Scriptum.Application.Tests.Fakes;

/// <summary>
/// Fake-Clock für deterministische Tests.
/// </summary>
public sealed class FakeClock : IClock
{
    /// <summary>
    /// Die aktuelle Zeit (kann manuell gesetzt werden).
    /// </summary>
    public DateTime Now { get; set; } = DateTime.Now;

    /// <summary>
    /// Springt zur angegebenen Zeit.
    /// </summary>
    public void SetTime(DateTime time)
    {
        Now = time;
    }

    /// <summary>
    /// Fügt eine Zeitspanne hinzu.
    /// </summary>
    public void Advance(TimeSpan span)
    {
        Now += span;
    }
}
