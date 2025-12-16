# Modulare Registrierung mit AddModulesFromAssemblies

Die Extension-Method `AddModulesFromAssemblies` ist das Herzstück der automatischen Service-Registrierung in Common.Bootstrap.

## ?? Inhaltsverzeichnis

- [Überblick](#überblick)
- [Grundlegende Verwendung](#grundlegende-verwendung)
- [Wie es funktioniert](#wie-es-funktioniert)
- [Erweiterte Szenarien](#erweiterte-szenarien)
- [Fehlerbehandlung](#fehlerbehandlung)
- [Performance](#performance)

## Überblick

`AddModulesFromAssemblies` scannt eine oder mehrere Assemblies nach allen Klassen, die `IServiceModule` implementieren, und führt deren `Register()`-Methode aus.

### Signatur

```csharp
public static IServiceCollection AddModulesFromAssemblies(
    this IServiceCollection services,
    params Assembly[] assemblies)
```

**Parameter:**
- `services`: Die `IServiceCollection`, die erweitert wird
- `assemblies`: Liste der zu scannenden Assemblies (optional)

**Rückgabe:**
- Die gleiche `IServiceCollection` für Fluent-API

## Grundlegende Verwendung

### Beispiel 1: Einzelne Assembly scannen

```csharp
using Common.Bootstrap;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Scannt die Assembly, in der Program liegt
builder.Services.AddModulesFromAssemblies(
    typeof(Program).Assembly
);

var app = builder.Build();
await app.RunAsync();
```

### Beispiel 2: Mehrere Assemblies scannen

```csharp
using Common.Bootstrap;
using MyApp.Infrastructure;
using MyApp.Domain;

var builder = Host.CreateApplicationBuilder(args);

// Scannt mehrere Assemblies
builder.Services.AddModulesFromAssemblies(
    typeof(Program).Assembly,              // App-Assembly
    typeof(InfrastructureModule).Assembly, // Infrastructure-Assembly
    typeof(DomainModule).Assembly          // Domain-Assembly
);

var app = builder.Build();
await app.RunAsync();
```

### Beispiel 3: Alle geladenen Assemblies scannen

```csharp
using Common.Bootstrap;

var builder = Host.CreateApplicationBuilder(args);

// Scannt ALLE geladenen Assemblies (wenn keine angegeben)
builder.Services.AddModulesFromAssemblies();

var app = builder.Build();
await app.RunAsync();
```

> ?? **Vorsicht**: Das Scannen aller Assemblies kann langsam sein und unerwartete Module aus NuGet-Packages laden!

## Wie es funktioniert

### Der Ablauf im Detail

1. **Assembly-Auswahl**
   ```csharp
   if (assemblies == null || assemblies.Length == 0)
       assemblies = AppDomain.CurrentDomain.GetAssemblies();
   ```
   Wenn keine Assemblies angegeben sind, werden alle geladenen Assemblies verwendet.

2. **Typ-Suche**
   ```csharp
   var modules = assemblies
       .SelectMany(a => SafeGetTypes(a))  // Alle Typen (mit Fehlerbehandlung)
       .Where(t => moduleType.IsAssignableFrom(t))  // Implementiert IServiceModule?
       .Where(t => !t.IsAbstract && !t.IsInterface)  // Konkrete Klasse?
       .Select(t => (IServiceModule)Activator.CreateInstance(t)!)
       .ToList();
   ```

3. **Registrierung**
   ```csharp
   foreach (var module in modules)
       module.Register(services);
   ```

### Was wird gefunden?

? **Wird registriert:**
- Konkrete Klassen, die `IServiceModule` implementieren
- Öffentliche und interne Klassen
- Nested classes (auch private, wenn zugänglich)

? **Wird NICHT registriert:**
- Abstrakte Klassen
- Interfaces
- Klassen ohne parameterlosen Konstruktor
- Generische Klassen mit ungebundenen Typparametern

### Beispiel: Was wird gefunden?

```csharp
// ? Wird gefunden und registriert
public class MyModule : IServiceModule
{
    public void Register(IServiceCollection services) { }
}

// ? Wird gefunden (internal ist ok)
internal class InternalModule : IServiceModule
{
    public void Register(IServiceCollection services) { }
}

// ? Wird gefunden (nested class)
public class OuterClass
{
    public class NestedModule : IServiceModule
    {
        public void Register(IServiceCollection services) { }
    }
}

// ? Wird NICHT gefunden (abstract)
public abstract class AbstractModule : IServiceModule
{
    public abstract void Register(IServiceCollection services);
}

// ? Wird NICHT gefunden (kein parameterloser Konstruktor)
public class ConfigurableModule : IServiceModule
{
    private readonly string _config;
    
    public ConfigurableModule(string config)  // ? Kein parameterloser Konstruktor
    {
        _config = config;
    }
    
    public void Register(IServiceCollection services) { }
}

// ? Wird NICHT gefunden (offener generischer Typ)
public class GenericModule<T> : IServiceModule
{
    public void Register(IServiceCollection services) { }
}
```

## Erweiterte Szenarien

### Szenario 1: Selektives Laden nach Namespace

```csharp
using System.Reflection;
using Common.Bootstrap;

var builder = Host.CreateApplicationBuilder(args);

// Nur Module aus bestimmten Namespaces laden
var assembly = typeof(Program).Assembly;
var allTypes = assembly.GetTypes();

var modules = allTypes
    .Where(t => t.Namespace?.StartsWith("MyApp.Features") == true)
    .Where(t => typeof(IServiceModule).IsAssignableFrom(t))
    .Where(t => !t.IsAbstract && !t.IsInterface)
    .Select(t => (IServiceModule)Activator.CreateInstance(t)!);

foreach (var module in modules)
{
    module.Register(builder.Services);
}
```

### Szenario 2: Bedingtes Laden

```csharp
using Common.Bootstrap;

var builder = Host.CreateApplicationBuilder(args);

if (builder.Environment.IsDevelopment())
{
    // Nur Development-Module laden
    builder.Services.AddModulesFromAssemblies(
        typeof(DevelopmentModule).Assembly
    );
}
else
{
    // Nur Production-Module laden
    builder.Services.AddModulesFromAssemblies(
        typeof(ProductionModule).Assembly
    );
}
```

### Szenario 3: Plugin-Architektur

```csharp
using System.IO;
using System.Reflection;
using Common.Bootstrap;

var builder = Host.CreateApplicationBuilder(args);

// Lade Plugins aus einem Verzeichnis
var pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
var pluginAssemblies = new List<Assembly>();

if (Directory.Exists(pluginPath))
{
    foreach (var dll in Directory.GetFiles(pluginPath, "*.dll"))
    {
        try
        {
            var assembly = Assembly.LoadFrom(dll);
            pluginAssemblies.Add(assembly);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden von {dll}: {ex.Message}");
        }
    }
}

// Scanne Plugin-Assemblies nach Modulen
builder.Services.AddModulesFromAssemblies(pluginAssemblies.ToArray());
```

### Szenario 4: Manuelles Modul mit Konfiguration

```csharp
using Common.Bootstrap;

var builder = Host.CreateApplicationBuilder(args);

// Automatisches Scannen
builder.Services.AddModulesFromAssemblies(typeof(Program).Assembly);

// Zusätzliches manuelles Modul mit Konfiguration
var config = builder.Configuration.GetValue<string>("DatabaseProvider");
if (config == "SqlServer")
{
    new SqlServerModule().Register(builder.Services);
}
else if (config == "PostgreSQL")
{
    new PostgreSQLModule().Register(builder.Services);
}
```

## Fehlerbehandlung

### ReflectionTypeLoadException

Die Extension-Method behandelt `ReflectionTypeLoadException` automatisch:

```csharp
private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
{
    try
    {
        return assembly.GetTypes();
    }
    catch (ReflectionTypeLoadException ex)
    {
        // Gibt nur die erfolgreich geladenen Typen zurück
        return ex.Types.Where(t => t != null)!;
    }
}
```

**Was bedeutet das?**
- Wenn eine Assembly Typen enthält, die nicht geladen werden können (z.B. fehlende Abhängigkeiten), werden diese übersprungen
- Die erfolgreich geladenen Typen werden trotzdem verarbeitet
- Kein Crash der Anwendung

### Fehler bei der Modul-Instanziierung

```csharp
// ? Modul ohne parameterlosen Konstruktor
public class BrokenModule : IServiceModule
{
    public BrokenModule(string config)  // ? Fehler!
    {
    }
    
    public void Register(IServiceCollection services) { }
}
```

**Resultat:** `MissingMethodException` beim Aufruf von `Activator.CreateInstance(t)`

**Lösung:**
```csharp
// ? Parameterloser Konstruktor hinzufügen
public class FixedModule : IServiceModule
{
    private readonly string _config;
    
    public FixedModule() : this("default") { }  // ? Hinzufügen
    
    public FixedModule(string config)
    {
        _config = config;
    }
    
    public void Register(IServiceCollection services) { }
}
```

### Fehler in Register()-Methode

```csharp
public class ProblematicModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // ? Fehler beim Registrieren
        throw new InvalidOperationException("Configuration error");
    }
}
```

**Resultat:** Exception wird nach oben propagiert und stoppt die Anwendung.

**Best Practice:** Verwenden Sie Try-Catch für robuste Fehlerbehandlung:

```csharp
var builder = Host.CreateApplicationBuilder(args);

try
{
    builder.Services.AddModulesFromAssemblies(typeof(Program).Assembly);
}
catch (Exception ex)
{
    // Logging
    Console.WriteLine($"Fehler beim Laden der Module: {ex.Message}");
    throw;
}
```

## Performance

### Benchmarks (ungefähr)

| Szenario | Assemblies | Module | Dauer |
|----------|-----------|--------|-------|
| Kleine App | 1 | 3 | < 1ms |
| Mittlere App | 5 | 15 | 5-10ms |
| Große App | 20 | 50 | 50-100ms |
| Alle geladenen | 100+ | ? | 500ms+ |

### Performance-Tipps

#### ? Do's

1. **Spezifische Assemblies angeben**
   ```csharp
   // ? Gut: Nur benötigte Assemblies
   services.AddModulesFromAssemblies(
       typeof(Program).Assembly,
       typeof(MyLibrary.Module).Assembly
   );
   ```

2. **Assembly-References nutzen**
   ```csharp
   // ? Gut: Type.Assembly ist schnell
   var assembly = typeof(MyModule).Assembly;
   services.AddModulesFromAssemblies(assembly);
   ```

#### ? Don'ts

1. **Nicht alle Assemblies scannen**
   ```csharp
   // ? Langsam: Scannt ALLES
   services.AddModulesFromAssemblies();
   ```

2. **Nicht in Hot-Paths aufrufen**
   ```csharp
   // ? Sehr schlecht: In Request-Handler
   app.MapGet("/", (IServiceCollection services) =>
   {
       services.AddModulesFromAssemblies(typeof(Program).Assembly);
       // ...
   });
   ```

### Caching

Wenn Sie häufig die gleichen Assemblies scannen müssen:

```csharp
// Einmalig beim Start
private static readonly Assembly[] CachedAssemblies = new[]
{
    typeof(Program).Assembly,
    typeof(InfrastructureModule).Assembly,
    typeof(DomainModule).Assembly
};

// Wiederverwendung
builder.Services.AddModulesFromAssemblies(CachedAssemblies);
```

## Debugging

### Module-Registrierung loggen

```csharp
using System.Reflection;
using Common.Bootstrap;

var builder = Host.CreateApplicationBuilder(args);

// Manuelles Scannen mit Logging
var assembly = typeof(Program).Assembly;
var moduleType = typeof(IServiceModule);

var modules = assembly.GetTypes()
    .Where(t => moduleType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
    .ToList();

Console.WriteLine($"Gefundene Module in {assembly.FullName}:");
foreach (var module in modules)
{
    Console.WriteLine($"  - {module.FullName}");
    var instance = (IServiceModule)Activator.CreateInstance(module)!;
    instance.Register(builder.Services);
}
```

### Services inspizieren

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddModulesFromAssemblies(typeof(Program).Assembly);

// Alle registrierten Services ausgeben
foreach (var service in builder.Services)
{
    Console.WriteLine($"{service.ServiceType.Name} -> {service.ImplementationType?.Name ?? "Factory"}");
}
```

## Weiterführende Themen

- [IServiceModule ?](ServiceModules.md)
- [EqualityComparer-Management ?](EqualityComparer.md)
- [Zurück zur Übersicht ?](../README.md)
