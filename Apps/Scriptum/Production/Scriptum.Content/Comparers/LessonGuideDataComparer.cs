using Scriptum.Content.Data;

namespace Scriptum.Content.Comparers;

/// <summary>
/// EqualityComparer für <see cref="LessonGuideData"/>.
/// Vergleicht basierend auf <see cref="LessonGuideData.LessonId"/>.
/// </summary>
/// <remarks>
/// Dieser Comparer wird für die Delta-Erkennung beim JSON-Repository verwendet.
/// Zwei Lektions-Anleitungen werden als gleich betrachtet, wenn ihre LessonId identisch ist (Ordinal).
/// </remarks>
public sealed class LessonGuideDataComparer : IEqualityComparer<LessonGuideData>
{
    /// <summary>
    /// Vergleicht zwei LessonGuideData-Instanzen basierend auf ihrer LessonId.
    /// </summary>
    /// <param name="x">Erste Lektions-Anleitung.</param>
    /// <param name="y">Zweite Lektions-Anleitung.</param>
    /// <returns>True, wenn beide LessonIds identisch sind; sonst false.</returns>
    public bool Equals(LessonGuideData? x, LessonGuideData? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null || y is null)
            return false;

        return string.Equals(x.LessonId, y.LessonId, StringComparison.Ordinal);
    }

    /// <summary>
    /// Berechnet den HashCode einer LessonGuideData basierend auf der LessonId.
    /// </summary>
    /// <param name="obj">Die Lektions-Anleitung.</param>
    /// <returns>HashCode basierend auf LessonId (Ordinal).</returns>
    /// <exception cref="ArgumentNullException">Wird ausgelöst, wenn obj null ist.</exception>
    public int GetHashCode(LessonGuideData obj)
    {
        if (obj is null)
            throw new ArgumentNullException(nameof(obj));

        return obj.LessonId.GetHashCode(StringComparison.Ordinal);
    }
}
