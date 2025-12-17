using System;
using System.Collections.Generic;

namespace TypeTutor.Logic.Data;

/// <summary>
/// Equality Comparer für LessonGuideData basierend auf dem LessonId (eindeutiger Identifier).
/// Wird für DataToolKit-Collections und Repository-Operationen verwendet.
/// </summary>
public sealed class LessonGuideDataEqualityComparer : IEqualityComparer<LessonGuideData>
{
    /// <summary>
    /// Vergleicht zwei LessonGuideData-Objekte anhand ihrer LessonId.
    /// </summary>
    public bool Equals(LessonGuideData? x, LessonGuideData? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        
        return string.Equals(x.LessonId, y.LessonId, StringComparison.Ordinal);
    }

    /// <summary>
    /// Berechnet den HashCode basierend auf der LessonId.
    /// Gibt 0 zurück für null (gemäß IEqualityComparer-Konvention).
    /// </summary>
    public int GetHashCode(LessonGuideData obj)
    {
        if (obj is null) return 0;
        
        return obj.LessonId?.GetHashCode() ?? 0;
    }
}
