using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeTutor.Logic.Data;

/// <summary>
/// Equality Comparer für LessonData basierend auf dem Title (eindeutiger Identifier).
/// Wird für DataToolKit-Collections und Repository-Operationen verwendet.
/// </summary>
public sealed class LessonDataEqualityComparer : IEqualityComparer<LessonData>
{
    /// <summary>
    /// Vergleicht zwei LessonData-Objekte anhand ihres Titles.
    /// </summary>
    public bool Equals(LessonData? x, LessonData? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        
        return string.Equals(x.Title, y.Title, StringComparison.Ordinal);
    }

    /// <summary>
    /// Berechnet den HashCode basierend auf dem Title.
    /// </summary>
    public int GetHashCode(LessonData obj)
    {
        if (obj is null)
            throw new ArgumentNullException(nameof(obj));
        
        return obj.Title?.GetHashCode() ?? 0;
    }
}
