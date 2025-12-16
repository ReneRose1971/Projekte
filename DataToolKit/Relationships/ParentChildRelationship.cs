using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Storage.DataStores;
using DataToolKit.Storage.Persistence;

namespace DataToolKit.Relationships
{
    /// <summary>
    /// Verwaltet eine 1:n-Beziehung zwischen einem statischen Parent-Objekt und einer gefilterten Auflistung von Child-Elementen.
    /// </summary>
    /// <typeparam name="TParent">Typ des Parent-Objekts.</typeparam>
    /// <typeparam name="TChild">Typ der Child-Elemente (muss eine Klasse sein).</typeparam>
    /// <remarks>
    /// <para>
    /// <b>Architektur:</b>
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <b>DataSource:</b> Enthält ALLE potentiellen Child-Elemente (z.B. alle Orders).
    /// CollectionChanged wird manuell überwacht.
    /// </item>
    /// <item>
    /// <b>PropertyChangedBinder:</b> Überwacht PropertyChanged für ALLE Items in DataSource,
    /// um zu erkennen, wenn ein Item durch Property-Änderung die Filterbedingung erfüllt/nicht mehr erfüllt.
    /// </item>
    /// <item>
    /// <b>Childs:</b> Enthält nur die gefilterten Items, die zum Parent gehören.
    /// </item>
    /// </list>
    /// <para>
    /// <b>Funktionsweise:</b>
    /// </para>
    /// <list type="bullet">
    /// <item>Die <see cref="DataSource"/> stellt alle potentiellen Child-Elemente bereit.</item>
    /// <item>Der <see cref="IsChildFilter"/> entscheidet, welche Elemente zum <see cref="Parent"/> gehören.</item>
    /// <item>Die <see cref="Childs"/>-Collection wird automatisch synchronisiert.</item>
    /// </list>
    /// <para>
    /// <b>Parent-Handling:</b> Das Parent-Objekt ist statisch und kann nur einmal gesetzt werden.
    /// Eine Änderung zur Laufzeit ist nicht vorgesehen.
    /// </para>
    /// <para>
    /// <b>Performance:</b> Optimiert für geringe Datenmengen (&lt; 100 Einträge) und moderate Änderungshäufigkeit.
    /// </para>
    /// <para>
    /// <b>Verwendung:</b>
    /// </para>
    /// <code>
    /// // Variante 1: Automatisches Laden der DataSource via GetDataStore
    /// var relationship = new ParentChildRelationship&lt;Customer, Order&gt;(dataStoreProvider)
    /// {
    ///     Parent = selectedCustomer,
    ///     IsChildFilter = (customer, order) => order.CustomerId == customer.Id
    /// };
    /// // DataSource wird automatisch via provider.GetDataStore&lt;Order&gt;() geladen
    /// 
    /// // Variante 2: Explizite DataSource-Zuweisung
    /// var relationship = new ParentChildRelationship&lt;Customer, Order&gt;(dataStoreProvider)
    /// {
    ///     DataSource = customOrderStore,
    ///     Parent = selectedCustomer,
    ///     IsChildFilter = (customer, order) => order.CustomerId == customer.Id
    /// };
    /// 
    /// // Childs wird automatisch aktualisiert
    /// var orders = relationship.Childs.Items;
    /// </code>
    /// </remarks>
    public sealed class ParentChildRelationship<TParent, TChild> : IDisposable
        where TChild : class
    {
        private readonly IDataStoreProvider _dataStoreProvider;
        private readonly InMemoryDataStore<TChild> _childStore;
        private readonly PropertyChangedBinder<TChild> _propertyChangedBinder;
        
        private TParent? _parent;
        private Func<TParent, TChild, bool>? _isChildFilter;
        private IDataStore<TChild>? _dataSource;
        private bool _parentWasSet;
        private bool _disposed;

        /// <summary>
        /// Erstellt eine neue Instanz zur Verwaltung von Parent-Child-Beziehungen.
        /// </summary>
        /// <param name="dataStoreProvider">Provider zum Abrufen des DataStores für Child-Elemente.</param>
        /// <exception cref="ArgumentNullException">Wenn <paramref name="dataStoreProvider"/> null ist.</exception>
        /// <remarks>
        /// Nach der Konstruktion müssen <see cref="Parent"/> und <see cref="IsChildFilter"/> gesetzt werden,
        /// damit die Synchronisation funktioniert.
        /// </remarks>
        public ParentChildRelationship(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider ?? throw new ArgumentNullException(nameof(dataStoreProvider));
            
            // Nicht-Singleton InMemoryDataStore für gefilterte Child-Elemente
            _childStore = _dataStoreProvider.GetInMemory<TChild>(isSingleton: false);

            // PropertyChangedBinder für effiziente PropertyChanged-Verwaltung
            _propertyChangedBinder = new PropertyChangedBinder<TChild>(
                enabled: true,
                onEntityChanged: OnChildPropertyChanged);
        }

        /// <summary>
        /// Das Parent-Objekt der Beziehung.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Wenn versucht wird, Parent nach der ersten Zuweisung erneut zu setzen.
        /// </exception>
        /// <remarks>
        /// <para>
        /// <b>Wichtig:</b> Das Parent-Objekt kann nur einmal gesetzt werden und ist danach unveränderlich.
        /// Dies entspricht dem Anwendungsfall, dass eine Relationship-Instanz für ein spezifisches
        /// Parent-Objekt erstellt wird.
        /// </para>
        /// <para>
        /// Beim Setzen wird automatisch eine Neusynchronisation der <see cref="Childs"/>-Collection ausgelöst.
        /// </para>
        /// </remarks>
        public TParent? Parent
        {
            get => _parent;
            set
            {
                if (_parentWasSet)
                {
                    throw new InvalidOperationException(
                        "Das Parent-Objekt kann nur einmal gesetzt werden. " +
                        "Um ein anderes Parent zu verwenden, erstellen Sie eine neue ParentChildRelationship-Instanz.");
                }

                _parent = value;
                _parentWasSet = true;

                // Childs neu synchronisieren
                SynchronizeChilds();
            }
        }

        /// <summary>
        /// Filter-Funktion zur Bestimmung, ob ein Child-Element zum aktuellen Parent gehört.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Die Funktion erhält das Parent-Objekt und ein Child-Element und gibt <c>true</c> zurück,
        /// wenn das Child zum Parent gehört.
        /// </para>
        /// <para>
        /// <b>Beispiel:</b>
        /// <code>
        /// IsChildFilter = (customer, order) => order.CustomerId == customer.Id;
        /// </code>
        /// </para>
        /// <para>
        /// Beim Setzen wird automatisch eine Neusynchronisation der <see cref="Childs"/>-Collection ausgelöst.
        /// </para>
        /// </remarks>
        public Func<TParent, TChild, bool>? IsChildFilter
        {
            get => _isChildFilter;
            set
            {
                if (_isChildFilter == value)
                    return;

                _isChildFilter = value;
                SynchronizeChilds();
            }
        }

        /// <summary>
        /// DataSource für alle potentiellen Child-Elemente.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Lazy Initialization:</b> Wenn nicht explizit gesetzt, wird beim ersten Zugriff
        /// automatisch <c>provider.GetDataStore&lt;TChild&gt;()</c> aufgerufen.
        /// </para>
        /// <para>
        /// <b>Explizite Zuweisung:</b> Sie können auch manuell einen DataStore zuweisen,
        /// z.B. einen nicht-Singleton-Store oder einen gefilterten Store.
        /// </para>
        /// <para>
        /// Beim Setzen werden alte Event-Subscriptions entfernt und neue erstellt.
        /// Die <see cref="Childs"/>-Collection wird automatisch neu synchronisiert.
        /// </para>
        /// </remarks>
        public IDataStore<TChild>? DataSource
        {
            get
            {
                // Lazy Initialization: Beim ersten Zugriff automatisch laden
                if (_dataSource == null)
                {
                    try
                    {
                        _dataSource = _dataStoreProvider.GetDataStore<TChild>();
                        SubscribeToDataSource();
                        SynchronizeChilds();
                    }
                    catch (InvalidOperationException)
                    {
                        // DataStore wurde noch nicht registriert - das ist OK
                        // Die Synchronisation erfolgt, sobald DataSource gesetzt wird
                    }
                }
                return _dataSource;
            }
            set
            {
                if (_dataSource == value)
                    return;

                // Alte DataSource: Events abmelden und PropertyChanged-Bindings entfernen
                UnsubscribeFromDataSource();

                _dataSource = value;

                // Neue DataSource: Events anmelden und neu synchronisieren
                if (_dataSource != null)
                {
                    SubscribeToDataSource();
                    SynchronizeChilds();
                }
                else
                {
                    // DataSource wurde auf null gesetzt - Childs leeren
                    _childStore.Clear();
                }
            }
        }

        /// <summary>
        /// Schreibgeschützte Auflistung der gefilterten Child-Elemente.
        /// </summary>
        /// <remarks>
        /// Diese Collection wird automatisch synchronisiert bei Änderungen in <see cref="DataSource"/>
        /// oder Properties von DataSource-Items.
        /// </remarks>
        public InMemoryDataStore<TChild> Childs => _childStore;

        /// <summary>
        /// Abonniert CollectionChanged-Events der DataSource und bindet PropertyChanged für existierende Items.
        /// </summary>
        private void SubscribeToDataSource()
        {
            if (_dataSource == null)
                return;

            // CollectionChanged-Event auf DataSource (Add, Remove, Clear, etc.)
            if (_dataSource.Items is INotifyCollectionChanged collectionChanged)
            {
                collectionChanged.CollectionChanged += OnDataSourceCollectionChanged;
            }

            // PropertyChanged-Binding für ALLE Items in DataSource
            // (nicht nur für gefilterte Items, da sich Items durch Property-Änderung qualifizieren können)
            _propertyChangedBinder.AttachRange(_dataSource.Items);
        }

        /// <summary>
        /// Entfernt Event-Subscriptions von der aktuellen DataSource.
        /// </summary>
        private void UnsubscribeFromDataSource()
        {
            if (_dataSource == null)
                return;

            // CollectionChanged-Event abmelden
            if (_dataSource.Items is INotifyCollectionChanged collectionChanged)
            {
                collectionChanged.CollectionChanged -= OnDataSourceCollectionChanged;
            }

            // Alle PropertyChanged-Bindings entfernen
            _propertyChangedBinder.DetachAll();
        }

        /// <summary>
        /// Behandelt Änderungen in der DataSource-Collection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Add:</b> Neue Items werden auf PropertyChanged überwacht und gefiltert.
        /// Nur passende Items werden zu Childs hinzugefügt.
        /// </para>
        /// <para>
        /// <b>Remove:</b> Entfernte Items werden von PropertyChanged entbunden und aus Childs entfernt.
        /// </para>
        /// <para>
        /// <b>Reset:</b> Komplette Neusynchronisation - alle Items neu filtern.
        /// </para>
        /// </remarks>
        private void OnDataSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        var newChilds = e.NewItems.Cast<TChild>().ToList();
                        
                        // PropertyChanged für neue Items binden
                        _propertyChangedBinder.AttachRange(newChilds);

                        // Nur Items hinzufügen, die den Filter erfüllen
                        foreach (var item in newChilds)
                        {
                            if (ShouldIncludeChild(item))
                            {
                                _childStore.Add(item);
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems.Cast<TChild>())
                        {
                            // PropertyChanged entbinden
                            _propertyChangedBinder.Detach(item);
                            
                            // Aus Childs entfernen (falls vorhanden)
                            _childStore.Remove(item);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    // Alte Items entbinden und entfernen
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems.Cast<TChild>())
                        {
                            _propertyChangedBinder.Detach(item);
                            _childStore.Remove(item);
                        }
                    }
                    
                    // Neue Items binden und filtern
                    if (e.NewItems != null)
                    {
                        var newChilds = e.NewItems.Cast<TChild>().ToList();
                        _propertyChangedBinder.AttachRange(newChilds);

                        foreach (var item in newChilds)
                        {
                            if (ShouldIncludeChild(item))
                            {
                                _childStore.Add(item);
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    // Komplette Neusynchronisation
                    _propertyChangedBinder.DetachAll();
                    SynchronizeChilds();
                    break;
            }
        }

        /// <summary>
        /// Behandelt PropertyChanged-Events von Child-Items in der DataSource.
        /// </summary>
        /// <remarks>
        /// Wird vom <see cref="PropertyChangedBinder{T}"/> aufgerufen, wenn sich eine Property
        /// eines Child-Items ändert (z.B. ForeignKey). Prüft, ob das Item zum Parent gehört
        /// oder entfernt werden muss.
        /// </remarks>
        private void OnChildPropertyChanged(TChild item)
        {
            var shouldInclude = ShouldIncludeChild(item);
            var isIncluded = _childStore.Items.Contains(item);

            if (shouldInclude && !isIncluded)
            {
                // Item erfüllt jetzt den Filter ? zu Childs hinzufügen
                _childStore.Add(item);
            }
            else if (!shouldInclude && isIncluded)
            {
                // Item erfüllt Filter nicht mehr ? aus Childs entfernen
                _childStore.Remove(item);
            }
        }

        /// <summary>
        /// Synchronisiert die Childs-Collection basierend auf aktuellem Parent und Filter.
        /// </summary>
        private void SynchronizeChilds()
        {
            _childStore.Clear();

            // Trigger lazy loading of DataSource if not yet initialized
            var dataSource = DataSource;
            
            if (dataSource == null || _parent == null || _isChildFilter == null)
                return;

            // PropertyChanged-Binding für ALLE Items in DataSource sicherstellen
            _propertyChangedBinder.AttachRange(dataSource.Items);
            
            // Nur passende Childs zur Child-Collection hinzufügen
            var matchingChilds = dataSource.Items.Where(child => _isChildFilter(_parent, child));
            _childStore.AddRange(matchingChilds);
        }

        /// <summary>
        /// Prüft, ob ein Child-Element zum aktuellen Parent gehört.
        /// </summary>
        private bool ShouldIncludeChild(TChild child)
        {
            if (_parent == null || _isChildFilter == null)
                return false;

            return _isChildFilter(_parent, child);
        }

        /// <summary>
        /// Gibt alle Ressourcen frei und entfernt Event-Subscriptions.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            // DataSource-Events und PropertyChanged-Bindings entfernen
            UnsubscribeFromDataSource();

            // PropertyChangedBinder disposed automatisch alle verbleibenden Bindings
            _propertyChangedBinder.Dispose();

            _childStore.Clear();
        }
    }
}
