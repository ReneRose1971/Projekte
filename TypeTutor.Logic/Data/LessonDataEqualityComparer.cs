using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeTutor.Logic.Data;

/// <summary>
/// Equality Comparer für LessonData basierend auf dem LessonId (eindeutiger Identifier).
/// Wird für DataToolKit-Collections und Repository-Operationen verwendet.
/// </summary>
public sealed class LessonDataEqualityComparer : IEqualityComparer<LessonData>
{
    /// <summary>
    /// Vergleicht zwei LessonData-Objekte anhand ihrer LessonId.
    /// </summary>
    public bool Equals(LessonData? x, LessonData? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        
        return string.Equals(x.LessonId, y.LessonId, StringComparison.Ordinal);
    }

    /// <summary>
    /// Berechnet den HashCode basierend auf der LessonId.
    /// Gibt 0 zurück für null (gemäß IEqualityComparer-Konvention).
    /// </summary>
    public int GetHashCode(LessonData obj)
    {
        if (obj is null) return 0;
        
        return obj.LessonId?.GetHashCode() ?? 0;
    }
}
