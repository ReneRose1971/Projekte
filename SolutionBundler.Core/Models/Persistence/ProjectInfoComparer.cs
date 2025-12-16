using System;
using System.Collections.Generic;

namespace SolutionBundler.Core.Models;

/// <summary>
/// EqualityComparer für ProjectInfo basierend auf dem Name-Property (case-insensitive).
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
