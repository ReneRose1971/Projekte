using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DataToolKit.Abstractions.DataStores;

namespace DataToolKit.Storage.Extensions
{
    /// <summary>
    /// Extension-Methoden für die Synchronisation von DataStores.
    /// </summary>
    public static class DataStoreSyncExtensions
    {
        /// <summary>
        /// Synchronisiert einen Target-DataStore mit einem Source-DataStore.
        /// </summary>
        /// <typeparam name="T">Elementtyp.</typeparam>
        /// <param name="target">Ziel-DataStore, der synchronisiert werden soll.</param>
        /// <param name="source">Quell-DataStore, dessen Änderungen überwacht werden.</param>
        /// <param name="comparer">
        /// Optionaler Comparer zur Identifikation von Elementen. 
        /// Default: <see cref="EqualityComparer{T}.Default"/>
        /// </param>
        /// <returns>
        /// Ein <see cref="IDisposable"/>, das die Synchronisation beim Dispose beendet.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <b>Funktionsweise:</b>
        /// </para>
        /// <list type="bullet">
        /// <item>Initiale Synchronisation: Alle Items aus Source werden zu Target hinzugefügt</item>
        /// <item>Laufende Synchronisation: CollectionChanged-Events werden überwacht</item>
        /// <item>Bei Add in Source ? Add in Target</item>
        /// <item>Bei Remove in Source ? Remove in Target</item>
        /// <item>Bei Reset in Source ? Clear + AddRange in Target</item>
        /// </list>
        /// <para>
        /// <b>Identität:</b> Elemente werden mittels <paramref name="comparer"/> identifiziert.
        /// Bei <c>null</c> wird <see cref="EqualityComparer{T}.Default"/> verwendet.
        /// </para>
        /// <para>
        /// <b>Verwendung:</b>
        /// </para>
        /// <code>
        /// var subscription = targetStore.SyncWith(sourceStore);
        /// 
        /// // Änderungen in sourceStore werden automatisch zu targetStore übertragen
        /// sourceStore.Add(item);  // ? targetStore.Add(item)
        /// 
        /// // Synchronisation beenden
        /// subscription.Dispose();
        /// </code>
        /// </remarks>
        public static IDisposable SyncWith<T>(
            this IDataStore<T> target,
            IDataStore<T> source,
            IEqualityComparer<T>? comparer = null)
            where T : class
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (source == null) throw new ArgumentNullException(nameof(source));

            return new DataStoreSync<T>(source, target, comparer ?? EqualityComparer<T>.Default);
        }
    }

    /// <summary>
    /// Interne Implementierung der DataStore-Synchronisation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Überwacht CollectionChanged-Events der Source und überträgt Änderungen zum Target.
    /// </para>
    /// <para>
    /// <b>Achtung:</b> Bei Replace-Operations werden alte Items entfernt und neue hinzugefügt.
    /// Bei Reset wird das Target komplett geleert und neu befüllt.
    /// </para>
    /// </remarks>
    internal sealed class DataStoreSync<T> : IDisposable where T : class
    {
        private readonly IDataStore<T> _source;
        private readonly IDataStore<T> _target;
        private readonly IEqualityComparer<T> _comparer;
        private bool _disposed;

        public DataStoreSync(IDataStore<T> source, IDataStore<T> target, IEqualityComparer<T> comparer)
        {
            _source = source;
            _target = target;
            _comparer = comparer;

            // Initiale Synchronisation: Alle Items aus Source zu Target hinzufügen
            _target.AddRange(_source.Items);

            // CollectionChanged abonnieren
            if (_source.Items is INotifyCollectionChanged collectionChanged)
            {
                collectionChanged.CollectionChanged += OnSourceCollectionChanged;
            }
        }

        private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        _target.AddRange(e.NewItems.Cast<T>());
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        _target.RemoveRange(e.OldItems.Cast<T>());
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    // Alte Items entfernen, neue hinzufügen
                    if (e.OldItems != null)
                    {
                        _target.RemoveRange(e.OldItems.Cast<T>());
                    }
                    if (e.NewItems != null)
                    {
                        _target.AddRange(e.NewItems.Cast<T>());
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    // Komplett neu synchronisieren
                    _target.Clear();
                    _target.AddRange(_source.Items);
                    break;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_source.Items is INotifyCollectionChanged collectionChanged)
            {
                collectionChanged.CollectionChanged -= OnSourceCollectionChanged;
            }
        }
    }
}
