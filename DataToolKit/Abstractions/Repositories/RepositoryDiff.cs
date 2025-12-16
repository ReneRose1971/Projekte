using System;
using System.Collections.Generic;

namespace DataToolKit.Abstractions.Repositories
{
    /// <summary>
    /// Ergebnis der Delta-Berechnung zwischen bestehendem DB-Bestand und neuem Zielbestand.
    /// </summary>
    public sealed class RepositoryDiff<T>
    {
        /// <summary>Elemente, die inhaltlich geändert wurden und zu aktualisieren sind.</summary>
        public IReadOnlyList<T> ToUpdate { get; }

        /// <summary>IDs, die in der DB existieren, im Zielbestand aber fehlen und zu löschen sind.</summary>
        public IReadOnlyList<int> ToDeleteIds { get; }

        /// <summary>Neue Elemente (Id==0) und Elemente mit Id&gt;0, die in der DB fehlen (Missing-IDs-Policy).</summary>
        public IReadOnlyList<T> ToInsert { get; }

        public RepositoryDiff(IReadOnlyList<T> toUpdate, IReadOnlyList<int> toDeleteIds, IReadOnlyList<T> toInsert)
        {
            ToUpdate = toUpdate ?? throw new ArgumentNullException(nameof(toUpdate));
            ToDeleteIds = toDeleteIds ?? throw new ArgumentNullException(nameof(toDeleteIds));
            ToInsert = toInsert ?? throw new ArgumentNullException(nameof(toInsert));
        }
    }
}
