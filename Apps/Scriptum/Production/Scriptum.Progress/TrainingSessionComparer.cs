using System;
using System.Collections.Generic;
using System.Linq;
using Scriptum.Progress;

namespace Scriptum.Persistence;

/// <summary>
/// EqualityComparer für <see cref="TrainingSession"/>.
/// Vergleicht alle relevanten Properties (nicht nur die Id).
/// </summary>
/// <remarks>
/// Dieser Comparer wird für die Delta-Erkennung beim LiteDB-Repository verwendet.
/// Zwei Sessions werden als gleich betrachtet, wenn alle ihre Properties identisch sind.
/// </remarks>
public sealed class TrainingSessionComparer : IEqualityComparer<TrainingSession>
{
    /// <summary>
    /// Vergleicht zwei TrainingSession-Instanzen auf inhaltliche Gleichheit.
    /// </summary>
    /// <param name="x">Erste Session.</param>
    /// <param name="y">Zweite Session.</param>
    /// <returns>True, wenn alle Properties identisch sind; sonst false.</returns>
    public bool Equals(TrainingSession? x, TrainingSession? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null || y is null)
            return false;

        return x.Id == y.Id
            && string.Equals(x.LessonId, y.LessonId, StringComparison.Ordinal)
            && string.Equals(x.ModuleId, y.ModuleId, StringComparison.Ordinal)
            && x.StartedAt == y.StartedAt
            && x.EndedAt == y.EndedAt
            && x.IsCompleted == y.IsCompleted
            && SequenceEqual(x.Inputs, y.Inputs)
            && SequenceEqual(x.Evaluations, y.Evaluations);
    }

    /// <summary>
    /// Berechnet den HashCode einer TrainingSession.
    /// </summary>
    /// <param name="obj">Die TrainingSession.</param>
    /// <returns>HashCode basierend auf allen Properties.</returns>
    public int GetHashCode(TrainingSession obj)
    {
        if (obj is null)
            throw new ArgumentNullException(nameof(obj));

        var hash = new HashCode();
        hash.Add(obj.Id);
        hash.Add(obj.LessonId, StringComparer.Ordinal);
        hash.Add(obj.ModuleId, StringComparer.Ordinal);
        hash.Add(obj.StartedAt);
        hash.Add(obj.EndedAt);
        hash.Add(obj.IsCompleted);

        AddSequence(hash, obj.Inputs);
        AddSequence(hash, obj.Evaluations);

        return hash.ToHashCode();
    }

    private static bool SequenceEqual<T>(List<T> a, List<T> b) where T : class
    {
        if (a.Count != b.Count)
            return false;

        return a.SequenceEqual(b, EqualityComparer<T>.Default);
    }

    private static void AddSequence<T>(HashCode hash, List<T> items) where T : class
    {
        hash.Add(items.Count);

        for (int i = 0; i < items.Count; i++)
        {
            hash.Add(items[i], EqualityComparer<T>.Default);
        }
    }
}
