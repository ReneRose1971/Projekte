# DataToolKit

Datei-basierte Repository-Implementierungen für .NET 8 mit JSON- und LiteDB-Unterstützung sowie vollständiger Dependency-Injection-Integration.

## ?? Inhaltsverzeichnis

- [Überblick](#überblick)
- [Installation](#installation)
- [Schnellstart](#schnellstart)
- [Kernkonzepte](#kernkonzepte)
  - [Storage Options](Docs/Storage-Options.md)
  - [Repositories](Docs/Repositories.md)
  - [DataStore Provider](Docs/DataStore-Provider.md)
  - [PropertyChangedBinder](Docs/PropertyChangedBinder.md)
  - [ParentChildRelationship](Docs/ParentChildRelationship.md)
- [?? API-Referenz](Docs/API-Referenz.md) — **Vollständige alphabetisch sortierte API-Dokumentation**

## Überblick

**DataToolKit** ist eine leichtgewichtige Bibliothek für dateibasierte Datenpersistenz in .NET-Anwendungen. Sie bietet:

- ? **JSON-Repository**: Atomare Persistierung mit Backup-Mechanismus
- ? **LiteDB-Repository**: NoSQL-Datenbank mit Delta-Synchronisierung
- ? **Type-Safe Storage Options**: `IStorageOptions<T>` für typsichere Konfiguration
- ? **DI-Integration**: Vollständige Integration mit Microsoft.Extensions.DependencyInjection
- ? **Repository Factory**: Zentrale Factory für dynamische Repository-Auflösung
- ? **DataStore Provider**: Thread-Safe Singleton-Management mit AutoLoad
- ? **ParentChildRelationship**: Automatische 1:n-Beziehungsverwaltung mit PropertyChanged-Tracking
- ? **DataStore Synchronisation**: `SyncWith`-Extension für Collection-Sync

### Wann DataToolKit verwenden?

? **Ideal für:**
- Desktop-Anwendungen (WPF, WinForms, Avalonia)
- Kleine bis mittlere Datenmengen (< 10.000 Datensätze)
- Lokale Konfigurationen und Benutzerdaten
- Prototyping und Proof-of-Concepts
- Offline-First-Anwendungen

? **Nicht geeignet für:**
- Web-APIs mit hohem Traffic
- Sehr große Datenmengen (> 100.000 Datensätze)
- Komplexe Abfragen und Relationen
- Multi-User-Szenarien mit Locking-Anforderungen

## Installation

### NuGet Package
```bash
dotnet add package DataToolKit
```

### Abhängigkeiten

DataToolKit benötigt:
- **.NET 8.0** oder höher
- **LiteDB 5.x** (für LiteDB-Repository)
- **Common.Bootstrap** (für DI-Integration)

## Schnellstart

### 1. Definieren Sie Ihre Entität

```csharp
using DataToolKit.Abstractions;

namespace MyApp.Models;

/// <summary>
/// Customer-Entität für Persistierung.
/// </summary>
public class Customer : EntityBase
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    // Equals/GetHashCode überschreiben für korrekte Vergleiche
    public override bool Equals(object? obj)
    {
        if (obj is not Customer other) return false;
        return Id == other.Id 
            && Name == other.Name 
            && Email == other.Email;
    }

    public override int GetHashCode()
        => HashCode.Combine(Id, Name, Email);
}
```

> ?? **Tipp**: Bei LiteDB muss `Id = 0` für neue Entitäten sein, damit die Datenbank eine ID zuweist.

### 2. Registrieren Sie die Services

```csharp
using Common.Bootstrap;
using Common.Bootstrap.Defaults;
using DataToolKit.Abstractions.DI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Common.Bootstrap registrieren
new CommonBootstrapServiceModule().Register(builder.Services);

// DataToolKit registrieren
new DataToolKitServiceModule().Register(builder.Services);

// EqualityComparer für Customer (erforderlich für LiteDB!)
builder.Services.AddSingleton<IEqualityComparer<Customer>>(
    new FallbackEqualsComparer<Customer>());

// Repository registrieren (StorageOptions werden automatisch erstellt!)
builder.Services.AddLiteDbRepository<Customer>(
    appSubFolder: "MyApp",
    fileNameBase: "database",
    subFolder: "Databases"
);
// ? Speichert unter: C:\Users\{Username}\Documents\MyApp\Databases\database.db

var app = builder.Build();
await app.RunAsync();
```

### 3. Nutzen Sie den DataStore Provider

```csharp
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Storage.Repositories;

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
            isSingleton: true,        // Singleton für Shared State
            trackPropertyChanges: true, // Auto-Save bei Änderungen
            autoLoad: true);           // Daten sofort verfügbar
    }

    public ReadOnlyObservableCollection<Customer> Customers => _customers.Items;

    public void AddCustomer(string name, string email)
    {
        _customers.Add(new Customer
        {
            Id = 0,  // Wird von LiteDB automatisch gesetzt
            Name = name,
            Email = email,
            CreatedAt = DateTime.Now
        });
        // ? Automatisch persistiert
    }

    public void UpdateCustomer(Customer customer, string newEmail)
    {
        customer.Email = newEmail;
        // ? Automatisch persistiert (via PropertyChanged)
    }

    public void DeleteCustomer(Customer customer)
    {
        _customers.Remove(customer);
        // ? Automatisch persistiert
    }
}
```

## Kernkonzepte

### DataStore Provider

Der `DataStoreProvider` ist der **empfohlene Weg**, um mit DataStores zu arbeiten:

```csharp
// ? Neu: Mit DataStoreProvider (AutoLoad + Singleton)
var store = provider.GetPersistent<Customer>(
    repositoryFactory,
    autoLoad: true);  // Daten sofort verfügbar!

// ? Alt: Manuell (ohne AutoLoad)
var repository = serviceProvider.GetRequiredService<IRepository<Customer>>();
var store = new PersistentDataStore<Customer>(repository);
store.Load();  // Manuell laden
```

**Features:**
- ? **Thread-Safe Singleton-Management**
- ? **AutoLoad-Funktionalität**
- ? **Automatic Repository Selection** (EntityBase ? LiteDB, sonst JSON)
- ? **PropertyChanged-Tracking** für automatische Persistierung

[Mehr über DataStore Provider ?](Docs/DataStore-Provider.md)

### PropertyChangedBinder: Automatisches Event-Tracking

Verwaltet `INotifyPropertyChanged`-Events mit automatischer Doppelbindungs-Verhinderung:

```csharp
// Zwei Modi verfügbar:

// 1. Manueller Modus: Explizite Kontrolle
var binder = new PropertyChangedBinder<Customer>(
    enabled: true,
    onEntityChanged: customer => repository.Update(customer));

binder.Attach(customer);  // Manuell binden
customer.Name = "Changed";  // ? Callback wird aufgerufen
binder.Detach(customer);  // Manuell entbinden

// 2. DataStore-Modus: Automatische Synchronisation (empfohlen)
using var subscription = binder.AttachToDataStore(customerStore);
// ? Alle Add/Remove-Operationen werden automatisch überwacht
```

**Anwendungsfälle:**
- PersistentDataStore (automatische Persistierung)
- ParentChildRelationship (ForeignKey-Tracking)
- Validation-Tracking
- Audit-Logging

[Mehr über PropertyChangedBinder ?](Docs/PropertyChangedBinder.md)

### ParentChildRelationship: 1:n-Beziehungen

Verwaltet automatisch gefilterte Collections mit PropertyChanged-Tracking:

```csharp
// Customer (Parent) ? Orders (Childs)
var relationship = new ParentChildRelationship<Customer, Order>(provider)
{
    Parent = selectedCustomer,
    IsChildFilter = (customer, order) => order.CustomerId == customer.Id
};

// UI bindet an relationship.Childs.Items
DataGrid.ItemsSource = relationship.Childs.Items;

// Automatische Updates bei:
// - DataSource.Add(order)
// - order.CustomerId = newId  (PropertyChanged)
// - DataSource.Remove(order)
```

[Mehr über ParentChildRelationship ?](Docs/ParentChildRelationship.md)

### JSON-Repository: Atomare Persistierung

JSON-Repositories speichern Daten in einer JSON-Datei mit atomarem Write:

```csharp
// Repository registrieren (StorageOptions werden automatisch erstellt!)
services.AddJsonRepository<Settings>(
    appSubFolder: "MyApp",
    fileNameBase: "settings",
    subFolder: "Config"
);
// ? Speichert unter: C:\Users\{Username}\Documents\MyApp\Config\settings.json
```

**Besonderheiten:**
- ? Atomares Schreiben via `.tmp` ? `.json` + `.bak`
- ? Backup-Datei bei jedem Schreibvorgang
- ? ReadOnly-Collection beim Laden
- ? Gesamte Collection wird geladen/geschrieben
- ? **Automatische StorageOptions-Erstellung** via Extension

[Mehr über Repositories ?](Docs/Repositories.md)

### LiteDB-Repository: Delta-Synchronisierung

LiteDB-Repositories bieten NoSQL-Datenbank mit feingranularen Updates:

```csharp
// EqualityComparer registrieren (WICHTIG für Delta-Detection!)
services.AddSingleton<IEqualityComparer<Customer>>(
    new FallbackEqualsComparer<Customer>());

// Repository registrieren (StorageOptions werden automatisch erstellt!)
services.AddLiteDbRepository<Customer>(
    appSubFolder: "MyApp",
    fileNameBase: "database",
    subFolder: "Databases"
);
// ? Speichert unter: C:\Users\{Username}\Documents\MyApp\Databases\database.db
```

**Besonderheiten:**
- ? Delta-Erkennung (nur geänderte Datensätze werden geschrieben)
- ? Transaktionale Updates
- ? `Update(T)` und `Delete(T)` für Einzeloperationen
- ? Automatische ID-Vergabe bei `Id = 0`
- ? Benötigt `IEqualityComparer<T>` für Delta-Detection
- ? **Automatische StorageOptions-Erstellung** via Extension

[Mehr über Repositories ?](Docs/Repositories.md)

### Storage Options

Storage Options werden jetzt **automatisch** von den Extension-Methoden erstellt:

```csharp
// ? Moderne API: Extension-Methode übernimmt alles
services.AddJsonRepository<Customer>(
    appSubFolder: "MyApp",
    fileNameBase: "customers",
    subFolder: "Data"
);
// ? C:\Users\{Username}\Documents\MyApp\Data\customers.json

// ? LiteDB
services.AddLiteDbRepository<Order>(
    appSubFolder: "MyApp",
    fileNameBase: "orders",
    subFolder: "Databases"
);
// ? C:\Users\{Username}\Documents\MyApp\Databases\orders.db

// ? Alte API (nicht mehr nötig):
// services.AddSingleton<IStorageOptions<Customer>>(
//     new JsonStorageOptions<Customer>("MyApp", "customers", "Data", null));
// services.AddJsonRepository<Customer>();
```

**Vorteile der neuen API:**
- ? **Weniger Code** - keine manuelle StorageOptions-Erstellung
- ? **Weniger Fehler** - Parameter werden automatisch validiert
- ? **Konsistent** - immer die gleiche Struktur
- ? **Testbar** - interne Überladungen für Tests mit custom rootFolder

[Mehr über Storage Options ?](Docs/Storage-Options.md)

## Best Practices

### ? Do's

1. **Neue Extension-Methoden verwenden**
   ```csharp
   // ? Moderne API
   services.AddLiteDbRepository<Customer>(
       "MyApp", "database", "Databases");
   
   // ? Alte API (funktioniert noch, aber veraltet)
   services.AddSingleton<IStorageOptions<Customer>>(...)
   services.AddLiteDbRepository<Customer>();
   ```

2. **EntityBase vererben**
   ```csharp
   public class Customer : EntityBase  // ? EntityBase bietet Id-Property
   {
       public string Name { get; set; } = "";
   }
   ```

3. **Equals/GetHashCode überschreiben**
   ```csharp
   public override bool Equals(object? obj)
   {
       if (obj is not Customer other) return false;
       return Id == other.Id && Name == other.Name;
   }
   
   public override int GetHashCode() 
       => HashCode.Combine(Id, Name);
   ```

4. **EqualityComparer für LiteDB registrieren**
   ```csharp
   services.AddSingleton<IEqualityComparer<Customer>>(
       new FallbackEqualsComparer<Customer>());
   ```

5. **DataStoreProvider für Singleton-State verwenden**
   ```csharp
   // ? Alle ViewModels teilen die gleichen Daten
   var store = provider.GetPersistent<Customer>(
       factory,
       isSingleton: true);
   ```

### ? Don'ts

1. **Keine sehr großen Datenmengen**
2. **Keine konkurrierenden Schreibzugriffe**
3. **Nicht Id manuell setzen bei LiteDB-Insert**
4. **Kein AutoLoad ohne Error-Handling**
5. **Keine Singletons für isolierte Operationen**

## Dokumentation

### Vollständige Dokumentation

- ?? [Storage Options](Docs/Storage-Options.md) - Wo und wie Daten gespeichert werden
- ?? [Repositories](Docs/Repositories.md) - JSON vs. LiteDB Repository
- ?? [DataStore Provider](Docs/DataStore-Provider.md) - Thread-Safe Singleton-Management
- ?? [PropertyChangedBinder](Docs/PropertyChangedBinder.md) - Automatisches PropertyChanged-Event-Tracking
- ?? [ParentChildRelationship](Docs/ParentChildRelationship.md) - 1:n-Beziehungsverwaltung
- ?? [API-Referenz](Docs/API-Referenz.md) - **Vollständige alphabetisch sortierte API-Dokumentation mit Querverweisen**

### Verwandte Projekte

- **[Common.Bootstrap](../Common.BootStrap/README.md)**: Modulares DI-Framework (erforderliche Abhängigkeit)

## Lizenz & Repository

- **Repository**: [https://github.com/ReneRose1971/Libraries](https://github.com/ReneRose1971/Libraries)
- **Lizenz**: Siehe LICENSE-Datei im Repository

## Support & Beiträge

Bei Fragen oder Problemen erstellen Sie bitte ein Issue im GitHub-Repository.
