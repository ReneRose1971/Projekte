# Repositories - DataToolKit

Repositories in DataToolKit bieten eine einheitliche Schnittstelle für dateibasierte Persistierung. Wählen Sie zwischen JSON (atomares Write) oder LiteDB (Delta-Synchronisierung) basierend auf Ihren Anforderungen.

## ?? Inhaltsverzeichnis

- [Übersicht](#übersicht)
- [IRepositoryBase<T>](#irepositorybaset)
- [IRepository<T>](#irepositoryt)
- [JSON Repository](#json-repository)
- [LiteDB Repository](#litedb-repository)
- [Repository Factory](#repository-factory)
- [DataStore Provider](#datastore-provider)
- [Best Practices](#best-practices)

## Übersicht

### Repository-Hierarchie

```
IRepositoryBase<T>              # Basis-Interface (Load, Write, Clear)
    ??? JsonRepository<T>        # Atomares Write-All
    ??? IRepository<T>           # Erweitert um Update/Delete
            ??? LiteDbRepository<T>  # Granulare Operationen
```

### Wann welches Repository?

| Feature | JSON Repository | LiteDB Repository |
|---------|----------------|-------------------|
| **Persistierung** | Atomares WriteAll | Delta-Synchronisierung |
| **Datenstruktur** | JSON-Datei | NoSQL-Datenbank |
| **Update** | ? Nur WriteAll | ? Update(T) |
| **Delete** | ? Nur WriteAll | ? Delete(T) |
| **Backup** | ? .bak-Datei | ? Nein |
| **Performance** | Gut für < 1.000 | Gut für < 10.000 |
| **Komplex

ität** | Einfach | Mittel |
| **EqualityComparer** | ? Nicht nötig | ? Erforderlich |

## IRepositoryBase<T>

### Interface-Definition

```csharp
namespace DataToolKit.Abstractions.Repositories;

/// <summary>
/// Basis-Repository mit Load, Write und Clear.
/// </summary>
public interface IRepositoryBase<T> : IDisposable where T : class, IEntity
{
    /// <summary>
    /// Lädt alle Datensätze aus dem Repository.
    /// </summary>
    /// <returns>ReadOnly-Collection aller Datensätze.</returns>
    IReadOnlyList<T> Load();

    /// <summary>
    /// Schreibt alle Datensätze ins Repository.
    /// </summary>
    /// <param name="items">Zu speichernde Datensätze.</param>
    void Write(IEnumerable<T> items);

    /// <summary>
    /// Löscht alle Datensätze aus dem Repository.
    /// </summary>
    void Clear();
}
```

### Verwendung

```csharp
public class CustomerService
{
    private readonly IRepositoryBase<Customer> _repository;

    public CustomerService(IRepositoryBase<Customer> repository)
    {
        _repository = repository;
    }

    public void AddCustomer(Customer customer)
    {
        var customers = _repository.Load().ToList();
        customers.Add(customer);
        _repository.Write(customers);
    }

    public IReadOnlyList<Customer> GetAll()
    {
        return _repository.Load();
    }

    public void DeleteCustomer(int id)
    {
        var customers = _repository.Load()
            .Where(c => c.Id != id)
            .ToList();
        _repository.Write(customers);
    }
}
```

## IRepository<T>

### Interface-Definition

```csharp
namespace DataToolKit.Abstractions.Repositories;

/// <summary>
/// Erweitert IRepositoryBase um granulare Update/Delete-Operationen.
/// </summary>
public interface IRepository<T> : IRepositoryBase<T> where T : class, IEntity
{
    /// <summary>
    /// Aktualisiert einen einzelnen Datensatz.
    /// </summary>
    /// <param name="entity">Zu aktualisierender Datensatz (Id > 0).</param>
    /// <returns>Anzahl der aktualisierten Datensätze (1 oder 0).</returns>
    int Update(T entity);

    /// <summary>
    /// Löscht einen einzelnen Datensatz.
    /// </summary>
    /// <param name="entity">Zu löschender Datensatz (Id > 0).</param>
    /// <returns>Anzahl der gelöschten Datensätze (1 oder 0).</returns>
    int Delete(T entity);
}
```

### Verwendung

```csharp
public class OrderService
{
    private readonly IRepository<Order> _repository;

    public OrderService(IRepository<Order> repository)
    {
        _repository = repository;
    }

    public void UpdateOrderStatus(int orderId, string newStatus)
    {
        var order = _repository.Load().FirstOrDefault(o => o.Id == orderId);
        if (order != null)
        {
            order.Status = newStatus;
            _repository.Update(order);  // ? Nur dieser Datensatz wird geschrieben
        }
    }

    public void DeleteOrder(int orderId)
    {
        var order = _repository.Load().FirstOrDefault(o => o.Id == orderId);
        if (order != null)
        {
            _repository.Delete(order);  // ? Nur dieser Datensatz wird gelöscht
        }
    }
}
```

## JSON Repository

### Implementierung

`JsonRepository<T>` implementiert `IRepositoryBase<T>` und persistiert Daten in JSON-Dateien mit atomarem Write-Mechanismus.

### Features

- ? **Atomares Schreiben**: `.tmp` ? `.json` + `.bak`
- ? **Backup-Datei**: Automatisch bei jedem Write
- ? **ReadOnly-Collection**: Sichere Rückgabe
- ? **UTF-8 Encoding**: Korrekte Zeichensatzunterstützung

### Registrierung

```csharp
using DataToolKit.Abstractions.DI;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.Repositories;
using Microsoft.Extensions.DependencyInjection;

// Storage Options
services.AddSingleton<IStorageOptions<Customer>>(
    new JsonStorageOptions<Customer>("MyApp", "customers", "Data"));

// Repository
services.AddJsonRepository<Customer>();

// Oder manuell:
services.AddSingleton<IRepositoryBase<Customer>>(sp =>
{
    var options = sp.GetRequiredService<IStorageOptions<Customer>>();
    return new JsonRepository<Customer>(options);
});
```

### Dateistruktur

```
C:\Users\...\Documents\MyApp\Data\
    ??? customers.json       ? Aktuelle Daten
    ??? customers.json.bak   ? Backup (vorheriger Stand)
    ??? customers.json.tmp   ? Temporär (nur während Write)
```

### Beispiel-JSON

```json
[
  {
    "Id": 1,
    "Name": "Alice",
    "Email": "alice@example.com",
    "CreatedAt": "2024-01-15T10:30:00"
  },
  {
    "Id": 2,
    "Name": "Bob",
    "Email": "bob@example.com",
    "CreatedAt": "2024-01-16T14:20:00"
  }
]
```

### Atomarer Write-Prozess

```
1. Write(items)
   ?
2. Serialize to JSON
   ?
3. Write to customers.json.tmp
   ?
4. File.Replace(tmp ? json, backup: json.bak)
   ?
5. Result:
   - customers.json (neue Daten)
   - customers.json.bak (alte Daten)
   - customers.json.tmp (gelöscht)
```

### Best Practices

#### ? Do's

```csharp
// 1. Für kleine Datenmengen (< 1.000 Datensätze)
services.AddJsonRepository<Settings>();

// 2. Für einfache DTO-Objekte
public class Settings : IEntity
{
    public int Id { get; set; }
    public string Theme { get; set; } = "";
    public bool AutoSave { get; set; }
}

// 3. Regelmäßige Backups nutzen
var backupPath = options.FullPath + ".bak";
if (File.Exists(backupPath))
{
    File.Copy(backupPath, $"{options.FullPath}.backup-{DateTime.Now:yyyyMMdd}");
}
```

#### ? Don'ts

```csharp
// 1. Nicht für sehr große Datenmengen
// ? Schlecht: 10.000+ Datensätze in JSON
services.AddJsonRepository<Customer>(); // Use LiteDB instead!

// 2. Nicht für häufige Updates
// ? Schlecht: WriteAll bei jedem Property-Change
customer.Name = "New Name";
repository.Write(allCustomers);  // Alle Daten geschrieben!

// 3. Nicht für komplexe Abfragen
// ? Schlecht: Alle Daten laden für Abfrage
var result = repository.Load()
    .Where(c => c.CreatedAt > DateTime.Now.AddDays(-7))
    .OrderBy(c => c.Name)
    .ToList();  // Ineffizient!
```

## LiteDB Repository

### Implementierung

`LiteDbRepository<T>` implementiert `IRepository<T>` und bietet NoSQL-Datenbank mit Delta-Synchronisierung.

### Features

- ? **Delta-Erkennung**: Nur geänderte Datensätze werden geschrieben
- ? **Granulare Operationen**: `Update(T)` und `Delete(T)`
- ? **Transaktionen**: Atomare Updates
- ? **Automatische ID-Vergabe**: Bei `Id = 0`
- ? **Collections**: Eine Collection pro Typ

### Registrierung

```csharp
using Common.Bootstrap.Defaults;
using DataToolKit.Abstractions.DI;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.Repositories;
using Microsoft.Extensions.DependencyInjection;

// EqualityComparer (WICHTIG!)
services.AddSingleton<IEqualityComparer<Order>>(
    new FallbackEqualsComparer<Order>());

// Storage Options
services.AddSingleton<IStorageOptions<Order>>(
    new LiteDbStorageOptions<Order>("MyApp", "database", "Databases"));

// Repository
services.AddLiteDbRepository<Order>();

// Oder manuell:
services.AddSingleton<IRepository<Order>>(sp =>
{
    var options = sp.GetRequiredService<IStorageOptions<Order>>();
    var comparer = sp.GetRequiredService<IEqualityComparer<Order>>();
    return new LiteDbRepository<Order>(options, comparer);
});
```

### Delta-Synchronisierung

```csharp
// 1. Load existing data
var orders = repository.Load().ToList();
// DB: [Order{Id=1, Status="Pending"}, Order{Id=2, Status="Shipped"}]

// 2. Modify data
orders[0].Status = "Processing";  // Update
orders.Add(new Order { Id = 0, Status = "New" });  // Insert (Id=0!)
orders.RemoveAt(1);  // Delete

// 3. Write changes
repository.Write(orders);
// ? LiteDB erkennt Delta:
//   - Update: Order{Id=1}
//   - Insert: Order{Id=0} ? bekommt neue Id (3)
//   - Delete: Order{Id=2}
```

### Dateistruktur

```
C:\Users\...\Documents\MyApp\Databases\
    ??? database.db          ? LiteDB-Datenbankdatei
    ??? database-log.db      ? Optional: Transaction Log
```

### Collections

LiteDB speichert jeden Typ in einer eigenen Collection:

```
database.db
    ??? Collection "Customer" ? Customer-Objekte
    ??? Collection "Order" ? Order-Objekte
    ??? Collection "Product" ? Product-Objekte
```

### EqualityComparer

**WICHTIG**: LiteDB benötigt einen `IEqualityComparer<T>` für Delta-Erkennung!

```csharp
using Common.Bootstrap.Defaults;

// Option 1: FallbackEqualsComparer (nutzt Equals()-Methode)
services.AddSingleton<IEqualityComparer<Order>>(
    new FallbackEqualsComparer<Order>());

// Option 2: Custom Comparer
public class OrderComparer : IEqualityComparer<Order>
{
    public bool Equals(Order? x, Order? y)
    {
        if (x is null || y is null) return false;
        return x.Id == y.Id 
            && x.Status == y.Status 
            && x.Total == y.Total;
    }

    public int GetHashCode(Order obj)
    {
        return HashCode.Combine(obj.Id, obj.Status, obj.Total);
    }
}

services.AddSingleton<IEqualityComparer<Order>>(new OrderComparer());
```

### Best Practices

#### ? Do's

```csharp
// 1. Für mittlere Datenmengen (< 10.000 Datensätze)
services.AddLiteDbRepository<Customer>();

// 2. EntityBase vererben
public class Customer : EntityBase
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}

// 3. Equals() überschreiben für korrekte Delta-Erkennung
public override bool Equals(object? obj)
{
    if (obj is not Customer other) return false;
    return Id == other.Id && Name == other.Name && Email == other.Email;
}

public override int GetHashCode()
{
    return HashCode.Combine(Id, Name, Email);
}

// 4. Update() für einzelne Änderungen
customer.Name = "New Name";
repository.Update(customer);  // ? Nur dieser Datensatz
```

#### ? Don'ts

```csharp
// 1. Nicht ohne EqualityComparer
// ? Fehlt: services.AddSingleton<IEqualityComparer<Order>>(...)
services.AddLiteDbRepository<Order>();  // Delta-Erkennung funktioniert nicht!

// 2. Nicht Id manuell setzen bei Insert
var newOrder = new Order 
{ 
    Id = 999,  // ? Falsch! Id wird von LiteDB vergeben
    Status = "New" 
};
repository.Write(new[] { newOrder });

// ? Richtig:
var newOrder = new Order 
{ 
    Id = 0,  // ? Korrekt! LiteDB vergibt neue Id
    Status = "New" 
};

// 3. Nicht WriteAll für einzelne Updates
var orders = repository.Load().ToList();
orders[0].Status = "Processing";
repository.Write(orders);  // ? Alle Datensätze werden verglichen!

// ? Besser:
var order = repository.Load().First();
order.Status = "Processing";
repository.Update(order);  // ? Nur dieser Datensatz
```

## Repository Factory

Die `IRepositoryFactory` löst Repositories dynamisch auf, basierend auf dem Typ.

### Interface

```csharp
namespace DataToolKit.Storage.Repositories;

public interface IRepositoryFactory
{
    /// <summary>
    /// Löst ein JSON-Repository auf.
    /// </summary>
    IRepositoryBase<T> GetJsonRepository<T>();

    /// <summary>
    /// Löst ein LiteDB-Repository auf.
    /// </summary>
    IRepository<T> GetLiteDbRepository<T>() where T : class;
}
```

### Verwendung

```csharp
public class DataService
{
    private readonly IRepositoryFactory _factory;

    public DataService(IRepositoryFactory factory)
    {
        _factory = factory;
    }

    public void SaveCustomer(Customer customer)
    {
        // Dynamische Auflösung
        var repo = _factory.GetJsonRepository<Customer>();
        var customers = repo.Load().ToList();
        customers.Add(customer);
        repo.Write(customers);
    }

    public void SaveOrder(Order order)
    {
        // Dynamische Auflösung
        var repo = _factory.GetLiteDbRepository<Order>();
        var orders = repo.Load().ToList();
        orders.Add(order);
        repo.Write(orders);
    }
}
```

### Registrierung

```csharp
using DataToolKit.Abstractions.DI;
using DataToolKit.Storage.Repositories;

// Automatisch via DataToolKitServiceModule
services.AddSingleton<IRepositoryFactory, RepositoryFactory>();
```

## DataStore Provider

Der `DataStoreProvider` bietet **Thread-Safe Singleton-Management** und **AutoLoad-Funktionalität** für `PersistentDataStore<T>`.

### Interface

```csharp
namespace DataToolKit.Abstractions.DataStores;

public interface IDataStoreProvider
{
    /// <summary>
    /// Gibt einen InMemoryDataStore zurück (Singleton oder neue Instanz).
    /// </summary>
    InMemoryDataStore<T> GetInMemory<T>(
        bool isSingleton = true,
        IEqualityComparer<T>? comparer = null) where T : class;

    /// <summary>
    /// Gibt einen PersistentDataStore zurück (mit AutoLoad).
    /// </summary>
    PersistentDataStore<T> GetPersistent<T>(
        IRepositoryFactory repositoryFactory,
        bool isSingleton = true,
        bool trackPropertyChanges = true,
        bool autoLoad = true) where T : class, IEntity;

    /// <summary>
    /// Asynchrone Varianten
    /// </summary>
    Task<InMemoryDataStore<T>> GetInMemoryAsync<T>(...);
    Task<PersistentDataStore<T>> GetPersistentAsync<T>(...);

    /// <summary>
    /// Entfernt Singleton aus Cache
    /// </summary>
    bool RemoveSingleton<T>() where T : class;

    /// <summary>
    /// Löscht alle Singletons
    /// </summary>
    void ClearAll();
}
```

### Verwendung mit AutoLoad

```csharp
public class CustomerViewModel
{
    private readonly PersistentDataStore<Customer> _customers;

    public CustomerViewModel(
        IDataStoreProvider provider,
        IRepositoryFactory repositoryFactory)
    {
        // ? Daten werden automatisch geladen!
        _customers = provider.GetPersistent<Customer>(
            repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: true,
            autoLoad: true);  // ? Daten sofort verfügbar
    }

    public ReadOnlyObservableCollection<Customer> Customers => _customers.Items;

    public void AddCustomer(string name, string email)
    {
        _customers.Add(new Customer 
        { 
            Id = 0, 
            Name = name, 
            Email = email 
        });
        // ? Automatisch persistiert
    }
}
```

### Automatic Repository Selection

Der Provider erkennt automatisch, ob EntityBase ? LiteDB oder nur IEntity ? JSON:

```csharp
// EntityBase ? LiteDB
public class Order : EntityBase { ... }
provider.GetPersistent<Order>(factory);  // ? GetLiteDbRepository()

// Nur IEntity ? JSON
public class Settings : IEntity { ... }
provider.GetPersistent<Settings>(factory);  // ? GetJsonRepository()
```

## Best Practices

### ? Repository-Auswahl

```csharp
// JSON für:
// - Konfigurationen
// - Kleine Datenmengen (< 1.000)
// - Einfache DTOs
// - Seltene Updates
services.AddJsonRepository<Settings>();
services.AddJsonRepository<UserPreferences>();

// LiteDB für:
// - Business-Daten
// - Mittlere Datenmengen (< 10.000)
// - EntityBase-Objekte
// - Häufige Updates
services.AddLiteDbRepository<Customer>();
services.AddLiteDbRepository<Order>();
services.AddLiteDbRepository<Product>();
```

### ? DI-Registrierung

```csharp
public class MyDataModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // JSON
        services.AddSingleton<IStorageOptions<Settings>>(
            new JsonStorageOptions<Settings>("MyApp", "settings", "Config"));
        services.AddJsonRepository<Settings>();

        // LiteDB (mit EqualityComparer!)
        services.AddSingleton<IEqualityComparer<Customer>>(
            new FallbackEqualsComparer<Customer>());
        services.AddSingleton<IStorageOptions<Customer>>(
            new LiteDbStorageOptions<Customer>("MyApp", "database", "Databases"));
        services.AddLiteDbRepository<Customer>();
    }
}
```

### ? DataStore Provider verwenden

```csharp
// Statt direktem Repository-Zugriff:
// ? var repo = serviceProvider.GetRequiredService<IRepository<Customer>>();

// ? Besser: DataStoreProvider mit AutoLoad
var provider = serviceProvider.GetRequiredService<IDataStoreProvider>();
var store = provider.GetPersistent<Customer>(
    repositoryFactory,
    autoLoad: true);  // Daten sofort da!

// PropertyChanged wird automatisch persistiert
var customer = store.Items.First();
customer.Name = "Updated";  // ? Sofort in LiteDB gespeichert
```

## Weiterführende Themen

- [Storage Options ?](Storage-Options.md)
- [DataStore Provider Details ?](DataStore-Provider.md)
- [API-Referenz ?](API-Referenz.md)
- [Zurück zur Übersicht ?](../README.md)
