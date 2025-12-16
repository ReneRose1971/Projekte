# EqualityComparer-Management

Common.Bootstrap bietet Tools zur automatischen Registrierung von `IEqualityComparer<T>`-Implementierungen in Ihrer Anwendung.

## ?? Inhaltsverzeichnis

- [Überblick](#überblick)
- [Grundkonzept](#grundkonzept)
- [Automatisches Scanning](#automatisches-scanning)
- [Manuelle Registrierung](#manuelle-registrierung)
- [FallbackEqualsComparer](#fallbackequalscomparer)
- [Best Practices](#best-practices)
- [Häufige Probleme](#häufige-probleme)

## Überblick

`IEqualityComparer<T>` wird in .NET häufig benötigt für:
- Repository-Operationen (z.B. LiteDB Delta-Detection)
- Collection-Operationen (Distinct, Except, Union)
- Dictionary-Keys und HashSet-Elemente

**Common.Bootstrap** bietet einen strukturierten Ansatz für die Registrierung dieser Comparer.

> ?? **Wichtig**: Es gibt **keinen automatischen Fallback** mehr. Sie müssen für jeden Typ, der einen Comparer benötigt, explizit einen registrieren.

## Grundkonzept

### Das Problem

```csharp
// ? Fehler: Kein IEqualityComparer<Customer> registriert
public class OrderService
{
    public OrderService(IRepository<Customer> repository)
    {
        // LiteDbRepository benötigt IEqualityComparer<Customer>
        // ? InvalidOperationException beim Auflösen
    }
}
```

### Die Lösung

```csharp
// ? Explizite Registrierung
public class MyModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // Comparer explizit registrieren
        services.AddSingleton<IEqualityComparer<Customer>>(
            new FallbackEqualsComparer<Customer>());
        
        // Repository kann jetzt aufgelöst werden
        services.AddLiteDbRepository<Customer>();
    }
}
```

**Vorteile:**
- ? **Typsicherheit**: Compiler prüft, ob `IEqualityComparer<T>` registriert ist
- ? **DI-Integration**: Keine hartcodierten Comparer im Code
- ? **Testbarkeit**: Einfach zu mocken
- ? **Explizit**: Klare Abhängigkeiten

## Automatisches Scanning

### Assembly-Scanning nutzen

Die Extension-Method `AddEqualityComparersFromAssembly<TMarker>()` scannt eine Assembly und registriert **alle gefundenen Comparer**:

```csharp
using Common.Extensions;
using Common.Bootstrap;

public class MyModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // Scannt die Assembly nach allen IEqualityComparer<T>-Implementierungen
        services.AddEqualityComparersFromAssembly<MyModule>();
    }
}
```

### Was wird gefunden?

Das Scanning findet nur Typen, die **alle** Kriterien erfüllen:

? Konkrete Klassen (nicht abstract, nicht interface)  
? Öffentlich oder nested public  
? Haben einen öffentlichen parameterlosen Konstruktor  
? Keine offenen generischen Typen (kein `<T>`)  
? Implementieren `IEqualityComparer<T>`

### Beispiel: Automatisch gefundene Comparer

```csharp
namespace MyApp.Domain.Comparers;

// ? Wird gefunden und registriert
public class CustomerComparer : IEqualityComparer<Customer>
{
    public bool Equals(Customer? x, Customer? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return x.Id == y.Id;
    }

    public int GetHashCode(Customer obj) 
        => obj?.Id.GetHashCode() ?? 0;
}

// ? Wird auch gefunden (nested public)
public class ComparerTests
{
    public class OrderComparer : IEqualityComparer<Order>
    {
        public bool Equals(Order? x, Order? y) => x?.Id == y?.Id;
        public int GetHashCode(Order obj) => obj.Id.GetHashCode();
    }
}

// ? Wird NICHT gefunden (offener generischer Typ)
public class GenericComparer<T> : IEqualityComparer<T>
{
    public bool Equals(T? x, T? y) => EqualityComparer<T>.Default.Equals(x, y);
    public int GetHashCode(T obj) => obj?.GetHashCode() ?? 0;
}

// ? Wird NICHT gefunden (kein öffentlicher parameterloser Konstruktor)
public class ConfigurableComparer : IEqualityComparer<Product>
{
    private readonly bool _caseSensitive;
    
    public ConfigurableComparer(bool caseSensitive) // ? Kein parameterloser Konstruktor
    {
        _caseSensitive = caseSensitive;
    }
    
    public bool Equals(Product? x, Product? y) => /* ... */;
    public int GetHashCode(Product obj) => /* ... */;
}
```

### Mehrere Assemblies scannen

```csharp
using Common.Extensions;

public class MyModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // Scannt mehrere Assemblies
        services.AddEqualityComparersFromAssembly<MyModule>();        // Eigene Assembly
        services.AddEqualityComparersFromAssembly<SharedLibrary>();   // Shared Library
    }
}
```

## Manuelle Registrierung

### Einfache Registrierung

```csharp
using Microsoft.Extensions.DependencyInjection;
using Common.Bootstrap.Defaults;

public class MyModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // Direkte Instanz-Registrierung
        services.AddSingleton<IEqualityComparer<Customer>>(
            new FallbackEqualsComparer<Customer>());
        
        services.AddSingleton<IEqualityComparer<Order>>(
            new FallbackEqualsComparer<Order>());
    }
}
```

### Benutzerdefinierte Comparer

```csharp
// Ihr eigener Comparer mit spezifischer Logik
public class CustomerComparer : IEqualityComparer<Customer>
{
    public bool Equals(Customer? x, Customer? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        
        // Vergleicht Id UND Email
        return x.Id == y.Id && x.Email == y.Email;
    }

    public int GetHashCode(Customer obj)
    {
        if (obj is null) throw new ArgumentNullException(nameof(obj));
        return HashCode.Combine(obj.Id, obj.Email);
    }
}

// Registrierung
public class MyModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IEqualityComparer<Customer>>(
            new CustomerComparer());
    }
}
```

### Factory-basierte Registrierung

```csharp
public class MyModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // Factory-Methode für komplexe Initialisierung
        services.AddSingleton<IEqualityComparer<Product>>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var caseSensitive = config.GetValue<bool>("Product:CaseSensitiveComparison");
            
            return new ProductComparer(caseSensitive);
        });
    }
}
```

## FallbackEqualsComparer

### Was ist der FallbackEqualsComparer?

`FallbackEqualsComparer<T>` ist eine Utility-Klasse, die einfach `x.Equals(y)` aufruft:

```csharp
namespace Common.Bootstrap.Defaults;

public sealed class FallbackEqualsComparer<T> : IEqualityComparer<T>
{
    public bool Equals(T? x, T? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return x.Equals(y);
    }

    public int GetHashCode(T obj)
    {
        if (obj is null) throw new ArgumentNullException(nameof(obj));
        return obj.GetHashCode();
    }
}
```

### Wann verwenden?

? **Geeignet für:**
- Typen, die `Equals()` und `GetHashCode()` korrekt implementiert haben
- Einfache Value Objects
- Records (haben automatisch korrekte Implementierungen)

? **Nicht geeignet für:**
- Typen ohne überschriebene `Equals()`-Methode (nutzt dann Referenzvergleich)
- Komplexe Vergleichslogik erforderlich
- Performance-kritische Szenarien

### Verwendung

```csharp
using Common.Bootstrap.Defaults;

// Für jeden benötigten Typ registrieren
services.AddSingleton<IEqualityComparer<Customer>>(
    new FallbackEqualsComparer<Customer>());

services.AddSingleton<IEqualityComparer<Order>>(
    new FallbackEqualsComparer<Order>());

services.AddSingleton<IEqualityComparer<Product>>(
    new FallbackEqualsComparer<Product>());
```

## Best Practices

### ? Do's

#### 1. Explizite Registrierung für alle benötigten Typen

```csharp
public class DataModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // Storage Options
        services.AddSingleton<IStorageOptions<Customer>>(/* ... */);
        services.AddSingleton<IStorageOptions<Order>>(/* ... */);
        
        // Comparer für LiteDB-Repositories
        services.AddSingleton<IEqualityComparer<Customer>>(
            new FallbackEqualsComparer<Customer>());
        services.AddSingleton<IEqualityComparer<Order>>(
            new FallbackEqualsComparer<Order>());
        
        // Repositories
        services.AddLiteDbRepository<Customer>();
        services.AddLiteDbRepository<Order>();
    }
}
```

#### 2. Assembly-Scanning für Projekte mit vielen Comparern

```csharp
// In einem Projekt mit 20+ Entitäten
public class DomainModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // Alle Comparer automatisch finden
        services.AddEqualityComparersFromAssembly<DomainModule>();
    }
}

// Comparer in separatem Ordner organisieren
namespace MyApp.Domain.Comparers
{
    public class CustomerComparer : IEqualityComparer<Customer> { }
    public class OrderComparer : IEqualityComparer<Order> { }
    public class ProductComparer : IEqualityComparer<Product> { }
    // ... 20+ weitere
}
```

#### 3. Korrekte Equals/GetHashCode-Implementierung

```csharp
// ? Gut: Überschriebene Equals/GetHashCode
public class Customer : EntityBase
{
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";

    public override bool Equals(object? obj)
    {
        if (obj is not Customer other) return false;
        return Id == other.Id && Email == other.Email;
    }

    public override int GetHashCode()
        => HashCode.Combine(Id, Email);
}

// Jetzt kann FallbackEqualsComparer genutzt werden
services.AddSingleton<IEqualityComparer<Customer>>(
    new FallbackEqualsComparer<Customer>());
```

### ? Don'ts

#### 1. Nicht auf automatischen Fallback verlassen

```csharp
// ? Fehler: Kein Comparer registriert
public class BadModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IStorageOptions<Customer>>(/* ... */);
        
        // ? Vergessen, Comparer zu registrieren
        services.AddLiteDbRepository<Customer>();
        
        // ? InvalidOperationException beim Auflösen!
    }
}
```

#### 2. Nicht System-Assemblies scannen

```csharp
// ? Schlecht: System-Assembly scannen
services.AddEqualityComparersFromAssembly<object>();  // System.Private.CoreLib
services.AddEqualityComparersFromAssembly<string>();  // System.Runtime

// ? Gut: Nur eigene Assemblies scannen
services.AddEqualityComparersFromAssembly<MyModule>();
```

#### 3. Nicht FallbackEqualsComparer ohne korrekte Equals-Implementierung

```csharp
// ? Problematisch: Keine Equals-Überschreibung
public class BadEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    
    // Keine Equals/GetHashCode-Überschreibung
    // ? FallbackEqualsComparer nutzt Referenzvergleich!
}

// ? Wird nicht wie erwartet funktionieren
services.AddSingleton<IEqualityComparer<BadEntity>>(
    new FallbackEqualsComparer<BadEntity>());
```

## Häufige Probleme

### Problem 1: "No service for type IEqualityComparer<T>"

**Symptom:**
```
InvalidOperationException: Unable to resolve service for type 
'System.Collections.Generic.IEqualityComparer`1[MyApp.Customer]'
```

**Ursache:** Kein Comparer für `Customer` registriert.

**Lösung:**
```csharp
services.AddSingleton<IEqualityComparer<Customer>>(
    new FallbackEqualsComparer<Customer>());
```

### Problem 2: Comparer wird nicht gefunden beim Scanning

**Symptom:** Assembly-Scanning findet Ihren Comparer nicht.

**Mögliche Ursachen:**

```csharp
// ? Kein öffentlicher parameterloser Konstruktor
public class MyComparer : IEqualityComparer<Customer>
{
    private readonly ILogger _logger;
    public MyComparer(ILogger logger) => _logger = logger;
}

// ? Lösung: Parameterloser Konstruktor hinzufügen
public class MyComparer : IEqualityComparer<Customer>
{
    public MyComparer() { }  // ? Hinzufügen
}

// Oder manuell registrieren mit Factory
services.AddSingleton<IEqualityComparer<Customer>>(sp =>
    new MyComparer(sp.GetRequiredService<ILogger>()));
```

### Problem 3: Delta-Detection funktioniert nicht korrekt

**Symptom:** LiteDB-Repository erkennt Änderungen nicht.

**Ursache:** `Equals()` ist nicht korrekt implementiert.

**Lösung:**
```csharp
public class Customer : EntityBase
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";

    // ? Equals vergleicht alle relevanten Felder
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

## Integration mit DataToolKit

Wenn Sie [DataToolKit](../../DataToolKit/README.md) verwenden, benötigen LiteDB-Repositories immer einen Comparer:

```csharp
using Common.Bootstrap;
using Common.Bootstrap.Defaults;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.Repositories;

public class MyDataModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // 1. Comparer registrieren (MUSS VOR Repository kommen)
        services.AddSingleton<IEqualityComparer<Customer>>(
            new FallbackEqualsComparer<Customer>());
        
        // 2. Storage Options
        services.AddSingleton<IStorageOptions<Customer>>(
            new LiteDbStorageOptions<Customer>("MyApp", "customers", "Data"));
        
        // 3. Repository registrieren (benötigt Comparer!)
        services.AddLiteDbRepository<Customer>();
    }
}
```

Mehr Details: [DataToolKit Storage Options ?](../../DataToolKit/Docs/Storage-Options.md)

## Weiterführende Themen

- [IServiceModule ?](ServiceModules.md)
- [Modulare Registrierung ?](Modulare-Registrierung.md)
- [API-Referenz ?](API-Referenz.md)
- [Zurück zur Übersicht ?](../README.md)
