using System;
using System.Collections.Generic;

namespace Common.Bootstrap.Defaults;

/// <summary>
/// Fallback-Comparer, der ausschließlich <c>x.Equals(y)</c> verwendet.
/// Nutzt keine Reflexion und keine Feld-/Propertyvergleiche.
/// Dieser Comparer dient als allgemeingültige Standard-Implementierung für <see cref="IEqualityComparer{T}"/>,
/// wenn keine typspezifische Implementierung verfügbar ist.
/// </summary>
/// <typeparam name="T">Der zu vergleichende Typ.</typeparam>
public sealed class FallbackEqualsComparer<T> : IEqualityComparer<T>
{
    /// <summary>
    /// Vergleicht zwei Objekte auf Gleichheit mittels <c>x.Equals(y)</c>.
    /// </summary>
    /// <param name="x">Das erste zu vergleichende Objekt.</param>
    /// <param name="y">Das zweite zu vergleichende Objekt.</param>
    /// <returns><c>true</c>, wenn die Objekte gleich sind; andernfalls <c>false</c>.</returns>
    public bool Equals(T? x, T? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return x.Equals(y);
    }

    /// <summary>
    /// Liefert den Hash-Code des Objekts mittels <c>obj.GetHashCode()</c>.
    /// </summary>
    /// <param name="obj">Das Objekt, für das der Hash-Code berechnet werden soll.</param>
    /// <returns>Der Hash-Code des Objekts.</returns>
    /// <exception cref="ArgumentNullException">Wenn <paramref name="obj"/> <c>null</c> ist.</exception>
    public int GetHashCode(T obj)
    {
        if (obj is null) throw new ArgumentNullException(nameof(obj));
        return obj.GetHashCode();
    }
}
