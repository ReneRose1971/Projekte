# Storage Options - IStorageOptions<T>

Storage Options definieren, **wo** und **wie** Ihre Daten gespeichert werden. Mit `IStorageOptions<T>` erhalten Sie eine typsichere, flexible Konfiguration für jedes Repository.

## ?? Inhaltsverzeichnis

- [Konzept](#konzept)
- [Moderne API (Empfohlen)](#moderne-api-empfohlen)
- [Legacy API](#legacy-api)
- [IStorageOptions<T> Interface](#istoragetionst-interface)
- [JSON Storage Options](#json-storage-options)
- [LiteDB Storage Options](#litedb-storage-options)
- [Pfadstruktur](#pfadstruktur)
- [Best Practices](#best-practices)

## Konzept

### Das Problem ohne Storage Options

```csharp
// ? Alt: Pfad-Strings überall im Code
var repo1 = new JsonRepository<Customer>("C:\\MyApp\\Data\\customers.json");
var repo2 = new JsonRepository<Order>("C:\\MyApp\\Data\\orders.json");
var repo3 = new LiteDbRepository<Product>("C:\\MyApp\\Databases\\products.db");

// Keine Typsicherheit, fehleranfällig, schwer zu testen
```

### Die Lösung mit Storage Options

DataToolKit bietet **zwei APIs** für die Registrierung:

1. **Moderne API (Empfohlen)**: Extension-Methoden übernehmen StorageOptions-Erstellung automatisch
2. **Legacy API**: Manuelle StorageOptions-Registrierung (weiterhin unterstützt)

## Moderne API (Empfohlen)

### ? **NEU: Extension-Methoden mit automatischer StorageOptions-Erstellung**

Die **moderne API** erstellt `IStorageOptions<T>` automatisch - Sie übergeben einfach die Parameter:

```csharp
using DataToolKit.Abstractions.DI;
using Microsoft.Extensions.DependencyInjection;

public class MyDataModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // ? Moderne API: Extension-Methode übernimmt alles
        services.AddJsonRepository<Customer>(
            appSubFolder: "MyApp",
            fileNameBase: "customers",
            subFolder: "Data"
        );
        // ? IStorageOptions<Customer> wird automatisch erstellt und registriert!
        // ?? C:\Users\{Username}\Documents\MyApp\Data\customers.json

        // ? LiteDB mit automatischer StorageOptions-Erstellung
        services.AddLiteDbRepository<Order>(
            appSubFolder: "MyApp",
            fileNameBase: "orders",
            subFolder: "Databases"
        );
        // ? IStorageOptions<Order> und IRepository<Order> automatisch registriert!
        // ?? C:\Users\{Username}\Documents\MyApp\Databases\orders.db
    }
}
```

**Vorteile der modernen API:**
- ? **Weniger Code**: Keine manuelle StorageOptions-Erstellung nötig
- ? **Klarer**: Alle Parameter in einem Aufruf
- ? **Typsicher**: Compiler prüft alle Parameter
- ? **Konsistent**: Gleiche API für JSON und LiteDB

### Verfügbare Extension-Methoden

```csharp
namespace DataToolKit.Abstractions.DI;

public static class RepositoryRegistrationExtensions
{
    // JSON-Repository
    public static IServiceCollection AddJsonRepository<T>(
        this IServiceCollection services,
        string appSubFolder,
        string fileNameBase,
        string? subFolder = null,
        string? rootFolder = null)  // Optional: Custom Root (default: MyDocuments)
        where T : class;

    // LiteDB-Repository
    public static IServiceCollection AddLiteDbRepository<T>(
        this IServiceCollection services,
        string appSubFolder,
        string fileNameBase,
        string? subFolder = null,
        string? rootFolder = null)  // Optional: Custom Root
        where T : class, IEntity;
}
```

### Beispiele mit moderner API

#### Einfache Registrierung

```csharp
using DataToolKit.Abstractions.DI;

// JSON-Repository
services.AddJsonRepository<Customer>(
    "MyApp",          // appSubFolder
    "customers"       // fileNameBase
);
// ?? C:\Users\{Username}\Documents\MyApp\customers.json

// LiteDB-Repository (benötigt noch EqualityComparer!)
services.AddSingleton<IEqualityComparer<Order>>(
    new FallbackEqualsComparer<Order>());
services.AddLiteDbRepository<Order>(
    "MyApp",
    "database"
);
// ?? C:\Users\{Username}\Documents\MyApp\database.db
```

#### Mit Unterverzeichnissen

```csharp
// JSON in Data-Ordner
services.AddJsonRepository<Customer>(
    appSubFolder: "MyApp",
    fileNameBase: "customers",
    subFolder: "Data"
);
// ?? C:\Users\{Username}\Documents\MyApp\Data\customers.json

// LiteDB in Databases-Ordner
services.AddSingleton<IEqualityComparer<Order>>(
    new FallbackEqualsComparer<Order>());
services.AddLiteDbRepository<Order>(
    appSubFolder: "MyApp",
    fileNameBase: "orders",
    subFolder: "Databases"
);
// ?? C:\Users\{Username}\Documents\MyApp\Databases\orders.db
```

#### Mehrere Repositories

```csharp
public class DataModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // JSON-Repositories
        services.AddJsonRepository<Settings>("MyApp", "settings", "Config");
        services.AddJsonRepository<UserPreferences>("MyApp", "preferences", "Config");
        
        // LiteDB-Repositories (alle in gleicher Datenbank)
        services.AddSingleton<IEqualityComparer<Customer>>(new FallbackEqualsComparer<Customer>());
        services.AddSingleton<IEqualityComparer<Order>>(new FallbackEqualsComparer<Order>());
        services.AddSingleton<IEqualityComparer<Product>>(new FallbackEqualsComparer<Product>());
        
        services.AddLiteDbRepository<Customer>("MyApp", "myapp", "Databases");
        services.AddLiteDbRepository<Order>("MyApp", "myapp", "Databases");
        services.AddLiteDbRepository<Product>("MyApp", "myapp", "Databases");
        // ? Alle nutzen C:\Users\...\Documents\MyApp\Databases\myapp.db
        // Aber unterschiedliche Collections: "Customer", "Order", "Product"
    }
}
```

## Legacy API

### ?? **VERALTET: Manuelle StorageOptions-Registrierung**

Die **Legacy API** funktioniert weiterhin, ist aber **nicht mehr empfohlen**:

```csharp
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.Repositories;

// ?? Legacy: Manuelle Registrierung (funktioniert, aber veraltet)
services.AddSingleton<IStorageOptions<Customer>>(
    new JsonStorageOptions<Customer>("MyApp", "customers", "Data"));
services.AddJsonRepository<Customer>();

services.AddSingleton<IEqualityComparer<Order>>(
    new FallbackEqualsComparer<Order>());
services.AddSingleton<IStorageOptions<Order>>(
    new LiteDbStorageOptions<Order>("MyApp", "orders", "Databases"));
services.AddLiteDbRepository<Order>();
```

**Wann Legacy API verwenden?**
- ? Bestehender Code (Migration nicht zwingend nötig)
- ? Custom IStorageOptions-Implementierungen
- ? Sehr spezielle Anforderungen

**Migration zur modernen API:**

```csharp
// ?? ALT (Legacy):
services.AddSingleton<IStorageOptions<Customer>>(
    new JsonStorageOptions<Customer>("MyApp", "customers", "Data"));
services.AddJsonRepository<Customer>();

// ? NEU (Moderne API):
services.AddJsonRepository<Customer>(
    appSubFolder: "MyApp",
    fileNameBase: "customers",
    subFolder: "Data");
```

## IStorageOptions<T> Interface

```csharp
namespace DataToolKit.Abstractions.Repositories;

public interface IStorageOptions<T>
{
    /// <summary>
    /// Name des Anwendungs-Unterordners unterhalb von "Eigene Dokumente".
    /// Beispiel: "MyApp" ? C:\Users\...\Documents\MyApp\
    /// </summary>
    string AppSubFolder { get; }

    /// <summary>
    /// Optionaler zusätzlicher Unterordner.
    /// Beispiel: "Data" ? C:\Users\...\Documents\MyApp\Data\
    /// </summary>
    string? SubFolder { get; }

    /// <summary>
    /// Basisname der Datei ohne Erweiterung.
    /// Beispiel: "customers" ? customers.json oder customers.db
    /// </summary>
    string FileNameBase { get; }

    /// <summary>
    /// Absoluter Pfad zum "Eigene Dokumente"-Ordner.
    /// </summary>
    string RootFolder { get; }

    /// <summary>
    /// Vollständiger Pfad zum Zielverzeichnis.
    /// </summary>
    string EffectiveRoot { get; }

    /// <summary>
    /// Vollständiger Dateipfad inklusive Name und Erweiterung.
    /// Beispiel: C:\Users\...\Documents\MyApp\Data\customers.json
    /// </summary>
    string FullPath { get; }
}
```

## JSON Storage Options

### Konstruktor

```csharp
public JsonStorageOptions<T>(
    string appSubFolder,     // Pflicht: App-Verzeichnis
    string fileNameBase,     // Pflicht: Dateiname ohne .json
    string? subFolder = null // Optional: Unterverzeichnis
)
```

### Beispiele

#### Einfaches Beispiel

```csharp
var options = new JsonStorageOptions<Customer>(
    appSubFolder: "MyApp",
    fileNameBase: "customers"
);

// Resultat:
// C:\Users\YourName\Documents\MyApp\customers.json
```

#### Mit Unterverzeichnis

```csharp
var options = new JsonStorageOptions<Customer>(
    appSubFolder: "MyApp",
    fileNameBase: "customers",
    subFolder: "Data"
);

// Resultat:
// C:\Users\YourName\Documents\MyApp\Data\customers.json
```

#### Mehrere Entitäten

```csharp
// Alle im gleichen Verzeichnis
services.AddSingleton<IStorageOptions<Customer>>(
    new JsonStorageOptions<Customer>("MyApp", "customers", "Data"));

services.AddSingleton<IStorageOptions<Order>>(
    new JsonStorageOptions<Order>("MyApp", "orders", "Data"));

services.AddSingleton<IStorageOptions<Product>>(
    new JsonStorageOptions<Product>("MyApp", "products", "Data"));

// Resultat:
// C:\Users\...\Documents\MyApp\Data\customers.json
// C:\Users\...\Documents\MyApp\Data\orders.json
// C:\Users\...\Documents\MyApp\Data\products.json
```

## LiteDB Storage Options

### Konstruktor

```csharp
public LiteDbStorageOptions<T>(
    string appSubFolder,     // Pflicht: App-Verzeichnis
    string fileNameBase,     // Pflicht: Dateiname ohne .db
    string? subFolder = null // Optional: Unterverzeichnis
)
```

### Besonderheiten

LiteDB-Options haben zusätzlich:

```csharp
public string GetConnectionString()
{
    return $"Filename={FullPath}";
}
```

### Beispiele

#### Einzelne Datenbank

```csharp
var options = new LiteDbStorageOptions<Customer>(
    appSubFolder: "MyApp",
    fileNameBase: "database",
    subFolder: "Databases"
);

// Resultat:
// C:\Users\...\Documents\MyApp\Databases\database.db
// Connection String: "Filename=C:\Users\...\Documents\MyApp\Databases\database.db"
```

#### Mehrere Entitäten, gleiche Datenbank

```csharp
// Alle in der gleichen Datenbank-Datei
services.AddSingleton<IStorageOptions<Customer>>(
    new LiteDbStorageOptions<Customer>("MyApp", "myapp", "Databases"));

services.AddSingleton<IStorageOptions<Order>>(
    new LiteDbStorageOptions<Order>("MyApp", "myapp", "Databases"));

services.AddSingleton<IStorageOptions<Product>>(
    new LiteDbStorageOptions<Product>("MyApp", "myapp", "Databases"));

// Alle nutzen: C:\Users\...\Documents\MyApp\Databases\myapp.db
// Aber unterschiedliche Collections innerhalb der Datenbank:
// - Customer ? Collection "Customer"
// - Order ? Collection "Order"
// - Product ? Collection "Product"
```

#### Getrennte Datenbanken

```csharp
// Jede Entität in eigener Datenbank
services.AddSingleton<IStorageOptions<Customer>>(
    new LiteDbStorageOptions<Customer>("MyApp", "customers", "Databases"));

services.AddSingleton<IStorageOptions<Order>>(
    new LiteDbStorageOptions<Order>("MyApp", "orders", "Databases"));

// Resultat:
// C:\Users\...\Documents\MyApp\Databases\customers.db
// C:\Users\...\Documents\MyApp\Databases\orders.db
```

## Pfadstruktur

### Standard-Pfadaufbau

```
C:\Users\{Username}\Documents\  ? RootFolder (Environment.SpecialFolder.MyDocuments)
    ??? {AppSubFolder}\          ? Ihre App (z.B. "MyApp")
        ??? {SubFolder?}\        ? Optional (z.B. "Data" oder "Databases")
            ??? {FileNameBase}.{ext}  ? Datei (z.B. "customers.json")
```

### Konkrete Beispiele

#### Beispiel 1: Einfache Struktur

```csharp
new JsonStorageOptions<Customer>("TypeTutor", "users")

// Pfad:
// C:\Users\John\Documents\TypeTutor\users.json
```

#### Beispiel 2: Organisiert mit Unterordnern

```csharp
new JsonStorageOptions<Customer>("TypeTutor", "users", "Data")
new JsonStorageOptions<Settings>("TypeTutor", "settings", "Config")
new LiteDbStorageOptions<Exercise>("TypeTutor", "exercises", "Databases")

// Pfade:
// C:\Users\John\Documents\TypeTutor\Data\users.json
// C:\Users\John\Documents\TypeTutor\Config\settings.json
// C:\Users\John\Documents\TypeTutor\Databases\exercises.db
```

#### Beispiel 3: Mehrere Apps

```csharp
// App 1
new JsonStorageOptions<Customer>("MyShop", "customers", "Data")
// C:\Users\...\Documents\MyShop\Data\customers.json

// App 2
new JsonStorageOptions<Contact>("MyContacts", "contacts", "Data")
// C:\Users\...\Documents\MyContacts\Data\contacts.json
```

### Verzeichnisanlage

> ?? **Automatisch**: Storage Options erstellen das Zielverzeichnis automatisch beim Konstruktor-Aufruf.

```csharp
var options = new JsonStorageOptions<Customer>("MyApp", "customers", "Data");
// ? C:\Users\...\Documents\MyApp\Data\ wird angelegt, falls nicht vorhanden
```

## Best Practices

### ? Do's

**1. Moderne API verwenden:**
```csharp
// ? Empfohlen: Moderne API
services.AddJsonRepository<Customer>("MyApp", "customers", "Data");

// ?? Veraltet: Legacy API
services.AddSingleton<IStorageOptions<Customer>>(
    new JsonStorageOptions<Customer>("MyApp", "customers", "Data"));
services.AddJsonRepository<Customer>();
```

**2. Konsistente Benennung:**
```csharp
// ? Gut: Einheitliche Struktur
services.AddJsonRepository<Customer>("MyApp", "customers", "Data");
services.AddJsonRepository<Order>("MyApp", "orders", "Data");
services.AddJsonRepository<Product>("MyApp", "products", "Data");
```

**3. Unterordner für Organisation:**
```csharp
// ? Gut: Organisierte Struktur
services.AddJsonRepository<T>("MyApp", "file", "Data");       // JSON-Dateien
services.AddLiteDbRepository<T>("MyApp", "db", "Databases");  // Datenbanken
services.AddJsonRepository<T>("MyApp", "cfg", "Config");      // Konfigurationen
```

**4. EqualityComparer für LiteDB:**
```csharp
// ? Gut: EqualityComparer vor LiteDB-Repository registrieren
services.AddSingleton<IEqualityComparer<Order>>(
    new FallbackEqualsComparer<Order>());
services.AddLiteDbRepository<Order>("MyApp", "orders", "Databases");
```

### ? Don'ts

**1. Keine inkonsistenten App-Ordner:**
```csharp
// ? Schlecht: Verschiedene App-Ordner
services.AddJsonRepository<Customer>("MyApp", "customers");
services.AddJsonRepository<Order>("MyApplication", "orders");  // Anderer App-Name!
services.AddJsonRepository<Product>("App", "products");        // Noch ein anderer!
```

**2. Keine zu tiefen Verschachtelungen:**
```csharp
// ? Schlecht: Zu tief verschachtelt
services.AddJsonRepository<Customer>(
    "MyApp", 
    "customers", 
    "Data\\Production\\Year2024\\Month12\\Day01");  // ? Zu komplex!

// ? Besser: Flache Struktur
services.AddJsonRepository<Customer>("MyApp", "customers-2024-12", "Data");
```

**3. Keine Sonderzeichen in Dateinamen:**
```csharp
// ? Schlecht: Ungültige Zeichen
services.AddJsonRepository<Customer>("MyApp", "customers:data");     // : ist ungültig
services.AddJsonRepository<Customer>("MyApp", "customers/orders");   // / ist ungültig

// ? Gut: Nur alphanumerisch und Bindestriche
services.AddJsonRepository<Customer>("MyApp", "customers-data");
services.AddJsonRepository<Customer>("MyApp", "customers-orders");
```

**4. Kein Missing-EqualityComparer bei LiteDB:**
```csharp
// ? Schlecht: LiteDB ohne EqualityComparer
services.AddLiteDbRepository<Order>("MyApp", "orders", "Databases");
// ? Delta-Detection funktioniert nicht!

// ? Gut: EqualityComparer registrieren
services.AddSingleton<IEqualityComparer<Order>>(
    new FallbackEqualsComparer<Order>());
services.AddLiteDbRepository<Order>("MyApp", "orders", "Databases");
```

## Integration mit Common.Bootstrap

Storage Options werden typischerweise zusammen mit Common.Bootstrap verwendet:

```csharp
using Common.Bootstrap;
using Common.Bootstrap.Defaults;
using DataToolKit.Abstractions.DI;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.Repositories;
using Microsoft.Extensions.DependencyInjection;

public class MyDataModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // JSON-Repository
        services.AddSingleton<IStorageOptions<Customer>>(
            new JsonStorageOptions<Customer>("MyApp", "customers", "Data"));
        services.AddJsonRepository<Customer>();
        
        // LiteDB-Repository (benötigt zusätzlich EqualityComparer!)
        services.AddSingleton<IEqualityComparer<Order>>(
            new FallbackEqualsComparer<Order>());
        services.AddSingleton<IStorageOptions<Order>>(
            new LiteDbStorageOptions<Order>("MyApp", "orders", "Databases"));
        services.AddLiteDbRepository<Order>();
    }
}
```

Mehr Details: [Common.Bootstrap EqualityComparer ?](../../Common.BootStrap/Docs/EqualityComparer.md)

## Weiterführende Themen

- [Repositories ??](Repositories.md) - JSON & LiteDB Repositories
- [DataStore Provider ??](DataStore-Provider.md) - Thread-Safe Singleton-Management
- [API-Referenz ??](API-Referenz.md) - Vollständige API-Dokumentation
- [Zurück zur Übersicht ??](../README.md)
