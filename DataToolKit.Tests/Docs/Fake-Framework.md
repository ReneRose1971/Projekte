# Fake-Framework Guide

Einführung in das Fake-Framework für schnelle, isolierte Tests ohne I/O.

---

## ?? Überblick

Das **Fake-Framework** bietet In-Memory-Implementierungen aller wichtigen DataToolKit-Komponenten. Tests laufen in Millisekunden statt Sekunden, da keine echten Dateisystem- oder Datenbank-Operationen durchgeführt werden.

### Vorteile

? **Schnell** - Tests in Millisekunden  
? **Isoliert** - Keine externen Dependencies  
? **Konfigurierbar** - Fehler und Verzögerungen simulieren  
? **Inspection** - History-Tracking für Assertions  
? **Drop-in** - Gleiche Interfaces wie Produktions-Komponenten

---

## ?? Verfügbare Fakes

| Fake | Beschreibung | Use Case |
|------|--------------|----------|
| **FakeJsonRepository** | In-Memory JSON-Repository | Tests ohne Dateisystem |
| **FakeLiteDbRepository** | In-Memory LiteDB mit Auto-ID | Tests ohne Datenbank |
| **FakeRepositoryFactory** | Factory für Fakes | Multi-Repository-Tests |
| **FakeDataStoreProvider** | Provider mit Fakes | Integration-Tests |

---

## ?? Schnellstart

### 1. Einfacher Repository-Test

```csharp
using DataToolKit.Tests.Fakes.Repositories;
using Xunit;

public class RepositoryTests
{
    [Fact]
    public void Write_And_Load_Should_Roundtrip()
    {
        // Arrange
        var repo = new FakeJsonRepository<Customer>();
        var customers = new[]
        {
            new Customer { Id = 1, Name = "Alice" },
            new Customer { Id = 2, Name = "Bob" }
        };
        
        // Act
        repo.Write(customers);
        var loaded = repo.Load();
        
        // Assert
        Assert.Equal(2, loaded.Count);
        Assert.Equal(1, repo.WriteCallCount);
    }
}
```

### 2. Mit FakeDataStoreProvider

```csharp
using DataToolKit.Tests.Fakes.Providers;
using Xunit;

public class DataStoreTests
{
    [Fact]
    public void Add_Should_Persist()
    {
        // Arrange
        var provider = new FakeDataStoreProvider();
        var store = provider.GetPersistent<Customer>(
            provider.RepositoryFactory,
            autoLoad: false
        );
        
        // Act
        store.Add(new Customer { Id = 0, Name = "Alice" });
        
        // Assert
        Assert.Equal(1, store.Count);
        
        // Verify persistence
        var repo = provider.RepositoryFactory
            .GetFakeLiteDbRepository<Customer>();
        Assert.Single(repo.Load());
    }
}
```

### 3. Mit xUnit-Fixture

```csharp
using DataToolKit.Tests.Fakes.Builders;
using Xunit;

public class CustomerTests : IClassFixture<DataStoreTestFixture<Customer>>
{
    private readonly DataStoreTestFixture<Customer> _fixture;
    
    public CustomerTests(DataStoreTestFixture<Customer> fixture)
    {
        _fixture = fixture;
        _fixture.Reset(); // Jeder Test startet sauber
    }
    
    [Fact]
    public void Add_Should_Increase_Count()
    {
        // Arrange
        _fixture.SeedData(
            new Customer { Id = 1, Name = "Alice" }
        );
        
        // Act
        _fixture.DataStore.Add(new Customer { Id = 2, Name = "Bob" });
        
        // Assert
        Assert.Equal(2, _fixture.DataStore.Count);
    }
}
```

---

## ?? Features im Detail

### History-Tracking

Alle Fake-Repositories zeichnen ihre Operationen auf:

```csharp
var repo = new FakeJsonRepository<Customer>();

repo.Write(customers);
repo.Load();
repo.Clear();

// Assertions auf History
Assert.Equal(3, repo.History.Count);
Assert.Equal(1, repo.LoadCallCount);
Assert.Equal(1, repo.WriteCallCount);
Assert.Equal("Clear", repo.History.Last().Action);
```

### Fehler-Simulation

Simulieren Sie Fehler für Robustness-Tests:

```csharp
var repo = new FakeLiteDbRepository<Customer>();
repo.ThrowOnWrite = true;

var store = new PersistentDataStore<Customer>(repo);

// InvalidOperationException wird geworfen
Assert.Throws<InvalidOperationException>(
    () => store.Add(new Customer { Id = 0, Name = "Alice" })
);
```

### Verzögerungs-Simulation

Simulieren Sie langsame I/O-Operationen:

```csharp
var repo = new FakeJsonRepository<Customer>();
repo.SimulatedDelay = TimeSpan.FromMilliseconds(100);

var start = DateTime.UtcNow;
repo.Write(customers);
var elapsed = DateTime.UtcNow - start;

Assert.True(elapsed >= TimeSpan.FromMilliseconds(100));
```

### Automatische ID-Vergabe

FakeLiteDbRepository vergibt IDs automatisch:

```csharp
var repo = new FakeLiteDbRepository<Customer>();

var customer = new Customer { Id = 0, Name = "Alice" };
repo.Write(new[] { customer });

// ID wurde automatisch gesetzt
Assert.Equal(1, customer.Id);
Assert.Equal(1, repo.CurrentMaxId);
```

### Delta-Synchronisierung

FakeLiteDbRepository simuliert realistische Delta-Detection:

```csharp
var repo = new FakeLiteDbRepository<Customer>();

// Seed: Alice, Bob, Charlie
repo.Write(new[] { alice, bob, charlie });

// Update: Alice ändern, Charlie löschen, Dave hinzufügen
alice.Value = 150;
repo.Write(new[] { alice, bob, dave });

// Nur Delta wurde verarbeitet
var loaded = repo.Load();
Assert.Equal(3, loaded.Count);
Assert.Contains(loaded, e => e.Name == "Dave");
Assert.DoesNotContain(loaded, e => e.Name == "Charlie");
Assert.Equal(150, loaded.First(e => e.Name == "Alice").Value);
```

### SeedData ohne History

Füllen Sie Repositories mit Testdaten ohne History-Einträge:

```csharp
var repo = new FakeJsonRepository<Customer>();

repo.SeedData(
    new Customer { Id = 1, Name = "Alice" },
    new Customer { Id = 2, Name = "Bob" }
);

// Daten vorhanden, aber keine History
Assert.Equal(2, repo.Load().Count);
Assert.Equal(1, repo.History.Count); // Nur Load
```

---

## ?? Typische Szenarien

### Szenario 1: Repository-Verhalten testen

```csharp
[Fact]
public void Repository_Should_Handle_Duplicates()
{
    var repo = new FakeJsonRepository<Customer>();
    var customer = new Customer { Id = 1, Name = "Alice" };
    
    repo.Write(new[] { customer });
    repo.Write(new[] { customer }); // Duplikat
    
    Assert.Equal(2, repo.WriteCallCount);
    Assert.Equal(1, repo.Load().Count); // Nur eine Alice
}
```

### Szenario 2: PersistentDataStore-Integration

```csharp
[Fact]
public void PersistentDataStore_Should_Persist_On_Add()
{
    var provider = new FakeDataStoreProvider();
    var store = provider.GetPersistent<Customer>(
        provider.RepositoryFactory,
        autoLoad: false
    );
    
    store.Add(new Customer { Id = 0, Name = "Alice" });
    
    // Assert: Repository wurde aktualisiert
    var repo = provider.RepositoryFactory
        .GetFakeLiteDbRepository<Customer>();
    var persisted = repo.Load();
    
    Assert.Single(persisted);
    Assert.Equal("Alice", persisted.First().Name);
}
```

### Szenario 3: Fehlerbehandlung testen

```csharp
[Fact]
public void DataStore_Should_Handle_Repository_Failure()
{
    var provider = new FakeDataStoreProvider();
    var repo = provider.RepositoryFactory.GetFakeLiteDbRepository<Customer>();
    repo.ThrowOnWrite = true;
    
    var store = provider.GetPersistent<Customer>(
        provider.RepositoryFactory,
        autoLoad: false
    );
    
    var ex = Assert.Throws<InvalidOperationException>(
        () => store.Add(new Customer { Id = 0, Name = "Alice" })
    );
    Assert.Contains("Simulated write failure", ex.Message);
}
```

### Szenario 4: Komplexe Testdaten

```csharp
[Fact]
public void Test_Large_Dataset()
{
    var factory = new FakeRepositoryFactory();
    var repo = new RepositoryScenarioBuilder<Customer>(factory)
        .WithRandomEntities(1000, i => new Customer
        {
            Id = 0,
            Name = $"Customer {i}",
            Email = $"customer{i}@test.com"
        })
        .BuildFakeLiteDb();
    
    var loaded = repo.Load();
    
    Assert.Equal(1000, loaded.Count);
    Assert.All(loaded, c => Assert.True(c.Id > 0));
}
```

---

## ?? Best Practices

### 1. Fakes statt echte I/O

**? Empfohlen:**
```csharp
var provider = new FakeDataStoreProvider();
var store = provider.GetPersistent<Customer>(
    provider.RepositoryFactory,
    autoLoad: false
);
```

**? Vermeiden:**
```csharp
var repo = new JsonRepository<Customer>(options);
// Langsam, benötigt Cleanup
```

### 2. Builder für Testdaten

```csharp
// ? Mit Builder
var customers = new TestEntityBuilder<Customer>()
    .BuildMany(100, (c, i) => {
        c.Name = $"Customer {i}";
        c.Email = $"customer{i}@test.com";
    });

// ? Manuell
var customers = new List<Customer>();
for (int i = 0; i < 100; i++)
    customers.Add(new Customer { Name = $"Customer {i}" });
```

### 3. Fixtures für gemeinsames Setup

```csharp
public class TestsFixture : IDisposable
{
    public FakeDataStoreProvider Provider { get; }
    
    public TestsFixture()
    {
        Provider = new FakeDataStoreProvider();
    }
    
    public void Dispose() => Provider.ClearAll();
}

public class MyTests : IClassFixture<TestsFixture>
{
    private readonly TestsFixture _fixture;
    
    public MyTests(TestsFixture fixture)
    {
        _fixture = fixture;
    }
}
```

### 4. History-Tracking für Assertions

```csharp
var repo = provider.RepositoryFactory
    .GetFakeLiteDbRepository<Customer>();

store.Add(customer1);
store.Add(customer2);
store.Remove(customer1);

// Assertions
Assert.Equal(3, repo.History.Count);
Assert.Equal(2, repo.History.Count(h => h.Action == "Write"));
```

### 5. Reset zwischen Tests

```csharp
public class MyTests : IClassFixture<DataStoreTestFixture<Customer>>
{
    private readonly DataStoreTestFixture<Customer> _fixture;
    
    public MyTests(DataStoreTestFixture<Customer> fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public void Test1()
    {
        _fixture.Reset(); // ? Sauberer Start
        // ...
    }
    
    [Fact]
    public void Test2()
    {
        _fixture.Reset(); // ? Unabhängig von Test1
        // ...
    }
}
```

---

## ?? Beispiele

Vollständige Beispiel-Tests finden Sie unter:

- **FakeJsonRepository_Example_Tests.cs** - 8 Beispiele
- **FakeLiteDbRepository_Example_Tests.cs** - 11 Beispiele
- **FakeDataStoreProvider_Example_Tests.cs** - 10 Beispiele

---

## ?? Siehe auch

- [API-Referenz](./API-Referenz.md) - Vollständige API-Dokumentation
- [Test-Patterns](./Test-Patterns.md) - Bewährte Test-Patterns
- [DataToolKit API](../../DataToolKit/Docs/API-Referenz.md) - Produktions-APIs
