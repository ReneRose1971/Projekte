using Common.Bootstrap.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataToolKit.Abstractions.Repositories
{
    /// <summary>
    /// Builder für Repository-Deltas (Updates/Deletes/Inserts) anhand einer ID-Selektion und eines Inhaltscomparers.
    /// </summary>
    public static class RepositoryDiffBuilder
    {
        /// <summary>
        /// Erzeugt ein Delta für Entitäten mit ganzzahliger ID.
        /// Standard-Policy: Wenn ein Element <c>Id&gt;0</c> hat, in der DB jedoch nicht existiert, wird es als INSERT behandelt.
        /// </summary>
        public static RepositoryDiff<T> Build<T>(
            IEnumerable<T> existing,
            IEnumerable<T> incoming,
            Func<T, int> getId,
            IEqualityComparer<T>? comparer = null,
            bool missingAsInsert = true)
        {
            if (existing is null) throw new ArgumentNullException(nameof(existing));
            if (incoming is null) throw new ArgumentNullException(nameof(incoming));
            if (getId is null) throw new ArgumentNullException(nameof(getId));

            var eq = comparer ?? new FallbackEqualsComparer<T>();

            var existingList = existing.ToList();
            var incomingList = incoming.ToList();

            var existingById = existingList.Where(e => getId(e) > 0).ToDictionary(getId);
            var incomingById = incomingList.Where(e => getId(e) > 0).ToDictionary(getId);

            // Updates: Id vorhanden & inhaltlich verändert
            var toUpdate = new List<T>();
            foreach (var kv in incomingById)
            {
                var id = kv.Key;
                var newItem = kv.Value;

                if (existingById.TryGetValue(id, out var oldItem))
                {
                    if (!eq.Equals(oldItem, newItem))
                        toUpdate.Add(newItem);
                }
            }

            // Deletes: in DB vorhanden, in incoming nicht mehr
            var toDeleteIds = existingById.Keys.Except(incomingById.Keys).ToList();

            // Inserts: (a) neue Entitäten (Id == 0), (b) Id>0, die nicht existieren (Missing-IDs-Policy)
            var toInsert = incomingList.Where(e => getId(e) == 0).ToList();

            if (missingAsInsert)
            {
                foreach (var kv in incomingById)
                {
                    var id = kv.Key;
                    var newItem = kv.Value;
                    if (!existingById.ContainsKey(id))
                        toInsert.Add(newItem);
                }
            }

            return new RepositoryDiff<T>(toUpdate, toDeleteIds, toInsert);
        }

        /// <summary>
        /// Komfortvariante für <see cref="EntityBase"/>: nutzt <c>e =&gt; e.Id</c> als ID-Selektor.
        /// </summary>
        public static RepositoryDiff<T> BuildForEntityBase<T>(
            IEnumerable<T> existing,
            IEnumerable<T> incoming,
            IEqualityComparer<T>? comparer = null,
            bool missingAsInsert = true)
            where T : EntityBase
            => Build(existing, incoming, e => e.Id, comparer, missingAsInsert);
    }
}
