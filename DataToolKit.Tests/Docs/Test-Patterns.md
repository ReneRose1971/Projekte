# Test-Patterns und Best Practices

Bewährte Patterns und Best Practices für Tests mit dem DataToolKit-Fake-Framework.

---

## ?? Inhalt

- [Allgemeine Best Practices](#allgemeine-best-practices)
- [Fake-Patterns](#fake-patterns)
- [xUnit-Patterns](#xunit-patterns)
- [Testdaten-Management](#testdaten-management)
- [Assertions](#assertions)
- [Anti-Patterns](#anti-patterns)

---

## Allgemeine Best Practices

### 1. Fakes statt echte I/O

**? Empfohlen:**
```csharp
// Fake-Provider für schnelle Tests
var provider = new FakeDataStoreProvider();
var store = provider.GetPersistent<Customer>(
    provider.RepositoryFactory,
    autoLoad: false
);
```

**? Vermeiden:**
```csharp
// Echtes Repository mit Dateisystem-Zugriff
var options = new JsonStorageOptions<Customer>("App", "data", "Config");
var repo = new JsonRepository<Customer>(options);
// Langsam, benötigt Cleanup
```

### 2. One Assert per Test (bei Möglichkeit)

**? Gut:**
```csharp
[Fact]
public void Add_Should_Increase_Count()
{
    var store = provider.GetInMemory<Customer>();
    store.Add(new Customer { Id = 1, Name = "Alice" });
    
    Assert.Equal(1, store.Count);
}

[Fact]
public void Add_Should_Persist_To_Repository()
{
    var store = provider.GetPersistent<Customer>(...);
    store.Add(new Customer { Id = 0, Name = "Alice" });
    
    var repo = provider.RepositoryFactory.GetFakeLiteDbRepository<Customer>();
    Assert.Equal(1, repo.WriteCallCount);
}
```

**?? Akzeptabel (Related Assertions):**
```csharp
[Fact]
public void Add_Should_Update_Store_And_Repository()
{
    var store = provider.GetPersistent<Customer>(...);
    store.Add(new Customer { Id = 0, Name = "Alice" });
    
    // Related assertions
    Assert.Equal(1, store.Count);
    var repo = provider.RepositoryFactory.GetFakeLiteDbRepository<Customer>();
    Assert.Equal(1, repo.WriteCallCount);
}
```

### 3. Arrange-Act-Assert Pattern

**? Immer verwenden:**
```csharp
[Fact]
public void Test_Example()
{
    // Arrange
    var provider = new FakeDataStoreProvider();
    var store = provider.GetInMemory<Customer>();
    var customer = new Customer { Id = 1, Name = "Alice" };
    
    // Act
    store.Add(customer);
    
    // Assert
    Assert.Equal(1, store.Count);
}
```

---

## Fake-Patterns

### Pattern 1: Repository-Inspection

**Use Case:** Verifiziere, dass Repository korrekt aufgerufen wurde.

```csharp
[Fact]
public void DataStore_Should_Call_Repository_On_Add()
{
    // Arrange
    var provider = new FakeDataStoreProvider();
    var store = provider.GetPersistent<Customer>(
        provider.RepositoryFactory,
        autoLoad: false
    );
    
    // Act
    store.Add(new Customer { Id = 0, Name = "Alice" });
    store.Add(new Customer { Id = 0, Name = "Bob" });
    
    // Assert: Inspect Repository
    var repo = provider.RepositoryFactory.GetFakeLiteDbRepository<Customer>();
    Assert.Equal(2, repo.WriteCallCount);
    Assert.Equal(2, repo.History.Count(h => h.Action == "Write"));
}
```

### Pattern 2: Fehler-Simulation

**Use Case:** Teste Fehlerbehandlung.

```csharp
[Fact]
public void DataStore_Should_Handle_Repository_Failure()
{
    // Arrange
    var provider = new FakeDataStoreProvider();
    var repo = provider.RepositoryFactory.GetFakeLiteDbRepository<Customer>();
    repo.ThrowOnWrite = true;  // Simuliere Fehler
    
    var store = provider.GetPersistent<Customer>(
        provider.RepositoryFactory,
        autoLoad: false
    );
    
    // Act & Assert
    var ex = Assert.Throws<InvalidOperationException>(
        () => store.Add(new Customer { Id = 0, Name = "Alice" })
    );
    Assert.Contains("Simulated write failure", ex.Message);
}
```

### Pattern 3: History-Verification

**Use Case:** Verifiziere Sequenz von Operationen.

```csharp
[Fact]
public void DataStore_Should_Execute_Operations_In_Order()
{
    // Arrange
    var repo = new FakeJsonRepository<Customer>();
    
    // Act
    repo.Write(customers);  // 1
    repo.Load();            // 2
    repo.Clear();           // 3
    repo.Load();            // 4
    
    // Assert: Verify sequence
    Assert.Equal(4, repo.History.Count);
    Assert.Equal("Write", repo.History[0].Action);
    Assert.Equal("Load", repo.History[1].Action);
    Assert.Equal("Clear", repo.History[2].Action);
    Assert.Equal("Load", repo.History[3].Action);
}
```

### Pattern 4: SeedData für Setup

**Use Case:** Initialisiere Repository ohne History-Pollution.

```csharp
[Fact]
public void Test_With_Preseeded_Data()
{
    // Arrange
    var repo = new FakeLiteDbRepository<Customer>();
    repo.SeedData(
        new Customer { Id = 1, Name = "Alice" },
        new Customer { Id = 2, Name = "Bob" }
    );
    
    // Act
    var loaded = repo.Load();
    
    // Assert
    Assert.Equal(2, loaded.Count);
    Assert.Equal(1, repo.History.Count); // Nur Load, kein Write
}
```

---

## xUnit-Patterns

### Pattern 1: Fixtures für gemeinsames Setup

**Use Case:** Mehrere Tests mit gleichem Setup.

```csharp
public class CustomerTestsFixture : IDisposable
{
    public FakeDataStoreProvider Provider { get; }
    public IDataStore<Customer> Store { get; }
    
    public CustomerTestsFixture()
    {
        Provider = new FakeDataStoreProvider();
        Store = Provider.GetInMemory<Customer>();
    }
    
    public void Dispose()
    {
        Provider.ClearAll();
    }
}

public class CustomerTests : IClassFixture<CustomerTestsFixture>
{
    private readonly CustomerTestsFixture _fixture;
    
    public CustomerTests(CustomerTestsFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public void Test1()
    {
        _fixture.Store.Add(new Customer { Id = 1, Name = "Alice" });
        Assert.Equal(1, _fixture.Store.Count);
    }
}
```

### Pattern 2: DataStoreTestFixture

**Use Case:** Standardisiertes Setup für DataStore-Tests.

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
        _fixture.Reset(); // Sauberer Start
        _fixture.SeedData(new Customer { Id = 1, Name = "Alice" });
        
        Assert.Equal(1, _fixture.DataStore.Count);
    }
    
    [Fact]
    public void Test2()
    {
        _fixture.Reset(); // Unabhängig von Test1
        Assert.Equal(0, _fixture.DataStore.Count);
    }
}
```

### Pattern 3: Theory für parametrisierte Tests

**Use Case:** Teste gleiche Logik mit verschiedenen Daten.

```csharp
[Theory]
[InlineData(1, "Alice")]
[InlineData(2, "Bob")]
[InlineData(3, "Charlie")]
public void Add_Should_Store_Customer(int id, string name)
{
    // Arrange
    var store = provider.GetInMemory<Customer>();
    var customer = new Customer { Id = id, Name = name };
    
    // Act
    store.Add(customer);
    
    // Assert
    Assert.Contains(store.Items, c => c.Id == id && c.Name == name);
}
```

---

## Testdaten-Management

### Pattern 1: TestEntityBuilder

**Use Case:** Erstelle komplexe Testdaten mit Fluent API.

```csharp
[Fact]
public void Test_With_Builder()
{
    // Arrange
    var customers = new TestEntityBuilder<Customer>()
        .WithId(0) // Auto-ID
        .BuildMany(100, (c, i) => {
            c.Name = $"Customer {i}";
            c.Email = $"customer{i}@test.com";
            c.CreatedAt = DateTime.UtcNow.AddDays(-i);
        });
    
    var repo = new FakeLiteDbRepository<Customer>();
    
    // Act
    repo.Write(customers);
    
    // Assert
    Assert.Equal(100, repo.Load().Count);
}
```

### Pattern 2: RepositoryScenarioBuilder

**Use Case:** Erstelle komplexe Repository-Szenarien.

```csharp
[Fact]
public void Test_Complex_Scenario()
{
    // Arrange
    var factory = new FakeRepositoryFactory();
    var repo = new RepositoryScenarioBuilder<Customer>(factory)
        .WithEntity(new Customer { Id = 1, Name = "Alice", IsVIP = true })
        .WithEntity(new Customer { Id = 2, Name = "Bob", IsVIP = false })
        .WithRandomEntities(98, i => new Customer 
        { 
            Id = 0, 
            Name = $"Customer {i}",
            IsVIP = i % 10 == 0 
        })
        .BuildFakeLiteDb();
    
    // Act
    var vips = repo.Load().Where(c => c.IsVIP).ToList();
    
    // Assert
    Assert.True(vips.Count >= 2);
}
```

### Pattern 3: Object Mother

**Use Case:** Zentralisiere Testdaten-Erstellung.

```csharp
public static class TestCustomers
{
    public static Customer Alice => new Customer 
    { 
        Id = 1, 
        Name = "Alice", 
        Email = "alice@test.com",
        IsActive = true
    };
    
    public static Customer Bob => new Customer 
    { 
        Id = 2, 
        Name = "Bob", 
        Email = "bob@test.com",
        IsActive = false
    };
    
    public static List<Customer> GetStandardSet() => new()
    {
        Alice,
        Bob,
        new Customer { Id = 3, Name = "Charlie", Email = "charlie@test.com" }
    };
}

// Verwendung
[Fact]
public void Test_With_Standard_Data()
{
    var repo = new FakeJsonRepository<Customer>();
    repo.SeedData(TestCustomers.GetStandardSet().ToArray());
    
    Assert.Equal(3, repo.Load().Count);
}
```

---

## Assertions

### Pattern 1: Fluent Assertions (xUnit-Stil)

```csharp
[Fact]
public void Test_With_Fluent_Assertions()
{
    var repo = new FakeLiteDbRepository<Customer>();
    repo.Write(new[] { 
        new Customer { Id = 0, Name = "Alice" },
        new Customer { Id = 0, Name = "Bob" }
    });
    
    var loaded = repo.Load();
    
    // Collection assertions
    Assert.Equal(2, loaded.Count);
    Assert.All(loaded, c => Assert.True(c.Id > 0));
    Assert.Contains(loaded, c => c.Name == "Alice");
    Assert.DoesNotContain(loaded, c => c.Name == "Charlie");
}
```

### Pattern 2: Custom Assertion Methods

```csharp
public static class CustomAssertions
{
    public static void ShouldHavePersistedOnce<T>(
        this FakeLiteDbRepository<T> repo) where T : EntityBase
    {
        Assert.True(repo.WriteCallCount > 0, "Repository should have persisted data");
    }
    
    public static void ShouldContainEntity<T>(
        this IDataStore<T> store, 
        Func<T, bool> predicate) where T : class
    {
        Assert.Contains(store.Items, predicate);
    }
}

// Verwendung
[Fact]
public void Test_With_Custom_Assertions()
{
    var repo = provider.RepositoryFactory.GetFakeLiteDbRepository<Customer>();
    store.Add(new Customer { Id = 0, Name = "Alice" });
    
    repo.ShouldHavePersistedOnce();
    store.ShouldContainEntity(c => c.Name == "Alice");
}
```

---

## Anti-Patterns

### ? Anti-Pattern 1: Test-Interdependenzen

**Problem:** Tests beeinflussen sich gegenseitig.

```csharp
// ? FALSCH
public class BadTests
{
    private static FakeDataStoreProvider _provider = new();
    
    [Fact]
    public void Test1()
    {
        var store = _provider.GetInMemory<Customer>();
        store.Add(new Customer { Id = 1 });
    }
    
    [Fact]
    public void Test2()
    {
        var store = _provider.GetInMemory<Customer>();
        // Fehlschlag wenn Test1 vorher lief!
        Assert.Empty(store.Items);
    }
}
```

**? Lösung: Isolation**

```csharp
// ? RICHTIG
public class GoodTests : IClassFixture<DataStoreTestFixture<Customer>>
{
    private readonly DataStoreTestFixture<Customer> _fixture;
    
    public GoodTests(DataStoreTestFixture<Customer> fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public void Test1()
    {
        _fixture.Reset(); // Isolation
        _fixture.DataStore.Add(new Customer { Id = 1 });
    }
    
    [Fact]
    public void Test2()
    {
        _fixture.Reset(); // Isolation
        Assert.Empty(_fixture.DataStore.Items);
    }
}
```

### ? Anti-Pattern 2: Echte I/O in Unit-Tests

**Problem:** Langsame Tests, Cleanup nötig.

```csharp
// ? FALSCH
[Fact]
public void Test_With_Real_Files()
{
    var options = new JsonStorageOptions<Customer>("App", "data", "Test");
    var repo = new JsonRepository<Customer>(options);
    
    repo.Write(customers);
    var loaded = repo.Load();
    
    // Cleanup vergessen!
}
```

**? Lösung: Fakes oder TestDirectorySandbox**

```csharp
// ? RICHTIG (mit Fake)
[Fact]
public void Test_With_Fake()
{
    var repo = new FakeJsonRepository<Customer>();
    repo.Write(customers);
    var loaded = repo.Load();
    
    Assert.Equal(customers.Count, loaded.Count);
}

// ? RICHTIG (wenn echte Dateien nötig)
[Fact]
public void Test_With_Sandbox()
{
    using var sandbox = new TestDirectorySandbox();
    var options = new JsonStorageOptions<Customer>("App", "data", "Test", sandbox.Root);
    var repo = new JsonRepository<Customer>(options);
    
    repo.Write(customers);
    var loaded = repo.Load();
    
    // Auto-Cleanup durch Dispose
}
```

### ? Anti-Pattern 3: Zu viele Assertions

**Problem:** Unklarer Test-Zweck.

```csharp
// ? FALSCH
[Fact]
public void Test_Everything()
{
    var store = provider.GetPersistent<Customer>(...);
    
    store.Add(customer1);
    Assert.Equal(1, store.Count);
    
    store.Add(customer2);
    Assert.Equal(2, store.Count);
    
    store.Remove(customer1);
    Assert.Equal(1, store.Count);
    
    var repo = provider.RepositoryFactory.GetFakeLiteDbRepository<Customer>();
    Assert.Equal(3, repo.WriteCallCount);
    
    // Was wird hier eigentlich getestet?
}
```

**? Lösung: Fokussierte Tests**

```csharp
// ? RICHTIG
[Fact]
public void Add_Should_Increase_Count()
{
    var store = provider.GetInMemory<Customer>();
    store.Add(new Customer { Id = 1 });
    
    Assert.Equal(1, store.Count);
}

[Fact]
public void Remove_Should_Decrease_Count()
{
    var store = provider.GetInMemory<Customer>();
    store.Add(customer);
    
    store.Remove(customer);
    
    Assert.Equal(0, store.Count);
}

[Fact]
public void Add_And_Remove_Should_Persist()
{
    var store = provider.GetPersistent<Customer>(...);
    store.Add(customer);
    store.Remove(customer);
    
    var repo = provider.RepositoryFactory.GetFakeLiteDbRepository<Customer>();
    Assert.Equal(2, repo.WriteCallCount);
}
```

---

## ?? Siehe auch

- [API-Referenz](./API-Referenz.md) - Vollständige API-Dokumentation
- [Fake-Framework Guide](./Fake-Framework.md) - Einführung in das Framework
- [xUnit Documentation](https://xunit.net/) - Offizielle xUnit-Dokumentation
