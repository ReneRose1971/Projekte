using System;
using System.Collections.Generic;

namespace DataToolKit.Abstractions.DataStores
{
    /// <summary>
    /// Hilfsmethoden für Collections mit Unterstützung eines <see cref="IEqualityComparer{T}"/>.
    /// </summary>
    public static class CollectionHelpers
    {
        /// <summary>
        /// Prüft, ob die Sequenz ein Element enthält, verglichen mit dem angegebenen Comparer.
        /// </summary>
        public static bool ContainsWithComparer<T>(IEnumerable<T> source, T item, IEqualityComparer<T> comparer)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (comparer is null) throw new ArgumentNullException(nameof(comparer));

            foreach (var existing in source)
            {
                if (comparer.Equals(existing, item))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Liefert den Index des ersten Elements, das dem übergebenen Element entspricht, oder -1.
        /// </summary>
        public static int IndexOfWithComparer<T>(IList<T> list, T item, IEqualityComparer<T> comparer)
        {
            if (list is null) throw new ArgumentNullException(nameof(list));
            if (comparer is null) throw new ArgumentNullException(nameof(comparer));

            for (int i = 0; i < list.Count; i++)
            {
                if (comparer.Equals(list[i], item))
                    return i;
            }
            return -1;
        }
    }
}
