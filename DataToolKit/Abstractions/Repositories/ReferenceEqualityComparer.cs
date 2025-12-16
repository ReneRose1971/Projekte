using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DataToolKit.Abstractions.Repositories
{
    /// <summary>
    /// Referenzbasierter Gleichheitsvergleicher für Referenztypen.
    /// Nützlich, um Doppelbindungen (Event-Handler) zu verhindern und Puffersätze per Referenz zu führen.
    /// </summary>
    /// <typeparam name="T">Referenztyp.</typeparam>
    public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        public static readonly ReferenceEqualityComparer<T> Default = new();

        public bool Equals(T? x, T? y) => ReferenceEquals(x, y);

        public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
