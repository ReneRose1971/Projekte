# DataToolKit — API-Referenz

Vollständige alphabetisch sortierte API-Dokumentation für alle öffentlichen Schnittstellen und Klassen.

## ?? Inhaltsverzeichnis

- [DataStoreProvider](#datastoreprovider)
- [DataStoreSyncExtensions](#datastoresyncextensions)
- [DataToolKitServiceModule](#datatoolkitservicemodule)
- [EntityBase](#entitybase)
- [IDataStoreProvider](#idatastoreprovider)
- [IEntity](#ientity)
- [IRepository\<T\>](#irepositoryt)
- [IRepositoryBase\<T\>](#irepositorybaset)
- [IRepositoryFactory](#irepositoryfactory)
- [IStorageOptions\<T\>](#istorageoptionst)
- [InMemoryDataStore\<T\>](#inmemorydatastoret)
- [JsonRepository\<T\>](#jsonrepositoryt)
- [JsonStorageOptions\<T\>](#jsonstorageoptionst)
- [LiteDbRepository\<T\>](#litedbrepositoryt)
- [LiteDbStorageOptions\<T\>](#litedbstorageoptionst)
- [ParentChildRelationship\<TParent, TChild\>](#parentchildrelationshiptparent-tchild)
- [PersistentDataStore\<T\>](#persistentdatastoret)
- [PropertyChangedBinder\<T\>](#propertychangedbindert)
- [RepositoryRegistrationExtensions](#repositoryregistrationextensions)

---

## DataStoreSyncExtensions

**Namespace:** `DataToolKit.Storage.Extensions`  
**Assembly:** DataToolKit.dll  
**Typ:** Static Extension Class

### Beschreibung

Extension-Methoden für vollständige Collection-Synchronisation zwischen DataStores mit CollectionChanged-Tracking.

### Methoden

#### `IDisposable SyncWith<T>(this IDataStore<T> target, IDataStore<T> source, IEqualityComparer<T>? comparer = null) where T : class`

Synchronisiert einen Target-DataStore vollständig mit einem Source-DataStore.

**Typ-Parameter:**
- `T` — Elementtyp (class constraint).

**Parameter:**
- `target` (`IDataStore<T>`): Ziel-DataStore, der synchronisiert werden soll.
- `source` (`IDataStore<T>`): Quell-DataStore, dessen Änderungen überwacht werden.
- `comparer` (`IEqualityComparer<T>?`): Optionaler Comparer zur Identifikation von Elementen. Standard: `EqualityComparer<T>.Default`.

**Rückgabewert:**
- `IDisposable` — Subscription-Objekt. Dispose stoppt die Synchronisation.

**Funktionsweise:**

1. **Initiale Synchronisation**: Alle Items aus Source werden zu Target hinzugefügt
2. **Laufende Synchronisation**: CollectionChanged-Events werden überwacht
   - **Add** in Source ? Add in Target
   - **Remove** in Source ? Remove in Target
   - **Replace** in Source ? Remove old + Add new in Target
   - **Reset** in Source ? Clear + AddRange in Target

**Verwendungsbeispiel:**

```csharp
using DataToolKit.Storage.Extensions;
using DataToolKit.Storage.DataStores;

// Backup-DataStore synchronisieren mit Main-DataStore
var mainStore = new InMemoryDataStore<Customer>();
var backupStore = new InMemoryDataStore<Customer>();

using var sync = backupStore.SyncWith(mainStore);

// Änderungen in mainStore werden automatisch zu backupStore übertragen
mainStore.Add(new Customer { Id = 1, Name = "Alice" });
// ? backupStore enthält jetzt auch Alice

mainStore.Remove(customer);
// ? Wird auch aus backupStore entfernt

// Dispose beendet Synchronisation
sync.Dispose();
```

**Thread-Safety:**
- ?? **Nicht thread-safe**: Beide DataStores sollten vom gleichen Thread verwendet werden.

**Exceptions:**
- `ArgumentNullException`: Wenn `target` oder `source` null ist.

### Siehe auch

- [ParentChildRelationship](#parentchildrelationshiptparent-tchild) — Nutzt ähnliche Synchronisations-Mechanismen
- [PropertyChangedBinder](#propertychangedbindert) — PropertyChanged-Tracking
- [InMemoryDataStore](#inmemorydatastoret) — Synchronisierbare DataStores

---

## ParentChildRelationship\<TParent, TChild\>

**Namespace:** `DataToolKit.Relationships`  
**Assembly:** DataToolKit.dll  
**Implementiert:** `IDisposable`

### Beschreibung

Verwaltet automatisch 1:n-Beziehungen zwischen einem Parent-Objekt und einer gefilterten Collection von Child-Elementen mit vollständiger PropertyChanged- und CollectionChanged-Synchronisation.

### Konstruktor

#### `ParentChildRelationship(IDataStoreProvider dataStoreProvider)`

Erstellt eine neue Instanz zur Verwaltung von Parent-Child-Beziehungen.

**Parameter:**
- `dataStoreProvider` (`IDataStoreProvider`): Provider zum Abrufen des DataStores für Child-Elemente (aus DI).

**Exceptions:**
- `ArgumentNullException`: Wenn `dataStoreProvider` null ist.

### Properties

#### `TParent? Parent { get; set; }`

Das Parent-Objekt der Beziehung.

**Einschränkungen:**
- ? Kann **nur einmal** gesetzt werden (immutable nach erstem Set)
- ? Kann auf `null` gesetzt werden
- ? Exception beim zweiten Setzen: `InvalidOperationException`

**Verhalten:**
- Beim Setzen wird automatisch `SynchronizeChilds()` aufgerufen

#### `Func<TParent, TChild, bool>? IsChildFilter { get; set; }`

Filter-Funktion zur Bestimmung, ob ein Child-Element zum aktuellen Parent gehört.

**Einschränkungen:**
- ? Kann **mehrfach** geändert werden
- ? Triggert automatische Re-Synchronisation

**Beispiel:**
```csharp
IsChildFilter = (customer, order) => order.CustomerId == customer.Id;
```

#### `IDataStore<TChild>? DataSource { get; set; }`

DataSource für alle potentiellen Child-Elemente.

**Lazy Initialization:**
- Beim ersten Zugriff wird automatisch `provider.GetDataStore<TChild>()` aufgerufen

**Explizite Zuweisung:**
- Alte Subscriptions werden entfernt
- Neue Subscriptions werden erstellt
- Childs wird neu synchronisiert

#### `InMemoryDataStore<TChild> Childs { get; }`

Schreibgeschützte Auflistung der gefilterten Child-Elemente.

**UI-Binding:**
```csharp
DataGrid.ItemsSource = relationship.Childs.Items;
```

### Methoden

#### `void Dispose()`

Gibt alle Ressourcen frei und entfernt Event-Subscriptions.

**Beim Dispose:**
1. CollectionChanged-Event von DataSource wird abgemeldet
2. PropertyChangedBinder wird disposed (alle Bindings entfernt)
3. Childs-Collection wird geleert

### Verwendungsbeispiel

```csharp
using DataToolKit.Relationships;
using DataToolKit.Abstractions.DataStores;

public class CustomerDetailViewModel : IDisposable
{
    private readonly ParentChildRelationship<Customer, Order> _orderRelationship;

    public CustomerDetailViewModel(
        IDataStoreProvider provider,
        Customer customer)
    {
        // Relationship erstellen
        _orderRelationship = new ParentChildRelationship<Customer, Order>(provider)
        {
            Parent = customer,
            IsChildFilter = (c, o) => o.CustomerId == c.Id
        };
    }

    // UI bindet an diese Collection
    public ReadOnlyObservableCollection<Order> Orders 
        => _orderRelationship.Childs.Items;

    public void Dispose() => _orderRelationship.Dispose();
}
```

### Automatische Synchronisation

**1. CollectionChanged (DataSource):**
```csharp
// Neue Order hinzufügen
orderStore.Add(new Order { CustomerId = customer.Id });
// ? Wird automatisch zu Childs hinzugefügt (Filter matched)
```

**2. PropertyChanged (Items in DataSource):**
```csharp
// ForeignKey ändern
order.CustomerId = otherCustomerId;
// ? Wird automatisch aus Childs entfernt (Filter nicht mehr erfüllt)
```

**3. Remove-Operations:**
```csharp
orderStore.Remove(order);
// ? Wird automatisch aus Childs entfernt
```

**4. Clear-Operations:**
```csharp
orderStore.Clear();
// ? Childs wird automatisch geleert
```

### Siehe auch

- [ParentChildRelationship Dokumentation](ParentChildRelationship.md) — Vollständiger Leitfaden
- [PropertyChangedBinder](#propertychangedbindert) — Intern für PropertyChanged-Tracking
- [DataStoreSyncExtensions](#datastoresyncextensions) — Collection-Synchronisation
- [IDataStoreProvider](#idatastoreprovider) — DataStore-Management

---

## PropertyChangedBinder\<T\>

**Namespace:** `DataToolKit.Storage.Persistence`  
**Assembly:** DataToolKit.dll  
**Implementiert:** `IDisposable`

### Beschreibung

Bindet einmalig pro Entität an `INotifyPropertyChanged` und ruft beim Eintreten die angegebene Aktion auf. Doppelbindungen werden zuverlässig verhindert (idempotentes Attach).

### Konstruktor

#### `PropertyChangedBinder(bool enabled, Action<T> onEntityChanged)`

Erstellt einen PropertyChangedBinder.

**Parameter:**
- `enabled` (`bool`): Wenn `false`, werden alle Operationen übersprungen.
- `onEntityChanged` (`Action<T>`): Callback, der bei PropertyChanged aufgerufen wird.

**Exceptions:**
- `ArgumentNullException`: Wenn `onEntityChanged` null ist.

### Modi

**1. Manueller Modus:**
- Explizite `Attach()`/`Detach()`-Aufrufe
- Volle Kontrolle über Bindings

**2. DataStore-Modus:**
- Via `AttachToDataStore()` automatische Synchronisation
- Reagiert auf CollectionChanged-Events

### Methoden (Manueller Modus)

#### `void Attach(T entity)`

Idempotentes Binden an PropertyChanged-Events einer Entität.

**Parameter:**
- `entity` (`T`): Die zu überwachende Entität.

**Verhalten:**
- Event-Handler wird vor dem Anfügen sicherheitshalber entfernt
- Dann genau einmal angefügt
- Dadurch sind Doppelbindungen ausgeschlossen

**Idempotenz:**
```csharp
var entity = new Customer { Id = 1 };

binder.Attach(entity);
binder.Attach(entity);  // ? Sicher, kein Duplikat
binder.Attach(entity);  // ? Sicher, kein Duplikat

entity.Name = "Changed";
// ? Callback wird nur EINMAL aufgerufen
```

#### `void AttachRange(IEnumerable<T> entities)`

Bindet mehrere Entitäten.

#### `void Detach(T entity)`

Entfernt die Bindung zu einer Entität.

#### `void DetachAll()`

Entfernt alle Bindings.

### Methoden (DataStore-Modus)

#### `IDisposable AttachToDataStore(IDataStore<T> dataStore)`

Bindet den Binder automatisch an einen DataStore.

**Parameter:**
- `dataStore` (`IDataStore<T>`): Der zu überwachende DataStore.

**Rückgabewert:**
- `IDisposable` — Subscription-Objekt. Dispose beendet DataStore-Bindung.

**Im DataStore-Modus:**
- ? Alle existierenden Items werden gebunden
- ? Neue Items bei CollectionChanged.Add werden gebunden
- ? Entfernte Items bei CollectionChanged.Remove werden entbunden
- ? Bei CollectionChanged.Reset werden alle Items neu gebunden

**Verwendungsbeispiel:**

```csharp
using DataToolKit.Storage.Persistence;
using DataToolKit.Storage.DataStores;

// Manueller Modus
var binder = new PropertyChangedBinder<Customer>(
    enabled: true,
    onEntityChanged: customer => 
    {
        Console.WriteLine($"Customer {customer.Id} changed");
        repository.Update(customer);
    });

var customer = new Customer { Id = 1, Name = "Alice" };
binder.Attach(customer);

customer.Name = "Bob";  // ? Callback wird aufgerufen

binder.Detach(customer);

// DataStore-Modus (empfohlen)
var store = new InMemoryDataStore<Customer>();
using var subscription = binder.AttachToDataStore(store);

store.Add(new Customer { Id = 2, Name = "Charlie" });
// ? Automatisch gebunden, PropertyChanged wird überwacht
```

### Dispose

```csharp
public void Dispose()
```

Entfernt alle Bindings und DataStore-Subscriptions.

### Siehe auch

- [PropertyChangedBinder Dokumentation](PropertyChangedBinder.md) — Vollständiger Leitfaden
- [PersistentDataStore](#persistentdatastoret) — Nutzt PropertyChangedBinder
- [ParentChildRelationship](#parentchildrelationshiptparent-tchild) — Nutzt PropertyChangedBinder im DataStore-Modus

---

## Weitere Komponenten

Die vollständige API-Dokumentation für alle anderen Komponenten finden Sie in den jeweiligen Guide-Dokumenten:

- **[DataStoreProvider](DataStore-Provider.md)** — Thread-Safe Provider mit AutoLoad
- **[EntityBase](Repositories.md#entity-base)** — Basisklasse für Entitäten
- **[Repositories](Repositories.md)** — JSON und LiteDB Repository
- **[Storage Options](Storage-Options.md)** — Konfiguration für Speicherorte
- **[InMemoryDataStore](DataStore-Provider.md#inmemorydatastore)** — In-Memory Collection
- **[PersistentDataStore](DataStore-Provider.md#persistentdatastore)** — Persistenter DataStore

---

## Verwandte Dokumentation

- ?? [Storage Options](Storage-Options.md) - Wo und wie Daten gespeichert werden
- ?? [Repositories](Repositories.md) - JSON vs. LiteDB Repository
- ?? [DataStore Provider](DataStore-Provider.md) - Thread-Safe Singleton-Management
- ?? [PropertyChangedBinder](PropertyChangedBinder.md) - Automatisches PropertyChanged-Event-Tracking
- ?? [ParentChildRelationship](ParentChildRelationship.md) - 1:n-Beziehungsverwaltung
- ?? [Common.Bootstrap API-Referenz](../../Common.BootStrap/Docs/API-Referenz.md)

---

## Lizenz & Repository

- **Repository**: [https://github.com/ReneRose1971/Libraries](https://github.com/ReneRose1971/Libraries)
- **Lizenz**: Siehe LICENSE-Datei im Repository
