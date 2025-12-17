using System;
using System.Collections.Generic;

namespace SolutionBundler.Core.Models.Persistence;

/// <summary>
/// EqualityComparer für ProjectInfo basierend auf dem Name-Property (case-insensitive).
/// Die Group-Property wird bewusst NICHT berücksichtigt, damit Gruppenänderungen
/// nicht zur Erstellung neuer Objekte führen.
/// </summary>
public sealed class ProjectInfoComparer : IEqualityComparer<ProjectInfo>
{
    public bool Equals(ProjectInfo? x, ProjectInfo? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(ProjectInfo obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name);
    }
}
