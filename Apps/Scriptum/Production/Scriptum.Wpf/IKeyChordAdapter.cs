using System.Windows.Input;
using Scriptum.Core;

namespace Scriptum.Wpf;

/// <summary>
/// Adapter zur Übersetzung von WPF-KeyEventArgs in fachliche KeyChord-Objekte.
/// </summary>
public interface IKeyChordAdapter
{
    /// <summary>
    /// Versucht, aus einem WPF-KeyEvent ein fachliches KeyChord zu erstellen.
    /// </summary>
    /// <param name="e">Das WPF-KeyEvent.</param>
    /// <param name="chord">Das resultierende KeyChord (falls erfolgreich).</param>
    /// <returns>
    /// true, wenn die Taste relevant ist und gemappt werden konnte;
    /// false, wenn die Taste ignoriert werden soll (z.B. Ctrl-Kombinationen).
    /// </returns>
    bool TryCreateChord(KeyEventArgs e, out KeyChord chord);
}
