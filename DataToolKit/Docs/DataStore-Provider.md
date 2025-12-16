# DataStore Provider - Thread-Safe Singleton Management

Der `DataStoreProvider` ist ein **thread-sicherer Provider** für DataStore-Instanzen mit **Singleton-Management**, **AutoLoad-Funktionalität** und **automatischer Repository-Auswahl**.

## ?? Inhaltsverzeichnis

- [Übersicht](#übersicht)
- [IDataStoreProvider Interface](#idatastoreprovider-interface)
- [Singleton-Management](#singleton-management)
- [AutoLoad-Funktionalität](#autoload-funktionalität)
- [Automatic Repository Selection](#automatic-repository-selection)
- [Thread-Safety](#thread-safety)
- [Best Practices](#best-practices)

## Übersicht

### Das Problem

```csharp
// ? Alt: Manuelle DataStore-Verwaltung
public class CustomerViewModel
{
    private readonly PersistentDataStore<Customer> _store;

    public CustomerViewModel(IRepository<Customer> repository)
    {
        _store = new PersistentDataStore<Customer>(repository);
        _store.Load();  // Manuelles Laden!
    }
}

// Problem:
// - Kein Singleton ? Mehrere Instanzen mit gleichen Daten
// - Manuelles Load() ? Fehleranfällig
// - Kein Thread-Safety ? Race Conditions möglich
```

### Die Lösung

```csharp
// ? Neu: DataStoreProvider mit AutoLoad
public class CustomerViewModel
{
    private readonly PersistentDataStore<Customer> _store;

    public CustomerViewModel(
        IDataStoreProvider provider,
        IRepositoryFactory repositoryFactory)
    {
        _store = provider.GetPersistent<Customer>(
            repositoryFactory,
            isSingleton: true,        // ? Singleton
            trackPropertyChanges: true, // ? Auto-Save
            autoLoad: true);           // ? Auto-Load!
    }
    
    // Daten sofort verfügbar, keine manuelle Initialisierung nötig!
    public ReadOnlyObservableCollection<Customer> Customers => _store.Items;
}
```

**Vorteile:**
- ? **Singleton**: Eine Instanz pro Typ, geteilt über alle ViewModels
- ? **AutoLoad**: Daten werden automatisch geladen
- ? **Thread-Safe**: Sichere parallele Zugriffe
- ? **Automatic Selection**: EntityBase ? LiteDB, sonst ? JSON

## IDataStoreProvider Interface

### Definition

```csharp
namespace DataToolKit.Abstractions.DataStores;

public interface IDataStoreProvider
{
    /// <summary>
    /// Gibt einen InMemoryDataStore zurück (Singleton oder neue Instanz).
    /// </summary>
    InMemoryDataStore<T> GetInMemory<T>(
        bool isSingleton = true,
        IEqualityComparer<T>? comparer = null)
        where T : class;

    /// <summary>
    /// Gibt einen InMemoryDataStore asynchron zurück.
    /// </summary>
    Task<InMemoryDataStore<T>> GetInMemoryAsync<T>(
        bool isSingleton = true,
        IEqualityComparer<T>? comparer = null)
        where T : class;

    /// <summary>
    /// Gibt einen PersistentDataStore zurück (mit AutoLoad).
    /// </summary>
    PersistentDataStore<T> GetPersistent<T>(
        IRepositoryFactory repositoryFactory,
        bool isSingleton = true,
        bool trackPropertyChanges = true,
        bool autoLoad = true)
        where T : class, IEntity;

    /// <summary>
    /// Gibt einen PersistentDataStore asynchron zurück.
    /// </summary>
    Task<PersistentDataStore<T>> GetPersistentAsync<T>(
        IRepositoryFactory repositoryFactory,
        bool isSingleton = true,
        bool trackPropertyChanges = true,
        bool autoLoad = true)
        where T : class, IEntity;

    /// <summary>
    /// Entfernt Singleton-Instanz aus dem Cache.
    /// </summary>
    bool RemoveSingleton<T>() where T : class;

    /// <summary>
    /// Entfernt alle Singleton-Instanzen.
    /// </summary>
    void ClearAll();
}
```

### Registrierung

```csharp
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Storage.DataStores;
using Microsoft.Extensions.DependencyInjection;

// Automatisch via DataToolKitServiceModule
services.AddSingleton<IDataStoreFactory, DataStoreFactory>();
services.AddSingleton<IDataStoreProvider, DataStoreProvider>();

// Oder manuell:
services.AddSingleton<IDataStoreProvider>(sp =>
{
    var factory = sp.GetRequiredService<IDataStoreFactory>();
    return new DataStoreProvider(factory);
});
```

## Singleton-Management

### Konzept

Der Provider verwaltet **eine Singleton-Instanz pro Typ**:

```csharp
// Erster Aufruf: Erstellt neue Instanz
var store1 = provider.GetPersistent<Customer>(factory);

// Zweiter Aufruf: Gibt gleiche Instanz zurück
var store2 = provider.GetPersistent<Customer>(factory);

Assert.Same(store1, store2);  // ? Gleiche Referenz!
```

### Cache-Keys

Intern verwendet der Provider **separate Keys** für InMemory vs. Persistent:

```csharp
// Keine Kollision zwischen InMemory und Persistent
var inMemoryStore = provider.GetInMemory<Customer>();
var persistentStore = provider.GetPersistent<Customer>(factory);

// ? Unterschiedliche Instanzen, keine Konflikte
Assert.NotSame(inMemoryStore, persistentStore);
```

### Non-Singleton Modus

```csharp
// Jeder Aufruf gibt neue Instanz zurück
var store1 = provider.GetPersistent<Customer>(
    factory, 
    isSingleton: false);  // ? Neueinstanz

var store2 = provider.GetPersistent<Customer>(
    factory, 
    isSingleton: false);  // ? Noch eine neue Instanz

Assert.NotSame(store1, store2);  // ? Unterschiedliche Referenzen
```

### RemoveSingleton

```csharp
// Singleton erstellen
var store = provider.GetPersistent<Customer>(factory);
store.Add(new Customer { Name = "Alice" });

// Singleton entfernen (disposed automatisch)
bool removed = provider.RemoveSingleton<Customer>();
Assert.True(removed);

// Nächster Aufruf erstellt neue Instanz
var newStore = provider.GetPersistent<Customer>(factory);
Assert.NotSame(store, newStore);
Assert.Equal(0, newStore.Count);  // Leer!
```

### ClearAll

```csharp
// Mehrere Singletons erstellen
var customers = provider.GetPersistent<Customer>(factory);
var orders = provider.GetPersistent<Order>(factory);
var products = provider.GetPersistent<Product>(factory);

// Alle Singletons entfernen
provider.ClearAll();

// Alle Instanzen werden disposed
// Nächste Aufrufe erstellen neue Instanzen
```

## AutoLoad-Funktionalität

### Konzept

Mit `autoLoad: true` werden Daten **automatisch** aus dem Repository geladen:

```csharp
// ? Alt: Manuelles Laden
var repository = serviceProvider.GetRequiredService<IRepository<Customer>>();
var store = new PersistentDataStore<Customer>(repository);
store.Load();  // ? Manuell!

// ? Neu: AutoLoad
var store = provider.GetPersistent<Customer>(
    repositoryFactory,
    autoLoad: true);  // ? Automatisch geladen!

// Daten sofort verfügbar
foreach (var customer in store.Items)
{
    Console.WriteLine(customer.Name);
}
```

### AutoLoad: true (Standard)

```csharp
var store = provider.GetPersistent<Customer>(
    repositoryFactory,
    autoLoad: true);  // ? Daten werden geladen

Assert.Equal(2, store.Count);  // ? Daten sind da!
```

### AutoLoad: false

```csharp
var store = provider.GetPersistent<Customer>(
    repositoryFactory,
    autoLoad: false);  // ? Keine Daten geladen

Assert.Equal(0, store.Count);  // ? Leer!

// Manuelles Laden bei Bedarf
store.Load();
Assert.Equal(2, store.Count);  // ? Jetzt sind Daten da
```

### Singleton + AutoLoad

**Wichtig**: Bei Singletons wird **nur beim ersten Aufruf** geladen:

```csharp
// Erster Aufruf: Erstellt Store + lädt Daten
var store1 = provider.GetPersistent<Customer>(
    repositoryFactory,
    isSingleton: true,
    autoLoad: true);

Assert.Equal(2, store1.Count);  // ? Daten geladen

// Zweiter Aufruf: Gibt gleiche Instanz zurück (kein erneutes Laden!)
var store2 = provider.GetPersistent<Customer>(
    repositoryFactory,
    isSingleton: true,
    autoLoad: true);  // ? autoLoad wird ignoriert

Assert.Same(store1, store2);
Assert.Equal(2, store2.Count);  // ? Gleiche Daten
```

### Non-Singleton + AutoLoad

Bei Non-Singletons wird **jedes Mal** neu geladen:

```csharp
// Erster Aufruf: Neue Instanz + laden
var store1 = provider.GetPersistent<Customer>(
    repositoryFactory,
    isSingleton: false,
    autoLoad: true);

// Zweiter Aufruf: NEUE Instanz + laden
var store2 = provider.GetPersistent<Customer>(
    repositoryFactory,
    isSingleton: false,
    autoLoad: true);

Assert.NotSame(store1, store2);  // ? Unterschiedliche Instanzen
Assert.Equal(store1.Count, store2.Count);  // ? Aber gleiche Daten
```

## Automatic Repository Selection

Der Provider erkennt **automatisch**, ob LiteDB oder JSON verwendet werden soll:

### Regel

```csharp
// EntityBase ? LiteDB Repository
public class Customer : EntityBase { ... }

// Nur IEntity ? JSON Repository
public class Settings : IEntity { ... }
```

### Implementierung

```csharp
private IRepositoryBase<T> ResolveRepository<T>(IRepositoryFactory factory)
    where T : class, IEntity
{
    // EntityBase ? LiteDB
    if (typeof(EntityBase).IsAssignableFrom(typeof(T)))
    {
        return factory.GetLiteDbRepository<T>();
    }

    // Nur IEntity ? JSON
    return factory.GetJsonRepository<T>();
}
```

### Beispiel

```csharp
// Customer : EntityBase
var customerStore = provider.GetPersistent<Customer>(factory);
// ? factory.GetLiteDbRepository<Customer>()
// ? LiteDbPersistenceStrategy (granular Update/Delete)

// Settings : IEntity
var settingsStore = provider.GetPersistent<Settings>(factory);
// ? factory.GetJsonRepository<Settings>()
// ? JsonPersistenceStrategy (atomic WriteAll)
```

### Override

Falls Sie die automatische Auswahl übersteuern möchten:

```csharp
// Manuelle Auswahl über direkte Repository-Verwendung
var jsonRepo = repositoryFactory.GetJsonRepository<Customer>();
var store = new PersistentDataStore<Customer>(jsonRepo);
store.Load();
```

## Thread-Safety

### SemaphoreSlim

Der Provider verwendet `SemaphoreSlim` für **thread-safe Singleton-Zugriffe**:

```csharp
public sealed class DataStoreProvider : IDataStoreProvider
{
    private readonly Dictionary<string, object> _singletons = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public PersistentDataStore<T> GetPersistent<T>(...)
    {
        _lock.Wait();  // ? Thread-safe
        try
        {
            // Singleton-Logik
        }
        finally
        {
            _lock.Release();
        }
    }
}
```

### Concurrent Access

```csharp
// 100 parallele Zugriffe
var tasks = Enumerable.Range(0, 100)
    .Select(_ => Task.Run(() => provider.GetPersistent<Customer>(factory)))
    .ToArray();

var stores = await Task.WhenAll(tasks);

// ? Alle bekommen die GLEICHE Instanz
Assert.Equal(1, stores.Distinct().Count());
```

### Async Support

```csharp
// Asynchrone Varianten für async/await Patterns
var store = await provider.GetPersistentAsync<Customer>(
    repositoryFactory,
    isSingleton: true,
    autoLoad: true);

// ? Thread-safe + async-kompatibel
```

## Best Practices

### ? Do's

#### 1. Singleton für Shared State

```csharp
// ? Gut: Singleton für ViewModels, die Daten teilen
public class CustomerListViewModel
{
    public CustomerListViewModel(
        IDataStoreProvider provider,
        IRepositoryFactory factory)
    {
        Customers = provider.GetPersistent<Customer>(
            factory,
            isSingleton: true);  // ? Alle ViewModels teilen Daten
    }

    public PersistentDataStore<Customer> Customers { get; }
}

public class CustomerDetailViewModel
{
    public CustomerDetailViewModel(
        IDataStoreProvider provider,
        IRepositoryFactory factory)
    {
        Customers = provider.GetPersistent<Customer>(
            factory,
            isSingleton: true);  // ? Gleiche Daten wie Liste
    }

    public PersistentDataStore<Customer> Customers { get; }
}
```

#### 2. AutoLoad aktivieren

```csharp
// ? Gut: AutoLoad spart manuellen Load()-Aufruf
var store = provider.GetPersistent<Customer>(
    factory,
    autoLoad: true);  // ? Daten sofort da

// Direkt mit Daten arbeiten
foreach (var customer in store.Items)
{
    Console.WriteLine(customer.Name);
}
```

#### 3. PropertyChanged-Tracking für LiteDB

```csharp
// ? Gut: PropertyChanged für automatische Persistierung
var store = provider.GetPersistent<Customer>(
    factory,
    trackPropertyChanges: true);  // ? Auto-Save

var customer = store.Items.First();
customer.Name = "Updated";  // ? Sofort in LiteDB gespeichert!
```

#### 4. using-Pattern mit Dispose

```csharp
// ? Gut: Provider disposed korrekt
using var provider = new DataStoreProvider(factory);

var store = provider.GetPersistent<Customer>(repositoryFactory);
// ...

// provider.Dispose() wird automatisch aufgerufen
// ? Alle Singletons werden disposed
```

### ? Don'ts

#### 1. Keine Singletons für isolierte Daten

```csharp
// ? Schlecht: Singleton für isolierte Daten
public class ExportService
{
    public void ExportCustomers()
    {
        var store = provider.GetPersistent<Customer>(
            factory,
            isSingleton: true);  // ? Teilt Daten mit anderen!
        
        // Manipulationen beeinflussen andere ViewModels
        store.Clear();  // ? Löscht Daten für alle!
    }
}

// ? Besser: Non-Singleton
public void ExportCustomers()
{
    var store = provider.GetPersistent<Customer>(
        factory,
        isSingleton: false);  // ? Isolierte Kopie
    
    store.Clear();  // ? Beeinflusst nur diese Instanz
}
```

#### 2. Kein AutoLoad ohne Error-Handling

```csharp
// ? Schlecht: AutoLoad ohne Try-Catch
var store = provider.GetPersistent<Customer>(
    factory,
    autoLoad: true);  // ? Was wenn Datei korrupt?

// ? Besser: Mit Error-Handling
try
{
    var store = provider.GetPersistent<Customer>(
        factory,
        autoLoad: true);
}
catch (JsonException ex)
{
    _logger.LogError(ex, "Failed to load customers");
    // Fallback-Logik
}
```

#### 3. Keine manuelle Singleton-Verwaltung

```csharp
// ? Schlecht: Eigene Singleton-Verwaltung
private static PersistentDataStore<Customer>? _instance;

public PersistentDataStore<Customer> GetCustomers()
{
    if (_instance == null)
    {
        _instance = new PersistentDataStore<Customer>(repository);
        _instance.Load();
    }
    return _instance;
}

// ? Besser: DataStoreProvider verwenden
public PersistentDataStore<Customer> GetCustomers()
{
    return provider.GetPersistent<Customer>(
        factory,
        isSingleton: true,
        autoLoad: true);
}
```

## Verwendungsszenarien

### Desktop-Anwendung (WPF)

```csharp
public class MainViewModel : ViewModelBase
{
    private readonly IDataStoreProvider _provider;
    private readonly IRepositoryFactory _factory;

    public MainViewModel(
        IDataStoreProvider provider,
        IRepositoryFactory factory)
    {
        _provider = provider;
        _factory = factory;

        // Singletons für Shared State
        Customers = _provider.GetPersistent<Customer>(_factory, autoLoad: true);
        Orders = _provider.GetPersistent<Order>(_factory, autoLoad: true);
        Products = _provider.GetPersistent<Product>(_factory, autoLoad: true);
    }

    public PersistentDataStore<Customer> Customers { get; }
    public PersistentDataStore<Order> Orders { get; }
    public PersistentDataStore<Product> Products { get; }

    // Alle ViewModels teilen diese Daten
    public ObservableCollection<Customer> CustomerList => Customers.Items;
}
```

### Background Service

```csharp
public class DataSyncService : BackgroundService
{
    private readonly IDataStoreProvider _provider;
    private readonly IRepositoryFactory _factory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Non-Singleton für isolierte Operationen
            var store = _provider.GetPersistent<Customer>(
                _factory,
                isSingleton: false,  // ? Isoliert
                autoLoad: true);

            // Sync-Logik
            await SyncWithRemote(store, stoppingToken);

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

## Weiterführende Themen

- [Repositories ?](Repositories.md)
- [Storage Options ?](Storage-Options.md)
- [API-Referenz ?](API-Referenz.md)
- [Zurück zur Übersicht ?](../README.md)
