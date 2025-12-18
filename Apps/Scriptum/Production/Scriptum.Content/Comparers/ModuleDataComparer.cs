using Scriptum.Content.Data;

namespace Scriptum.Content.Comparers;

/// <summary>
/// EqualityComparer für <see cref="ModuleData"/>.
/// Vergleicht basierend auf <see cref="ModuleData.ModuleId"/>.
/// </summary>
/// <remarks>
/// Dieser Comparer wird für die Delta-Erkennung beim JSON-Repository verwendet.
/// Zwei Module werden als gleich betrachtet, wenn ihre ModuleId identisch ist (Ordinal).
/// </remarks>
public sealed class ModuleDataComparer : IEqualityComparer<ModuleData>
{
    /// <summary>
    /// Vergleicht zwei ModuleData-Instanzen basierend auf ihrer ModuleId.
    /// </summary>
    /// <param name="x">Erstes Modul.</param>
    /// <param name="y">Zweites Modul.</param>
    /// <returns>True, wenn beide ModuleIds identisch sind; sonst false.</returns>
    public bool Equals(ModuleData? x, ModuleData? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null || y is null)
            return false;

        return string.Equals(x.ModuleId, y.ModuleId, StringComparison.Ordinal);
    }

    /// <summary>
    /// Berechnet den HashCode eines ModuleData basierend auf der ModuleId.
    /// </summary>
    /// <param name="obj">Das Modul.</param>
    /// <returns>HashCode basierend auf ModuleId (Ordinal).</returns>
    /// <exception cref="ArgumentNullException">Wird ausgelöst, wenn obj null ist.</exception>
    public int GetHashCode(ModuleData obj)
    {
        if (obj is null)
            throw new ArgumentNullException(nameof(obj));

        return obj.ModuleId.GetHashCode(StringComparison.Ordinal);
    }
}
