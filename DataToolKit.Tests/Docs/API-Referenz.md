# API-Referenz - DataToolKit.Tests

Vollständige API-Dokumentation aller Test-Komponenten und Fake-Implementierungen.

---

## ?? Namespaces

- [DataToolKit.Tests.Fakes.Repositories](#repositories)
- [DataToolKit.Tests.Fakes.Providers](#providers)
- [DataToolKit.Tests.Fakes.Builders](#builders-helpers)

---

## Repositories

### FakeJsonRepository<T>

**Namespace:** `DataToolKit.Tests.Fakes.Repositories`

In-Memory Fake für JsonRepository ohne Dateisystem-Zugriff.

#### Typ-Parameter

| Parameter | Constraints | Beschreibung |
|-----------|-------------|--------------|
| `T` | `class` | Entitätstyp |

#### Konstruktor

```csharp
public FakeJsonRepository()
```

Erstellt eine neue Instanz ohne Parameter.

#### Properties

| Property | Typ | Beschreibung |
|----------|-----|--------------|
| `ThrowOnLoad` | `bool` | Simuliert Load-Fehler, wenn `true` |
| `ThrowOnWrite` | `bool` | Simuliert Write-Fehler, wenn `true` |
| `SimulatedDelay` | `TimeSpan?` | Simuliert verzögerte Operationen |
| `History` | `IReadOnlyList<RepositoryOperation>` | Historie aller Repository-Operationen |
| `LoadCallCount` | `int` | Anzahl der Load-Aufrufe |
| `WriteCallCount` | `int` | Anzahl der Write-Aufrufe |

#### Methoden

##### Load()

```csharp
public IReadOnlyList<T> Load()
```

Lädt alle Elemente aus dem In-Memory-Store.

**Returns:** Schreibgeschützte Liste aller Elemente.

**Exceptions:**
- `IOException` - Wenn `ThrowOnLoad` = `true`

**Beispiel:**
```csharp
var repo = new FakeJsonRepository<Customer>();
repo.SeedData(new Customer { Id = 1, Name = "Alice" });

var items = repo.Load();
Assert.Single(items);
```

##### Write(IEnumerable<T>)

```csharp
public void Write(IEnumerable<T> items)
```

Schreibt alle Elemente in den In-Memory-Store (ersetzt bestehende Daten).

**Parameter:**
- `items` - Collection von Entitäten

**Exceptions:**
- `ArgumentNullException` - Wenn `items` `null` ist
- `ArgumentException` - Wenn Collection `null`-Elemente enthält
- `IOException` - Wenn `ThrowOnWrite` = `true`

**Beispiel:**
```csharp
var repo = new FakeJsonRepository<Customer>();
repo.Write(new[] {
    new Customer { Id = 1, Name = "Alice" },
    new Customer { Id = 2, Name = "Bob" }
});

Assert.Equal(1, repo.WriteCallCount);
```

##### Clear()

```csharp
public void Clear()
```

Leert den In-Memory-Store.

**Beispiel:**
```csharp
var repo = new FakeJsonRepository<Customer>();
repo.Write(customers);
repo.Clear();

Assert.Empty(repo.Load());
```

##### Reset()

```csharp
public void Reset()
```

Setzt das Repository in den Ausgangszustand zurück (leert Daten, History und Flags).

**Beispiel:**
```csharp
var repo = new FakeJsonRepository<Customer>();
repo.ThrowOnLoad = true;
repo.Write(customers);

repo.Reset();

Assert.False(repo.ThrowOnLoad);
Assert.Empty(repo.History);
```

##### SeedData(params T[])

```csharp
public void SeedData(params T[] items)
```

Füllt das Repository mit Test-Daten **ohne** History-Eintrag.

**Parameter:**
- `items` - Variable Anzahl von Entitäten

**Beispiel:**
```csharp
var repo = new FakeJsonRepository<Customer>();
repo.SeedData(
    new Customer { Id = 1, Name = "Alice" },
    new Customer { Id = 2, Name = "Bob" }
);

Assert.Equal(2, repo.Load().Count);
Assert.Equal(1, repo.History.Count); // Nur Load
```

---

### FakeLiteDbRepository<T>

**Namespace:** `DataToolKit.Tests.Fakes.Repositories`

In-Memory Fake für LiteDbRepository mit automatischer ID-Vergabe und Delta-Synchronisierung.

#### Typ-Parameter

| Parameter | Constraints | Beschreibung |
|-----------|-------------|--------------|
| `T` | `EntityBase` | Entitätstyp (muss von EntityBase erben) |

#### Konstruktor

```csharp
public FakeLiteDbRepository(IEqualityComparer<T>? comparer = null)
```

**Parameter:**
- `comparer` - Optionaler EqualityComparer für Delta-Erkennung (default: `FallbackEqualsComparer<T>`)

#### Properties

| Property | Typ | Beschreibung |
|----------|-----|--------------|
| `ThrowOnWrite` | `bool` | Simuliert Write-Fehler, wenn `true` |
| `ThrowOnUpdate` | `bool` | Simuliert Update-Fehler, wenn `true` |
| `ThrowOnDelete` | `bool` | Simuliert Delete-Fehler, wenn `true` |
| `History` | `IReadOnlyList<RepositoryOperation>` | Historie aller Repository-Operationen |
| `CurrentMaxId` | `int` | Aktuelle maximale ID |
| `WriteCallCount` | `int` | Anzahl der Write-Aufrufe |
| `LoadCallCount` | `int` | Anzahl der Load-Aufrufe |

#### Methoden

##### Load()

```csharp
public IReadOnlyList<T> Load()
```

Lädt alle Elemente aus dem In-Memory-Store.

**Returns:** Schreibgeschützte Liste aller Elemente.

##### Write(IEnumerable<T>)

```csharp
public void Write(IEnumerable<T> items)
```

Schreibt Elemente mit Delta-Synchronisierung (Insert/Update/Delete).

**Parameter:**
- `items` - Ziel-Collection

**Delta-Logik:**
- **Update:** Entitäten mit gleicher ID, aber unterschiedlichen Werten
- **Delete:** Entitäten, die nur in der Datenbank existieren
- **Insert:** Entitäten, die nur in der neuen Collection existieren (mit Auto-ID für Id=0)

**Exceptions:**
- `ArgumentNullException` - Wenn `items` `null` ist
- `InvalidOperationException` - Wenn `ThrowOnWrite` = `true`

**Beispiel:**
```csharp
var repo = new FakeLiteDbRepository<Customer>();

// Initial: A, B, C
repo.Write(new[] { alice, bob, charlie });

// Delta: A ändern, C löschen, D hinzufügen
alice.Value = 150;
repo.Write(new[] { alice, bob, dave });

var loaded = repo.Load();
Assert.Equal(3, loaded.Count);
Assert.Contains(loaded, e => e.Name == "Dave");
Assert.DoesNotContain(loaded, e => e.Name == "Charlie");
```

##### Update(T)

```csharp
public int Update(T item)
```

Aktualisiert eine einzelne Entität.

**Parameter:**
- `item` - Zu aktualisierende Entität

**Returns:** `1` bei Erfolg.

**Exceptions:**
- `ArgumentNullException` - Wenn `item` `null` ist
- `ArgumentException` - Wenn `item.Id <= 0`
- `InvalidOperationException` - Wenn Entität nicht gefunden oder `ThrowOnUpdate` = `true`

**Beispiel:**
```csharp
var repo = new FakeLiteDbRepository<Customer>();
repo.Write(new[] { new Customer { Id = 0, Name = "Alice" } });

var alice = repo.Load().First();
alice.Name = "Alice Updated";
repo.Update(alice);

Assert.Equal("Alice Updated", repo.Load().First().Name);
```

##### Delete(T)

```csharp
public int Delete(T item)
```

Löscht eine einzelne Entität.

**Parameter:**
- `item` - Zu löschende Entität

**Returns:** `1` bei Erfolg, `0` wenn `item.Id <= 0`.

**Exceptions:**
- `ArgumentNullException` - Wenn `item` `null` ist
- `InvalidOperationException` - Wenn Entität nicht gefunden oder `ThrowOnDelete` = `true`

**Beispiel:**
```csharp
var repo = new FakeLiteDbRepository<Customer>();
repo.Write(new[] { new Customer { Id = 0, Name = "Alice" } });

var alice = repo.Load().First();
repo.Delete(alice);

Assert.Empty(repo.Load());
```

##### Clear()

```csharp
public void Clear()
```

Leert den In-Memory-Store.

##### Reset()

```csharp
public void Reset()
```

Setzt das Repository in den Ausgangszustand zurück (leert Daten, History, NextId und Flags).

##### SeedData(params T[])

```csharp
public void SeedData(params T[] items)
```

Füllt das Repository mit Test-Daten. Entities mit `Id = 0` erhalten automatisch IDs.

**Parameter:**
- `items` - Variable Anzahl von Entitäten

**Beispiel:**
```csharp
var repo = new FakeLiteDbRepository<Customer>();
repo.SeedData(
    new Customer { Id = 0, Name = "Alice" },  // Bekommt ID 1
    new Customer { Id = 0, Name = "Bob" },    // Bekommt ID 2
    new Customer { Id = 5, Name = "Charlie" } // Behält ID 5
);

Assert.Equal(3, repo.CurrentMaxId);
```

##### GetById(int)

```csharp
public T? GetById(int id)
```

Gibt eine Entität anhand ihrer ID zurück.

**Parameter:**
- `id` - ID der gesuchten Entität

**Returns:** Entität oder `null`, wenn nicht gefunden.

**Beispiel:**
```csharp
var repo = new FakeLiteDbRepository<Customer>();
repo.SeedData(new Customer { Id = 1, Name = "Alice" });

var alice = repo.GetById(1);
Assert.NotNull(alice);
Assert.Equal("Alice", alice.Name);

var notFound = repo.GetById(999);
Assert.Null(notFound);
```

---

### FakeRepositoryFactory

**Namespace:** `DataToolKit.Tests.Fakes.Repositories`

Factory für Fake-Repositories mit zentralem Reset und Inspection.

#### Konstruktor

```csharp
public FakeRepositoryFactory()
```

#### Methoden

##### GetJsonRepository<T>()

```csharp
public IRepositoryBase<T> GetJsonRepository<T>()
```

Gibt das Fake JSON-Repository für den Typ T zurück (Singleton pro Typ).

**Returns:** `IRepositoryBase<T>`

**Beispiel:**
```csharp
var factory = new FakeRepositoryFactory();
var repo1 = factory.GetJsonRepository<Customer>();
var repo2 = factory.GetJsonRepository<Customer>();

Assert.Same(repo1, repo2); // Gleiche Instanz
```

##### GetLiteDbRepository<T>()

```csharp
public IRepository<T> GetLiteDbRepository<T>() where T : class
```

Gibt das Fake LiteDB-Repository für den Typ T zurück (Singleton pro Typ).

**Returns:** `IRepository<T>`

##### GetFakeJsonRepository<T>()

```csharp
public FakeJsonRepository<T> GetFakeJsonRepository<T>() where T : class
```

Gibt das typisierte Fake JSON-Repository für Test-Assertions zurück.

**Returns:** `FakeJsonRepository<T>`

**Beispiel:**
```csharp
var factory = new FakeRepositoryFactory();
var repo = factory.GetFakeJsonRepository<Customer>();

repo.Write(customers);

// Inspection
Assert.Equal(1, repo.WriteCallCount);
Assert.NotEmpty(repo.History);
```

##### GetFakeLiteDbRepository<T>()

```csharp
public FakeLiteDbRepository<T> GetFakeLiteDbRepository<T>() where T : EntityBase
```

Gibt das typisierte Fake LiteDB-Repository für Test-Assertions zurück.

**Returns:** `FakeLiteDbRepository<T>`

##### ResetAll()

```csharp
public void ResetAll()
```

Setzt alle Repositories in den Ausgangszustand zurück.

**Beispiel:**
```csharp
var factory = new FakeRepositoryFactory();
var repo1 = factory.GetFakeJsonRepository<Customer>();
var repo2 = factory.GetFakeLiteDbRepository<Order>();

repo1.Write(customers);
repo2.Write(orders);

factory.ResetAll();

Assert.Empty(repo1.Load());
Assert.Empty(repo2.Load());
Assert.Empty(repo1.History);
Assert.Empty(repo2.History);
```

##### ClearAll()

```csharp
public void ClearAll()
```

Entfernt alle Repositories aus dem Cache.

---

## Providers

### FakeDataStoreProvider

**Namespace:** `DataToolKit.Tests.Fakes.Providers`

Fake DataStoreProvider für Test-Szenarien ohne echte I/O.

#### Konstruktor

```csharp
public FakeDataStoreProvider(FakeRepositoryFactory? repositoryFactory = null)
```

**Parameter:**
- `repositoryFactory` - Optionale Repository-Factory (default: neue Instanz)

#### Properties

| Property | Typ | Beschreibung |
|----------|-----|--------------|
| `RepositoryFactory` | `FakeRepositoryFactory` | Zugriff auf Repository-Factory für Assertions |

#### Methoden

##### GetDataStore<T>()

```csharp
public IDataStore<T> GetDataStore<T>() where T : class
```

Gibt einen bereits registrierten DataStore zurück.

**Returns:** `IDataStore<T>`

**Exceptions:**
- `InvalidOperationException` - Wenn kein DataStore registriert

##### GetInMemory<T>()

```csharp
public InMemoryDataStore<T> GetInMemory<T>(
    bool isSingleton = true,
    IEqualityComparer<T>? comparer = null) where T : class
```

Gibt einen InMemoryDataStore zurück (Singleton oder neue Instanz).

**Parameter:**
- `isSingleton` - Singleton (true) oder neue Instanz (false)
- `comparer` - Optionaler EqualityComparer

**Returns:** `InMemoryDataStore<T>`

**Beispiel:**
```csharp
var provider = new FakeDataStoreProvider();
var store = provider.GetInMemory<Customer>(isSingleton: true);

store.Add(new Customer { Id = 1, Name = "Alice" });
Assert.Equal(1, store.Count);
```

##### GetPersistent<T>()

```csharp
public PersistentDataStore<T> GetPersistent<T>(
    IRepositoryFactory repositoryFactory,
    bool isSingleton = true,
    bool trackPropertyChanges = true,
    bool autoLoad = true) where T : class
```

Gibt einen PersistentDataStore zurück (mit Fake-Repository).

**Parameter:**
- `repositoryFactory` - Repository-Factory
- `isSingleton` - Singleton (true) oder neue Instanz (false)
- `trackPropertyChanges` - PropertyChanged-Tracking aktivieren
- `autoLoad` - Daten automatisch laden

**Returns:** `PersistentDataStore<T>`

**Repository-Auswahl:**
- `IEntity`-Typen ? LiteDB-Repository
- POCOs ? JSON-Repository

**Beispiel:**
```csharp
var provider = new FakeDataStoreProvider();
var store = provider.GetPersistent<Customer>(
    provider.RepositoryFactory,
    autoLoad: false
);

store.Add(new Customer { Id = 0, Name = "Alice" });

// Verify persistence
var repo = provider.RepositoryFactory.GetFakeLiteDbRepository<Customer>();
Assert.Single(repo.Load());
```

##### RemoveSingleton<T>()

```csharp
public bool RemoveSingleton<T>() where T : class
```

Entfernt eine Singleton-Instanz aus dem Cache und ruft Dispose() auf.

**Returns:** `true` wenn entfernt, `false` wenn nicht gefunden.

##### ClearAll()

```csharp
public void ClearAll()
```

Entfernt alle Singleton-Instanzen und setzt die Repository-Factory zurück.

**Beispiel:**
```csharp
var provider = new FakeDataStoreProvider();
var store = provider.GetInMemory<Customer>();

store.Add(new Customer { Id = 1, Name = "Alice" });

provider.ClearAll();

// Store ist disposed
Assert.Throws<InvalidOperationException>(() => provider.GetDataStore<Customer>());
```

---

## Builders & Helpers

### TestEntityBuilder<T>

**Namespace:** `DataToolKit.Tests.Fakes.Builders`

Fluent Builder für Test-Entities mit sinnvollen Defaults.

#### Typ-Parameter

| Parameter | Constraints | Beschreibung |
|-----------|-------------|--------------|
| `T` | `EntityBase, new()` | Entitätstyp |

#### Konstruktor

```csharp
public TestEntityBuilder()
```

#### Methoden

##### WithId(int)

```csharp
public TestEntityBuilder<T> WithId(int id)
```

Setzt die ID der Entität.

**Returns:** `this` (Fluent API)

##### With(Action<T>)

```csharp
public TestEntityBuilder<T> With(Action<T> configure)
```

Wendet eine benutzerdefinierte Konfiguration auf die Entität an.

**Returns:** `this` (Fluent API)

##### Build()

```csharp
public T Build()
```

Erstellt die konfigurierte Entität.

**Returns:** Konfigurierte Entität

**Beispiel:**
```csharp
var customer = new TestEntityBuilder<Customer>()
    .WithId(1)
    .With(c => c.Name = "Alice")
    .With(c => c.Email = "alice@test.com")
    .Build();

Assert.Equal(1, customer.Id);
Assert.Equal("Alice", customer.Name);
```

##### BuildMany(int)

```csharp
public List<T> BuildMany(int count)
```

Erstellt mehrere Kopien der konfigurierten Entität.

**Parameters:**
- `count` - Anzahl der Entities

**Returns:** Liste von Entities

##### BuildMany(int, Action<T, int>)

```csharp
public List<T> BuildMany(int count, Action<T, int> configureWithIndex)
```

Erstellt mehrere Entities mit Index-basierter Konfiguration.

**Parameters:**
- `count` - Anzahl der Entities
- `configureWithIndex` - Konfiguration mit Index-Parameter

**Returns:** Liste von Entities

**Beispiel:**
```csharp
var customers = new TestEntityBuilder<Customer>()
    .WithId(0)
    .BuildMany(10, (c, i) => {
        c.Name = $"Customer {i}";
        c.Email = $"customer{i}@test.com";
    });

Assert.Equal(10, customers.Count);
Assert.Equal("Customer 0", customers[0].Name);
```

---

### DataStoreTestFixture<T>

**Namespace:** `DataToolKit.Tests.Fakes.Builders`

xUnit-Fixture für DataStore-Tests mit automatischem Setup und Cleanup.

#### Typ-Parameter

| Parameter | Constraints | Beschreibung |
|-----------|-------------|--------------|
| `T` | `class` | Entitätstyp |

#### Konstruktor

```csharp
public DataStoreTestFixture(
    bool usePersistent = false,
    bool autoLoad = true,
    IEqualityComparer<T>? comparer = null)
```

**Parameters:**
- `usePersistent` - PersistentDataStore (true) oder InMemory (false)
- `autoLoad` - Daten automatisch laden (nur bei Persistent)
- `comparer` - Optionaler EqualityComparer (nur bei InMemory)

#### Properties

| Property | Typ | Beschreibung |
|----------|-----|--------------|
| `Provider` | `FakeDataStoreProvider` | Fake DataStoreProvider |
| `RepositoryFactory` | `FakeRepositoryFactory` | Fake RepositoryFactory |
| `DataStore` | `IDataStore<T>` | Konfigurierter DataStore |

#### Methoden

##### SeedData(params T[])

```csharp
public void SeedData(params T[] items)
```

Füllt den DataStore mit Test-Daten.

##### Reset()

```csharp
public void Reset()
```

Setzt den DataStore und alle Repositories zurück.

**Beispiel:**
```csharp
public class CustomerTests : IClassFixture<DataStoreTestFixture<Customer>>
{
    private readonly DataStoreTestFixture<Customer> _fixture;
    
    public CustomerTests(DataStoreTestFixture<Customer> fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public void Test1()
    {
        _fixture.Reset();
        _fixture.SeedData(new Customer { Id = 1, Name = "Alice" });
        
        Assert.Equal(1, _fixture.DataStore.Count);
    }
}
```

---

### RepositoryScenarioBuilder<T>

**Namespace:** `DataToolKit.Tests.Fakes.Builders`

Builder für komplexe Repository-Szenarien mit vorgefertigten Daten.

#### Typ-Parameter

| Parameter | Constraints | Beschreibung |
|-----------|-------------|--------------|
| `T` | `EntityBase, new()` | Entitätstyp |

#### Konstruktor

```csharp
public RepositoryScenarioBuilder(FakeRepositoryFactory factory)
```

#### Methoden

##### WithEntity(T)

```csharp
public RepositoryScenarioBuilder<T> WithEntity(T entity)
```

Fügt eine einzelne Entität zum Szenario hinzu.

##### WithEntities(params T[])

```csharp
public RepositoryScenarioBuilder<T> WithEntities(params T[] entities)
```

Fügt mehrere Entitäten zum Szenario hinzu.

##### WithRandomEntities(int, Func<int, T>)

```csharp
public RepositoryScenarioBuilder<T> WithRandomEntities(int count, Func<int, T> factory)
```

Fügt mehrere Entitäten mit einer Factory-Funktion hinzu.

##### BuildLiteDb()

```csharp
public IRepository<T> BuildLiteDb()
```

Erstellt ein LiteDB-Repository mit den konfigurierten Entities.

##### BuildJson()

```csharp
public IRepositoryBase<T> BuildJson()
```

Erstellt ein JSON-Repository mit den konfigurierten Entities.

**Beispiel:**
```csharp
var factory = new FakeRepositoryFactory();
var repo = new RepositoryScenarioBuilder<Customer>(factory)
    .WithEntity(new Customer { Id = 1, Name = "Alice" })
    .WithRandomEntities(99, i => new Customer 
    { 
        Id = 0, 
        Name = $"Customer {i}" 
    })
    .BuildLiteDb();

var loaded = repo.Load();
Assert.Equal(100, loaded.Count);
```

---

## ?? Hilfstypen

### RepositoryOperation

**Namespace:** `DataToolKit.Tests.Fakes.Repositories`

Record zur Repräsentation einer Repository-Operation.

```csharp
public record RepositoryOperation(string Action, DateTime Timestamp, int ItemCount);
```

**Properties:**
- `Action` - Name der Operation ("Load", "Write", "Clear", "Update", "Delete")
- `Timestamp` - Zeitpunkt der Operation (UTC)
- `ItemCount` - Anzahl betroffener Items

**Verwendung:**
```csharp
var repo = new FakeJsonRepository<Customer>();
repo.Write(customers);

var lastOp = repo.History.Last();
Assert.Equal("Write", lastOp.Action);
Assert.Equal(customers.Count, lastOp.ItemCount);
```

---

## ?? Siehe auch

- [Fake-Framework Guide](./Fake-Framework.md) - Einführung und Schnellstart
- [Test-Patterns](./Test-Patterns.md) - Best Practices und Patterns
- [DataToolKit API-Referenz](../../DataToolKit/Docs/API-Referenz.md) - Produktions-APIs
