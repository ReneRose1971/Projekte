using DataToolKit.Abstractions.Repositories;
using System.ComponentModel;
using System.Collections.Specialized;
using DataToolKit.Abstractions.DataStores;

namespace DataToolKit.Storage.Persistence
{
    /// <summary>
    /// Bindet einmalig pro Entität an INotifyPropertyChanged und ruft beim Eintreten die angegebene Aktion auf.
    /// Doppelbindungen werden zuverlässig verhindert (idempotentes Attach).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Zwei Modi:</b>
    /// </para>
    /// <list type="number">
    /// <item>
    /// <b>Manueller Modus:</b> <see cref="Attach"/>/<see cref="Detach"/> werden explizit aufgerufen.
    /// </item>
    /// <item>
    /// <b>DataStore-Modus:</b> Über <see cref="AttachToDataStore"/> wird automatisch auf 
    /// CollectionChanged-Events reagiert und Items werden synchronisiert.
    /// </item>
    /// </list>
    /// <para>
    /// <b>Idempotenz:</b> Mehrfaches Attach derselben Instanz ist sicher - es wird nur einmal gebunden.
    /// </para>
    /// </remarks>
    public sealed class PropertyChangedBinder<T> : IDisposable where T : class
    {
        private readonly bool _enabled;
        private readonly Action<T> _onEntityChanged;

        // Referenzbasiertes Tracking
        private readonly HashSet<T> _bound = new(ReferenceEqualityComparer<T>.Default);
        private IDisposable? _dataStoreSubscription;
        private bool _disposed;

        /// <summary>
        /// Erstellt einen PropertyChangedBinder.
        /// </summary>
        /// <param name="enabled">Wenn <c>false</c>, werden alle Operationen übersprungen.</param>
        /// <param name="onEntityChanged">Callback, der bei PropertyChanged aufgerufen wird.</param>
        public PropertyChangedBinder(bool enabled, Action<T> onEntityChanged)
        {
            _enabled = enabled;
            _onEntityChanged = onEntityChanged ?? throw new ArgumentNullException(nameof(onEntityChanged));
        }

        /// <summary>
        /// Bindet den Binder automatisch an einen DataStore.
        /// </summary>
        /// <param name="dataStore">Der zu überwachende DataStore.</param>
        /// <returns>
        /// Ein <see cref="IDisposable"/>, das die DataStore-Bindung beim Dispose beendet.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Im DataStore-Modus werden automatisch:
        /// </para>
        /// <list type="bullet">
        /// <item>Alle existierenden Items gebunden</item>
        /// <item>Neue Items bei CollectionChanged.Add gebunden</item>
        /// <item>Entfernte Items bei CollectionChanged.Remove entbunden</item>
        /// <item>Alle Items bei CollectionChanged.Reset neu gebunden</item>
        /// </list>
        /// <para>
        /// <b>Achtung:</b> Dispose des zurückgegebenen IDisposable beendet nur die DataStore-Bindung,
        /// nicht den Binder selbst. Bereits gebundene Items bleiben gebunden bis <see cref="Dispose"/> 
        /// des Binders aufgerufen wird.
        /// </para>
        /// </remarks>
        public IDisposable AttachToDataStore(IDataStore<T> dataStore)
        {
            if (dataStore == null) throw new ArgumentNullException(nameof(dataStore));
            if (!_enabled) return new EmptyDisposable();

            // Alle existierenden Items binden
            AttachRange(dataStore.Items);

            // CollectionChanged abonnieren
            if (dataStore.Items is INotifyCollectionChanged collectionChanged)
            {
                collectionChanged.CollectionChanged += OnDataStoreCollectionChanged;
            }

            // Disposable zurückgeben, das die DataStore-Bindung entfernt
            var subscription = new DataStoreSubscription(this, dataStore);
            _dataStoreSubscription = subscription;
            return subscription;
        }

        /// <summary>
        /// Idempotentes Binden: Event-Handler wird vor dem Anfügen sicherheitshalber entfernt
        /// und danach genau einmal angefügt. Dadurch sind Doppelbindungen ausgeschlossen,
        /// selbst wenn Attach mehrfach auf derselben Instanz aufgerufen wird.
        /// </summary>
        public void Attach(T entity)
        {
            if (!_enabled || entity is not INotifyPropertyChanged npc) return;

            // Idempotent: erst abmelden, dann anmelden
            npc.PropertyChanged -= OnEntityPropertyChanged;
            npc.PropertyChanged += OnEntityPropertyChanged;

            _bound.Add(entity);
        }

        public void AttachRange(IEnumerable<T> entities)
        {
            if (!_enabled) return;
            foreach (var e in entities) Attach(e);
        }

        public void Detach(T entity)
        {
            if (!_enabled || entity is not INotifyPropertyChanged npc) return;

            npc.PropertyChanged -= OnEntityPropertyChanged;
            _bound.Remove(entity);
        }

        public void DetachAll()
        {
            if (!_enabled) return;

            foreach (var e in _bound.ToList())
            {
                if (e is INotifyPropertyChanged npc)
                    npc.PropertyChanged -= OnEntityPropertyChanged;
            }
            _bound.Clear();
        }

        private void OnEntityPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is T entity)
                _onEntityChanged(entity);
        }

        private void OnDataStoreCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        AttachRange(e.NewItems.Cast<T>());
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems.Cast<T>())
                        {
                            Detach(item);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems.Cast<T>())
                        {
                            Detach(item);
                        }
                    }
                    if (e.NewItems != null)
                    {
                        AttachRange(e.NewItems.Cast<T>());
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    DetachAll();
                    break;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _dataStoreSubscription?.Dispose();
            DetachAll();
        }

        /// <summary>
        /// Subscription für DataStore-Bindung.
        /// </summary>
        private sealed class DataStoreSubscription : IDisposable
        {
            private readonly PropertyChangedBinder<T> _binder;
            private readonly IDataStore<T> _dataStore;
            private bool _disposed;

            public DataStoreSubscription(PropertyChangedBinder<T> binder, IDataStore<T> dataStore)
            {
                _binder = binder;
                _dataStore = dataStore;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;

                // CollectionChanged abmelden
                if (_dataStore.Items is INotifyCollectionChanged collectionChanged)
                {
                    collectionChanged.CollectionChanged -= _binder.OnDataStoreCollectionChanged;
                }
            }
        }

        /// <summary>
        /// Leeres Disposable für den Fall, dass _enabled = false.
        /// </summary>
        private sealed class EmptyDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }
}
