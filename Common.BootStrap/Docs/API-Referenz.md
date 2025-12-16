# Common.Bootstrap – API-Referenz

Vollständige alphabetisch sortierte API-Dokumentation für alle öffentlichen Schnittstellen und Klassen.

## ?? Inhaltsverzeichnis

- [CommonBootstrapServiceModule](#commonbootstrapservicemodule)
- [FallbackEqualsComparer\<T\>](#fallbackequalscomparert)
- [IServiceModule](#iservicemodule)
- [ServiceCollectionEqualityComparerExtensions](#servicecollectionequalitycomparerextensions)
- [ServiceCollectionModuleExtensions](#servicecollectionmoduleextensions)

---

## CommonBootstrapServiceModule

**Namespace:** `Common.Bootstrap`  
**Assembly:** Common.Bootstrap.dll  
**Implementiert:** [`IServiceModule`](#iservicemodule)

### Beschreibung

Service-Modul für Common.BootStrap, das alle konkreten `IEqualityComparer<T>`-Implementierungen aus der Assembly automatisch als Singleton registriert.

### Wichtige Hinweise

?? **Kein automatischer Fallback:** Es gibt keinen automatischen Fallback-Comparer für alle Typen. Entwickler müssen für jeden Typ, der einen `IEqualityComparer<T>` benötigt, eine konkrete Implementierung registrieren.

### Methoden

#### `void Register(IServiceCollection services)`

Registriert alle konkreten `IEqualityComparer<T>`-Implementierungen aus der Common.Bootstrap-Assembly.

**Parameter:**
- `services` (`IServiceCollection`): Die zu erweiternde Service-Collection.

**Verhalten:**
- Scannt die Assembly nach konkreten `IEqualityComparer<T>`-Implementierungen
- Registriert gefundene Comparer als Singleton (idempotent via `TryAddSingleton`)

### Verwendungsbeispiel

```csharp
using Common.Bootstrap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Registriert alle EqualityComparer aus Common.Bootstrap
new CommonBootstrapServiceModule().Register(builder.Services);

var app = builder.Build();
```

### Siehe auch

- [`IServiceModule`](#iservicemodule) – Basis-Interface
- [`ServiceCollectionEqualityComparerExtensions`](#servicecollectionequalitycomparerextensions) – Verwendete Extension-Methode
- [`FallbackEqualsComparer<T>`](#fallbackequalscomparert) – Generischer Fallback-Comparer

---

## FallbackEqualsComparer\<T\>

**Namespace:** `Common.Bootstrap.Defaults`  
**Assembly:** Common.Bootstrap.dll  
**Implementiert:** `IEqualityComparer<T>`

### Beschreibung

Fallback-Comparer, der ausschließlich `x.Equals(y)` und `obj.GetHashCode()` verwendet. Nutzt keine Reflexion und keine Feld-/Property-Vergleiche.

### Typ-Parameter

- `T` – Der zu vergleichende Typ.

### Anwendungsfälle

? **Ideal für:**
- Typen, die `Equals` und `GetHashCode` korrekt überschrieben haben
- Schnelle Vergleiche ohne Reflexions-Overhead
- Default-Implementierung bei fehlenden typspezifischen Comparern

? **Nicht geeignet für:**
- Typen ohne überschriebenes `Equals`/`GetHashCode` (nutzt dann Object-Identity)
- Komplexe Vergleichslogik, die über `Equals` hinausgeht

### Methoden

#### `bool Equals(T? x, T? y)`

Vergleicht zwei Objekte auf Gleichheit mittels `x.Equals(y)`.

**Parameter:**
- `x` (`T?`): Das erste zu vergleichende Objekt.
- `y` (`T?`): Das zweite zu vergleichende Objekt.

**Rückgabewert:**
- `true`, wenn die Objekte gleich sind; andernfalls `false`.

**Verhalten:**
1. Wenn `x` und `y` referenzidentisch sind ? `true`
2. Wenn `x` oder `y` null ist ? `false`
3. Sonst: Rückgabe von `x.Equals(y)`

#### `int GetHashCode(T obj)`

Liefert den Hash-Code des Objekts mittels `obj.GetHashCode()`.

**Parameter:**
- `obj` (`T`): Das Objekt, für das der Hash-Code berechnet werden soll.

**Rückgabewert:**
- Der Hash-Code des Objekts.

**Exceptions:**
- `ArgumentNullException`: Wenn `obj` null ist.

### Verwendungsbeispiel

```csharp
using Common.Bootstrap.Defaults;
using Microsoft.Extensions.DependencyInjection;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    public override bool Equals(object? obj)
        => obj is Customer c && Id == c.Id;

    public override int GetHashCode()
        => Id.GetHashCode();
}

// Manuelle Registrierung
services.AddSingleton<IEqualityComparer<Customer>>(
    new FallbackEqualsComparer<Customer>());

// Verwendung
var comparer = serviceProvider.GetRequiredService<IEqualityComparer<Customer>>();
bool areEqual = comparer.Equals(customer1, customer2);
```

### Siehe auch

- [`ServiceCollectionEqualityComparerExtensions`](#servicecollectionequalitycomparerextensions) – Automatische Comparer-Registrierung
- [EqualityComparer-Management](EqualityComparer.md) – Vollständiger Leitfaden

---

## IServiceModule

**Namespace:** `Common.Bootstrap`  
**Assembly:** Common.Bootstrap.dll

### Beschreibung

Schnittstelle für modulare DI-Registrierungen. Jede Bibliothek implementiert ein Modul, um ihre eigenen Services anzumelden.

### Design-Philosophie

Das Modul-Pattern ermöglicht:
- ? **Wiederverwendbarkeit**: Module können in verschiedenen Projekten genutzt werden
- ? **Trennung**: Jede Bibliothek ist für ihre eigene DI-Konfiguration verantwortlich
- ? **Testbarkeit**: Module können isoliert getestet werden
- ? **Übersichtlichkeit**: Klare Struktur statt einer riesigen `Program.cs`

### Methoden

#### `void Register(IServiceCollection services)`

Führt alle DI-Registrierungen dieses Moduls aus.

**Parameter:**
- `services` (`IServiceCollection`): Die zu erweiternde Service-Collection.

**Implementierungsrichtlinien:**
1. **Idempotenz**: Nutzen Sie `TryAdd*`-Methoden für sichere Mehrfach-Aufrufe
2. **Keine Seiteneffekte**: Keine I/O, keine globalen Zustandsänderungen
3. **Fokussiert**: Ein Modul pro Verantwortungsbereich
4. **Fehlerbehandlung**: Keine Exceptions bei fehlenden optionalen Dependencies

### Verwendungsbeispiel

```csharp
using Common.Bootstrap;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp.Infrastructure;

/// <summary>
/// Service-Modul für Infrastruktur-Services.
/// </summary>
public sealed class InfrastructureModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // Singleton-Services
        services.AddSingleton<IMyService, MyService>();
        
        // Scoped-Services
        services.AddScoped<IMyRepository, MyRepository>();
        
        // Transient-Services
        services.AddTransient<IMyFactory, MyFactory>();
        
        // EqualityComparer automatisch registrieren
        services.AddEqualityComparersFromAssembly<InfrastructureModule>();
    }
}
```

### Best Practices

#### ? Do's

```csharp
// Ein Modul pro Verantwortungsbereich
public class DatabaseModule : IServiceModule { }
public class MessagingModule : IServiceModule { }
public class ValidationModule : IServiceModule { }

// Idempotente Registrierungen
services.TryAddSingleton<IMyService, MyService>();

// Assembly-spezifisches Scanning
services.AddEqualityComparersFromAssembly<MyModule>();
```

#### ? Don'ts

```csharp
// Nicht: Alles in ein Modul packen
public class GiantModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // 100+ Zeilen Code...
    }
}

// Nicht: Zirkuläre Abhängigkeiten
public class ModuleA : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        new ModuleB().Register(services); // ? Nicht manuell aufrufen
    }
}
```

### Siehe auch

- [`ServiceCollectionModuleExtensions`](#servicecollectionmoduleextensions) – Automatisches Modul-Scanning
- [`CommonBootstrapServiceModule`](#commonbootstrapservicemodule) – Konkrete Implementierung
- [Service-Module Dokumentation](ServiceModules.md) – Vollständiger Leitfaden

---

## ServiceCollectionEqualityComparerExtensions

**Namespace:** `Common.Extensions`  
**Assembly:** Common.Bootstrap.dll

### Beschreibung

Extensions zur automatischen Registrierung von `IEqualityComparer<T>`-Implementierungen aus Assemblies in `IServiceCollection`.

### Methoden

#### `IServiceCollection AddEqualityComparersFromAssembly<TMarker>(this IServiceCollection services)`

Scannt die Assembly des angegebenen Marker-Typs nach allen konkreten Implementierungen von `IEqualityComparer<T>` und registriert sie als Singleton.

**Typ-Parameter:**
- `TMarker` – Ein beliebiger Typ aus der zu scannenden Assembly (z.B. das ServiceModule selbst).

**Parameter:**
- `services` (`IServiceCollection`): Die zu erweiternde Service-Collection.

**Rückgabewert:**
- Die erweiterte `IServiceCollection` für Fluent-API.

**Exceptions:**
- `ArgumentNullException`: Wenn `services` null ist.

### Filter-Kriterien

Diese Methode findet nur Typen, die **alle** folgenden Bedingungen erfüllen:

? **Gefunden werden:**
- Konkrete Klassen (nicht abstract, nicht interface)
- Öffentlich (`IsPublic`) oder nested public (`IsNestedPublic`)
- Haben einen öffentlichen parameterlosen Konstruktor
- Keine offenen generischen Typen (kein `ContainsGenericParameters`)
- Implementieren `IEqualityComparer<T>`

? **NICHT gefunden werden:**
- Abstrakte Klassen und Interfaces
- Generische Klassen mit ungebundenen Typparametern (z.B. `FallbackEqualsComparer<T>`)
- Klassen ohne öffentlichen parameterlosen Konstruktor
- Private oder internal Klassen

### Registrierungs-Verhalten

- **Idempotent**: Nutzt `TryAddSingleton` – bestehende Registrierungen werden nicht überschrieben
- **Singleton**: Alle gefundenen Comparer werden als Singleton registriert
- **Fehlertoleranz**: `ReflectionTypeLoadException` wird automatisch behandelt

### Verwendungsbeispiel

```csharp
using Common.Extensions;
using Common.Bootstrap;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp.Data;

// Diese Comparer werden automatisch gefunden:
public class CustomerComparer : IEqualityComparer<Customer>
{
    public bool Equals(Customer? x, Customer? y) 
        => x?.Id == y?.Id;
    
    public int GetHashCode(Customer obj) 
        => obj.Id.GetHashCode();
}

public class OrderComparer : IEqualityComparer<Order>
{
    public bool Equals(Order? x, Order? y) 
        => x?.OrderNumber == y?.OrderNumber;
    
    public int GetHashCode(Order obj) 
        => obj.OrderNumber.GetHashCode();
}

// Service-Modul
public class DataModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // Scannt die Assembly nach allen Comparer-Implementierungen
        services.AddEqualityComparersFromAssembly<DataModule>();
        
        // CustomerComparer und OrderComparer sind jetzt registriert!
    }
}

// Verwendung
var customerComparer = serviceProvider
    .GetRequiredService<IEqualityComparer<Customer>>();
```

### Fortgeschrittene Szenarien

#### Mehrere Assemblies scannen

```csharp
public class AppBootstrapModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // Scanne mehrere Assemblies
        services.AddEqualityComparersFromAssembly<AppBootstrapModule>();
        services.AddEqualityComparersFromAssembly<DataModule>();
        services.AddEqualityComparersFromAssembly<DomainModule>();
    }
}
```

#### Manuelle Override-Registrierung

```csharp
public void Register(IServiceCollection services)
{
    // Korrekt: Erst manuell, dann scannen
    services.AddSingleton<IEqualityComparer<Customer>>(
        new CustomCustomerComparer());
    services.AddEqualityComparersFromAssembly<MyModule>(); 
    // ? TryAdd überspringt Customer
    
    // Nicht: Nach dem Scannen überschreiben
    services.AddEqualityComparersFromAssembly<MyModule>();
    services.AddSingleton<IEqualityComparer<Customer>>(
        new CustomCustomerComparer()); 
    // ?? Überschreibt nicht wegen TryAdd
}
```

### Siehe auch

- [`FallbackEqualsComparer<T>`](#fallbackequalscomparert) – Generischer Fallback-Comparer
- [`CommonBootstrapServiceModule`](#commonbootstrapservicemodule) – Nutzt diese Extension
- [EqualityComparer-Management](EqualityComparer.md) – Vollständiger Leitfaden

---

## ServiceCollectionModuleExtensions

**Namespace:** `Common.Bootstrap`  
**Assembly:** Common.Bootstrap.dll

### Beschreibung

Erweiterungen zur automatischen Erkennung und Ausführung aller `IServiceModule`-Implementierungen aus Assemblies.

### Methoden

#### `IServiceCollection AddModulesFromAssemblies(this IServiceCollection services, params Assembly[] assemblies)`

Sucht in den angegebenen (oder allen geladenen) Assemblies nach Klassen, die `IServiceModule` implementieren, erzeugt Instanzen und ruft `Register` auf.

**Parameter:**
- `services` (`IServiceCollection`): Die zu erweiternde Service-Collection.
- `assemblies` (`params Assembly[]`): Liste der zu scannenden Assemblies. Wenn leer oder null, werden alle aktuell geladenen Assemblies (`AppDomain.CurrentDomain.GetAssemblies()`) gescannt.

**Rückgabewert:**
- Die gleiche `IServiceCollection` für Fluent-API-Verkettung.

**Exceptions:**
- `MissingMethodException`: Wenn ein gefundenes `IServiceModule` keinen öffentlichen parameterlosen Konstruktor hat.
- `InvalidOperationException`: Wenn während der Registrierung in einem Modul ein Fehler auftritt.

### Filter-Kriterien

**Gefunden werden:**
- Konkrete, nicht-abstrakte Klassen, die `IServiceModule` implementieren
- Öffentliche und interne Klassen
- Nested Classes (auch private, wenn zugänglich)

**NICHT gefunden werden:**
- Abstrakte Klassen und Interfaces
- Generische Klassen mit ungebundenen Typparametern
- Klassen ohne öffentlichen parameterlosen Konstruktor

### Fehlerbehandlung

`ReflectionTypeLoadException` wird automatisch behandelt – nur erfolgreich geladene Typen werden verarbeitet, fehlende Abhängigkeiten führen nicht zum Abbruch.

### Verwendungsbeispiele

#### Einzelne Assembly scannen

```csharp
using Common.Bootstrap;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Scannt nur die App-Assembly
builder.Services.AddModulesFromAssemblies(typeof(Program).Assembly);

var app = builder.Build();
await app.RunAsync();
```

#### Mehrere Assemblies scannen

```csharp
var builder = Host.CreateApplicationBuilder(args);

// Scannt mehrere Assemblies
builder.Services.AddModulesFromAssemblies(
    typeof(Program).Assembly,                  // App-Assembly
    typeof(InfrastructureModule).Assembly,     // Infrastructure
    typeof(DomainModule).Assembly              // Domain
);

var app = builder.Build();
```

#### Alle geladenen Assemblies scannen

```csharp
var builder = Host.CreateApplicationBuilder(args);

// ?? Scannt ALLE geladenen Assemblies (inkl. System-Assemblies)
// Nicht empfohlen für Produktion!
builder.Services.AddModulesFromAssemblies();

var app = builder.Build();
```

### Best Practices

#### ? Do's

```csharp
// 1. Explizite Assembly-Liste (empfohlen)
builder.Services.AddModulesFromAssemblies(
    typeof(Program).Assembly,
    typeof(MyModule).Assembly
);

// 2. Nur eigene Assemblies scannen
builder.Services.AddModulesFromAssemblies(
    AppDomain.CurrentDomain.GetAssemblies()
        .Where(a => a.FullName?.StartsWith("MyCompany") == true)
        .ToArray()
);
```

#### ? Don'ts

```csharp
// Nicht: Alle Assemblies scannen (Performance-Problem)
builder.Services.AddModulesFromAssemblies(); // ?

// Nicht: Module manuell instanziieren
var module = new MyModule();
module.Register(builder.Services); // ? Nutzen Sie AddModulesFromAssemblies

// Nicht: Doppelte Registrierungen
builder.Services.AddModulesFromAssemblies(typeof(Program).Assembly);
builder.Services.AddModulesFromAssemblies(typeof(Program).Assembly); // ? Redundant
```

### Siehe auch

- [`IServiceModule`](#iservicemodule) – Basis-Interface
- [`CommonBootstrapServiceModule`](#commonbootstrapservicemodule) – Beispiel-Implementierung
- [Modulare Registrierung](Modulare-Registrierung.md) – Vollständiger Leitfaden

---

## Verwandte Dokumentation

- ?? [Service-Module verstehen](ServiceModules.md)
- ?? [Modulare Registrierung](Modulare-Registrierung.md)
- ?? [EqualityComparer-Management](EqualityComparer.md)
- ?? [DataToolKit API-Referenz](../../DataToolKit/Docs/API-Referenz.md)

---

## Lizenz & Repository

- **Repository**: [https://github.com/ReneRose1971/Libraries](https://github.com/ReneRose1971/Libraries)
- **Lizenz**: Siehe LICENSE-Datei im Repository
