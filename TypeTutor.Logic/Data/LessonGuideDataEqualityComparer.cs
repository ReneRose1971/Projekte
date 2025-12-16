using System;
using System.Collections.Generic;

namespace TypeTutor.Logic.Data;

/// <summary>
/// Equality Comparer für LessonGuideData basierend auf dem Title (eindeutiger Identifier).
/// Wird für DataToolKit-Collections und Repository-Operationen verwendet.
/// </summary>
public sealed class LessonGuideDataEqualityComparer : IEqualityComparer<LessonGuideData>
{
    /// <summary>
    /// Vergleicht zwei LessonGuideData-Objekte anhand ihres Titles.
    /// </summary>
    public bool Equals(LessonGuideData? x, LessonGuideData? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        
        return string.Equals(x.Title, y.Title, StringComparison.Ordinal);
    }

    /// <summary>
    /// Berechnet den HashCode basierend auf dem Title.
    /// </summary>
    public int GetHashCode(LessonGuideData obj)
    {
        if (obj is null)
            throw new ArgumentNullException(nameof(obj));
        
        return obj.Title?.GetHashCode() ?? 0;
    }
}
