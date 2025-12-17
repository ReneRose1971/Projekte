using System;
using System.Collections.Generic;

namespace TypeTutor.Logic.Data;

/// <summary>
/// Equality Comparer für ModuleData basierend auf dem ModuleId (eindeutiger Identifier).
/// Wird für DataToolKit-Collections und Repository-Operationen verwendet.
/// </summary>
public sealed class ModuleDataEqualityComparer : IEqualityComparer<ModuleData>
{
    /// <summary>
    /// Vergleicht zwei ModuleData-Objekte anhand ihrer ModuleId.
    /// </summary>
    public bool Equals(ModuleData? x, ModuleData? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        
        return string.Equals(x.ModuleId, y.ModuleId, StringComparison.Ordinal);
    }

    /// <summary>
    /// Berechnet den HashCode basierend auf der ModuleId.
    /// Gibt 0 zurück für null (gemäß IEqualityComparer-Konvention).
    /// </summary>
    public int GetHashCode(ModuleData obj)
    {
        if (obj is null) return 0;
        
        return obj.ModuleId?.GetHashCode() ?? 0;
    }
}
