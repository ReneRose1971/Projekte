using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

using DataToolKit.Abstractions;
using DataToolKit.Abstractions.DataStores;

namespace DataToolKit.Storage.DataStores
{
    /// <summary>
    /// Basisklasse für in-memory DataStores.
    /// - Verwaltet intern eine <see cref="ObservableCollection{T}"/> und stellt über <see cref="ReadOnlyObservableCollection{T}"/> eine schreibgeschützte Sicht bereit.
    /// - Unterstützt einen optionalen <see cref="IEqualityComparer{T}"/> für konsistente Duplikats-/Entfernungslogik.
    /// - Ist thread-bewusst: Mutationen werden (falls erforderlich) auf den im Konstruktor erfassten <see cref="SynchronizationContext"/> marshalt.
    /// </summary>
    /// <typeparam name="T">Typ der Elemente, die verwaltet werden.</typeparam>
    public class InMemoryDataStore<T> : IDataStore<T>
    {
        private readonly ObservableCollection<T> _inner;
        private readonly IEqualityComparer<T> _comparer;
        private readonly SynchronizationContext? _context;

        /// <summary>
        /// Erstellt eine neue Instanz des <see cref="InMemoryDataStore{T}"/>.
        /// </summary>
        /// <param name="comparer">
        /// Optionaler Gleichheitsvergleicher. Wird <c>null</c> übergeben, kommt <see cref="EqualityComparer{T}.Default"/> zum Einsatz.
        /// </param>
        /// <param name="context">
        /// Optionaler SynchronizationContext. Standard ist <see cref="SynchronizationContext.Current"/> zum Zeitpunkt des Konstruktors.
        /// Alle Mutationen werden – falls vom aufrufenden Thread abweichend – synchron auf diesen Context marshalt.
        /// Ist kein Context verfügbar (<c>null</c>), erfolgen Mutationen ohne Marshaling (Achtung: dann selbst für Threadsicherheit sorgen).
        /// </param>
        public InMemoryDataStore(IEqualityComparer<T>? comparer = null, SynchronizationContext? context = null)
        {
            _comparer = comparer ?? EqualityComparer<T>.Default;
            _context = context ?? SynchronizationContext.Current;

            _inner = new ObservableCollection<T>();
            Items = new ReadOnlyObservableCollection<T>(_inner);
        }

        /// <inheritdoc />
        public ReadOnlyObservableCollection<T> Items { get; }

        /// <inheritdoc />
        public virtual int Count => _inner.Count;

        /// <inheritdoc />
        public virtual bool Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            return _context.Invoke(() =>
            {
                if (CollectionHelpers.ContainsWithComparer(_inner, item, _comparer)) return false;
                _inner.Add(item);
                return true;
            });
        }

        /// <inheritdoc />
        public virtual int AddRange(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            return _context.Invoke(() =>
            {
                int added = 0;
                foreach (var it in items)
                {
                    if (it == null) continue;
                    if (CollectionHelpers.ContainsWithComparer(_inner, it, _comparer)) continue;
                    _inner.Add(it);
                    added++;
                }
                return added;
            });
        }

        /// <inheritdoc />
        public virtual bool Remove(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            return _context.Invoke(() =>
            {
                int index = CollectionHelpers.IndexOfWithComparer(_inner, item, _comparer);
                if (index < 0) return false;
                _inner.RemoveAt(index);
                return true;
            });
        }

        /// <inheritdoc />
        public virtual int RemoveRange(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            return _context.Invoke(() =>
            {
                int removed = 0;
                foreach (var it in items)
                {
                    if (it == null) continue;
                    int index = CollectionHelpers.IndexOfWithComparer(_inner, it, _comparer);
                    if (index < 0) continue;
                    _inner.RemoveAt(index);
                    removed++;
                }
                return removed;
            });
        }

        /// <inheritdoc />
        public virtual int RemoveWhere(Predicate<T> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return _context.Invoke(() =>
            {
                var toRemove = _inner.Where(x => predicate(x)).ToList();
                int count = 0;
                foreach (var r in toRemove)
                {
                    int idx = CollectionHelpers.IndexOfWithComparer(_inner, r, _comparer);
                    if (idx >= 0)
                    {
                        _inner.RemoveAt(idx);
                        count++;
                    }
                }
                return count;
            });
        }

        /// <inheritdoc />
        public virtual void Clear()
        {
            _context.Invoke(() => _inner.Clear());
        }

        // -------------------- Hilfsfunktionen via CollectionHelpers --------------------
    }
}
