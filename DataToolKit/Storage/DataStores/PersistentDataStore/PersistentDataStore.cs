using System;
using System.Linq;
using Common.Bootstrap;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.Persistence;

namespace DataToolKit.Storage.DataStores
{
    /// <summary>
    /// Persistenter DataStore für kleine Datenmengen (&lt; 100 Einträge).
    /// Kombiniert In-Memory-Verwaltung mit sofortiger Persistierung.
    /// </summary>
    /// <typeparam name="T">Entitätstyp (class constraint - funktioniert mit POCOs und EntityBase).</typeparam>
    /// <remarks>
    /// <para>
    /// <b>Automatische Strategie-Auswahl:</b>
    /// </para>
    /// <list type="bullet">
    /// <item><b>JSON-Repository</b> (<see cref="IRepositoryBase{T}"/>): Alle Änderungen führen zu vollständigem <c>Write()</c></item>
    /// <item><b>LiteDB-Repository</b> (<see cref="IRepository{T}"/>): Nutzt granulare <c>Update()</c> und <c>Delete()</c></item>
    /// </list>
    /// <para>
    /// <b>PropertyChanged-Tracking:</b> Wenn aktiviert, werden Änderungen an Entitäts-Properties
    /// automatisch erkannt (via <see cref="System.ComponentModel.INotifyPropertyChanged"/>) und persistiert.
    /// </para>
    /// <para>
    /// <b>Verwendung mit POCOs (JSON):</b>
    /// </para>
    /// <code>
    /// // POCO ohne EntityBase
    /// public class Settings
    /// {
    ///     public string Theme { get; set; }
    ///     public int FontSize { get; set; }
    /// }
    /// 
    /// var jsonRepo = new JsonRepository&lt;Settings&gt;(options);
    /// var store = new PersistentDataStore&lt;Settings&gt;(jsonRepo);
    /// store.Load();
    /// store.Add(new Settings { Theme = "Dark" });  // Sofort persistiert
    /// </code>
    /// <para>
    /// <b>Verwendung mit EntityBase (LiteDB):</b>
    /// </para>
    /// <code>
    /// public class Customer : EntityBase
    /// {
    ///     public string Name { get; set; }
    /// }
    /// 
    /// var liteDbRepo = new LiteDbRepository&lt;Customer&gt;(options, comparer);
    /// var store = new PersistentDataStore&lt;Customer&gt;(liteDbRepo);
    /// store.Load();
    /// store.Add(new Customer { Name = "Alice" });  // Sofort persistiert
    /// 
    /// var customer = store.Items.First();
    /// customer.Name = "Bob";  // Sofort persistiert (via PropertyChanged)
    /// </code>
    /// </remarks>
    public sealed class PersistentDataStore<T> : InMemoryDataStore<T>, IDisposable 
        where T : class
    {
        private readonly IRepositoryBase<T> _repository;
        private readonly IPersistenceStrategy<T> _strategy;
        private readonly PropertyChangedBinder<T> _propBinder;
        private readonly DisposableCollection _disposables = new();
        private bool _disposed;

        /// <summary>
        /// Erstellt einen PersistentDataStore mit automatischer Strategie-Auswahl.
        /// </summary>
        /// <param name="repository">
        /// Repository für Persistierung. Kann <see cref="IRepositoryBase{T}"/> (JSON)
        /// oder <see cref="IRepository{T}"/> (LiteDB) sein. Die Strategie wird automatisch gewählt.
        /// </param>
        /// <param name="trackPropertyChanges">
        /// Wenn <c>true</c>, werden Änderungen an Entitäts-Properties automatisch
        /// persistiert (via <see cref="System.ComponentModel.INotifyPropertyChanged"/>).
        /// Entitäten müssen <see cref="System.ComponentModel.INotifyPropertyChanged"/> implementieren.
        /// Standard: <c>true</c>.
        /// </param>
        /// <exception cref="ArgumentNullException">Wenn <paramref name="repository"/> null ist.</exception>
        /// <remarks>
        /// <para>
        /// Nach der Konstruktion sollte <see cref="Load"/> aufgerufen werden, um Daten zu laden.
        /// </para>
        /// <para>
        /// <b>Ressourcen-Management:</b> Verwendet <see cref="DisposableCollection"/> für automatisches
        /// LIFO-Dispose der verwalteten Ressourcen (PropertyChangedBinder → PersistenceStrategy).
        /// </para>
        /// </remarks>
        public PersistentDataStore(
            IRepositoryBase<T> repository,
            bool trackPropertyChanges = true)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));

            // Automatische Strategie-Auswahl über Factory
            _strategy = PersistenceStrategyFactory.Create(repository, () => Items);

            // PropertyChanged-Tracking einrichten
            _propBinder = new PropertyChangedBinder<T>(
                trackPropertyChanges,
                entity => _strategy.OnEntityChanged(entity));

            // Ressourcen in DisposableCollection registrieren (LIFO-Reihenfolge)
            _disposables.Add(_propBinder);
            _disposables.Add(_strategy);
        }

        /// <summary>
        /// Lädt alle Daten aus dem Repository und füllt die In-Memory-Collection.
        /// </summary>
        /// <exception cref="InvalidOperationException">Wenn beim Laden ein Fehler auftritt.</exception>
        /// <exception cref="System.IO.IOException">Bei Dateisystem-Fehlern (JSON-Repository).</exception>
        /// <remarks>
        /// <para>
        /// Diese Methode sollte typischerweise direkt nach der Konstruktion aufgerufen werden.
        /// Bereits geladene Items werden mit PropertyChanged-Tracking verbunden (falls aktiviert).
        /// </para>
        /// <para>
        /// <b>Wichtig:</b> Diese Methode löst <b>keine</b> Persistierung aus - die Daten werden
        /// nur aus dem Repository geladen und in die In-Memory-Collection eingefügt.
        /// </para>
        /// </remarks>
        public void Load()
        {
            var items = _repository.Load();

            // Items zur In-Memory-Collection hinzufügen (ohne Persistierung)
            base.AddRange(items);

            // PropertyChanged-Binding für geladene Items aktivieren
            _propBinder.AttachRange(items);
        }

        /// <summary>
        /// Fügt ein Element zur Collection hinzu und persistiert sofort.
        /// </summary>
        /// <param name="item">Das hinzuzufügende Element.</param>
        /// <returns><c>true</c>, wenn das Element hinzugefügt wurde; <c>false</c>, wenn es bereits existiert.</returns>
        /// <exception cref="ArgumentNullException">Wenn <paramref name="item"/> null ist.</exception>
        public override bool Add(T item)
        {
            var added = base.Add(item);
            if (!added) return false;

            _propBinder.Attach(item);
            _strategy.OnAdded(item);
            return true;
        }

        /// <summary>
        /// Fügt mehrere Elemente zur Collection hinzu und persistiert sofort.
        /// </summary>
        /// <param name="items">Die hinzuzufügenden Elemente.</param>
        /// <returns>Anzahl der tatsächlich hinzugefügten Elemente.</returns>
        /// <exception cref="ArgumentNullException">Wenn <paramref name="items"/> null ist.</exception>
        public override int AddRange(System.Collections.Generic.IEnumerable<T> items)
        {
            var count = base.AddRange(items);
            if (count <= 0) return 0;

            _propBinder.AttachRange(items);
            
            // Für jedes hinzugefügte Item OnAdded aufrufen
            foreach (var item in items.Where(e => e is not null))
            {
                _strategy.OnAdded(item);
            }
            
            return count;
        }

        /// <summary>
        /// Entfernt ein Element aus der Collection und persistiert sofort.
        /// </summary>
        /// <param name="item">Das zu entfernende Element.</param>
        /// <returns><c>true</c>, wenn das Element entfernt wurde; <c>false</c>, wenn es nicht gefunden wurde.</returns>
        /// <exception cref="ArgumentNullException">Wenn <paramref name="item"/> null ist.</exception>
        public override bool Remove(T item)
        {
            var removed = base.Remove(item);
            if (!removed) return false;

            _propBinder.Detach(item);
            _strategy.OnRemoved(item);
            return true;
        }

        /// <summary>
        /// Entfernt mehrere Elemente aus der Collection und persistiert sofort.
        /// </summary>
        /// <param name="items">Die zu entfernenden Elemente.</param>
        /// <returns>Anzahl der entfernten Elemente.</returns>
        /// <exception cref="ArgumentNullException">Wenn <paramref name="items"/> null ist.</exception>
        public override int RemoveRange(System.Collections.Generic.IEnumerable<T> items)
        {
            var count = base.RemoveRange(items);
            if (count <= 0) return 0;

            foreach (var item in items.Where(e => e is not null))
            {
                _propBinder.Detach(item);
                _strategy.OnRemoved(item);
            }
            
            return count;
        }

        /// <summary>
        /// Leert die Collection vollständig und persistiert die Änderung sofort.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            _propBinder.DetachAll();
            _strategy.OnCleared();
        }

        /// <summary>
        /// Gibt Ressourcen frei und trennt alle PropertyChanged-Bindings.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Dispose-Reihenfolge:</b> Ressourcen werden automatisch in LIFO-Reihenfolge entsorgt:
        /// </para>
        /// <list type="number">
        /// <item><see cref="PropertyChangedBinder{T}"/> (zuletzt hinzugefügt → zuerst entsorgt)</item>
        /// <item><see cref="IPersistenceStrategy{T}"/> (zuerst hinzugefügt → zuletzt entsorgt)</item>
        /// </list>
        /// <para>
        /// <b>Exception-Handling:</b> Falls beim Entsorgen einer Ressource ein Fehler auftritt,
        /// werden die verbleibenden Ressourcen trotzdem entsorgt und alle Exceptions als
        /// <see cref="AggregateException"/> gesammelt und geworfen.
        /// </para>
        /// </remarks>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _disposables.Dispose();
        }
    }
}
