# Common.Bootstrap

Modulares Dependency-Injection-Framework für .NET 8 mit automatischer Service-Registrierung und EqualityComparer-Management.

## ?? Inhaltsverzeichnis

- [Überblick](#überblick)
- [Installation](#installation)
- [Schnellstart](#schnellstart)
- [Kernkonzepte](Docs/ServiceModules.md)
  - [IServiceModule](Docs/ServiceModules.md)
  - [Modulare Registrierung](Docs/Modulare-Registrierung.md)
  - [EqualityComparer-Management](Docs/EqualityComparer.md)
- [?? API-Referenz](Docs/API-Referenz.md) – **Vollständige alphabetisch sortierte API-Dokumentation**

## Überblick

**Common.Bootstrap** bietet eine strukturierte Lösung für die Dependency-Injection-Konfiguration in .NET-Anwendungen durch:

- ?? **Modulares Design**: Organisieren Sie DI-Registrierungen in wiederverwendbaren `IServiceModule`-Implementierungen
- ?? **Assembly-Scanning**: Automatische Erkennung und Registrierung von Services
- ?? **EqualityComparer-Management**: Vereinfachte Registrierung von `IEqualityComparer<T>`-Implementierungen
- ?? **Idempotenz**: Sichere Mehrfach-Registrierungen ohne Konflikte

### Wann Common.Bootstrap verwenden?

? **Ideal für:**
- Projekte mit mehreren Bibliotheken, die jeweils ihre eigenen Services registrieren müssen
- Anwendungen, die eine klare Trennung der DI-Konfiguration benötigen
- Teams, die wiederverwendbare Service-Module über Projekte hinweg nutzen möchten

? **Nicht geeignet für:**
- Sehr kleine Projekte mit wenigen Services (Overhead nicht gerechtfertigt)
- Szenarien, wo direkte `IServiceCollection`-Registrierung ausreicht

## Installation

### NuGet Package
```bash
dotnet add package Common.Bootstrap
```

### Lokale Entwicklung
```bash
# Klonen Sie das Repository
git clone https://github.com/ReneRose1971/Libraries.git
cd Libraries/Common.BootStrap

# Bauen und verwenden
dotnet build
```

## Schnellstart

### 1. Erstellen Sie ein Service-Modul

```csharp
using Common.Bootstrap;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp.Infrastructure;

/// <summary>
/// Service-Modul für die Infrastruktur-Services der App.
/// </summary>
public sealed class InfrastructureModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // Registrieren Sie Ihre Services
        services.AddSingleton<IMyService, MyService>();
        services.AddScoped<IMyRepository, MyRepository>();
    }
}
```

### 2. Registrieren Sie alle Module automatisch

```csharp
using Common.Bootstrap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Automatisches Scannen und Registrieren aller IServiceModule-Implementierungen
builder.Services.AddModulesFromAssemblies(
    typeof(InfrastructureModule).Assembly  // Ihre App-Assembly
);

var app = builder.Build();
await app.RunAsync();
```

### 3. Nutzen Sie Ihre Services

```csharp
public class MyApplication
{
    private readonly IMyService _service;

    public MyApplication(IMyService service)
    {
        _service = service;  // Automatisch injiziert
    }

    public void Run()
    {
        _service.DoSomething();
    }
}
```

## Hauptfeatures im Detail

### Modulare Service-Registrierung

Organisieren Sie komplexe DI-Konfigurationen in unabhängige, testbare Module:

```csharp
// Datenbank-Modul
public class DatabaseModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}

// Messaging-Modul
public class MessagingModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IMessageBus, RabbitMQBus>();
        services.AddScoped<IEventPublisher, EventPublisher>();
    }
}
```

[Mehr über Service-Module ?](Docs/ServiceModules.md)

### Automatisches EqualityComparer-Scanning

Registrieren Sie alle `IEqualityComparer<T>`-Implementierungen aus einer Assembly automatisch:

```csharp
using Common.Extensions;

public class MyModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // Scannt die Assembly nach allen EqualityComparer-Implementierungen
        services.AddEqualityComparersFromAssembly<MyModule>();
    }
}

// Ihre Comparer werden automatisch gefunden und registriert
public class CustomerComparer : IEqualityComparer<Customer>
{
    public bool Equals(Customer? x, Customer? y) => x?.Id == y?.Id;
    public int GetHashCode(Customer obj) => obj.Id.GetHashCode();
}
```

[Mehr über EqualityComparer-Management ?](Docs/EqualityComparer.md)

## Best Practices

### ? Do's

- **Ein Modul pro Verantwortungsbereich**: Trennen Sie z.B. Datenbank, Messaging, Validation
- **Explizite Comparer-Registrierung**: Registrieren Sie `IEqualityComparer<T>` für jeden Typ, der einen benötigt
- **Idempotente Registrierungen**: Nutzen Sie `TryAdd*`-Methoden für sichere Mehrfach-Aufrufe
- **Assembly-spezifisches Scanning**: Scannen Sie nur Ihre eigenen Assemblies, nicht System-Assemblies

### ? Don'ts

- **Kein automatischer Fallback**: Es gibt keinen automatischen `IEqualityComparer<T>` für alle Typen
- **Keine zirkulären Abhängigkeiten**: Module sollten unabhängig voneinander sein
- **Nicht alles in ein Modul**: Halten Sie Module fokussiert und klein

## Nächste Schritte

- ?? [Verstehen Sie IServiceModule](Docs/ServiceModules.md)
- ?? [Lernen Sie modulare Registrierung](Docs/Modulare-Registrierung.md)
- ?? [Durchsuchen Sie die vollständige API-Referenz](Docs/API-Referenz.md) – **Alphabetisch sortiert mit Querverweisen**

## Verwandte Projekte

- **[DataToolKit](../DataToolKit/README.md)**: Repository-Implementierungen mit Common.Bootstrap-Integration

## Lizenz & Repository

- **Repository**: [https://github.com/ReneRose1971/Libraries](https://github.com/ReneRose1971/Libraries)
- **Lizenz**: Siehe LICENSE-Datei im Repository

## Support & Beiträge

Bei Fragen oder Problemen erstellen Sie bitte ein Issue im GitHub-Repository.
