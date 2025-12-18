using Scriptum.Content.Data;

namespace Scriptum.Content.Comparers;

/// <summary>
/// EqualityComparer für <see cref="LessonData"/>.
/// Vergleicht basierend auf <see cref="LessonData.LessonId"/>.
/// </summary>
/// <remarks>
/// Dieser Comparer wird für die Delta-Erkennung beim JSON-Repository verwendet.
/// Zwei Lektionen werden als gleich betrachtet, wenn ihre LessonId identisch ist (Ordinal).
/// </remarks>
public sealed class LessonDataComparer : IEqualityComparer<LessonData>
{
    /// <summary>
    /// Vergleicht zwei LessonData-Instanzen basierend auf ihrer LessonId.
    /// </summary>
    /// <param name="x">Erste Lektion.</param>
    /// <param name="y">Zweite Lektion.</param>
    /// <returns>True, wenn beide LessonIds identisch sind; sonst false.</returns>
    public bool Equals(LessonData? x, LessonData? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null || y is null)
            return false;

        return string.Equals(x.LessonId, y.LessonId, StringComparison.Ordinal);
    }

    /// <summary>
    /// Berechnet den HashCode einer LessonData basierend auf der LessonId.
    /// </summary>
    /// <param name="obj">Die Lektion.</param>
    /// <returns>HashCode basierend auf LessonId (Ordinal).</returns>
    /// <exception cref="ArgumentNullException">Wird ausgelöst, wenn obj null ist.</exception>
    public int GetHashCode(LessonData obj)
    {
        if (obj is null)
            throw new ArgumentNullException(nameof(obj));

        return obj.LessonId.GetHashCode(StringComparison.Ordinal);
    }
}
