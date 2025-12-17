# GitHub Copilot Instructions

## Allgemeine Richtlinien

### Projekt-Charakteristiken
- **Ziel-Framework**: .NET 8
- **Sprache**: C# mit aktivierten Nullable Reference Types
- **Code-Stil**: Folge den bestehenden Konventionen im Projekt

### Test-Erstellung
?? **Tests nur auf explizite Nachfrage erstellen**
- Erstelle KEINE Unit-Tests, Integrationstests oder andere Tests, es sei denn, der Benutzer fordert dies explizit an
- Wenn Tests erstellt werden sollen, verwende:
  - xUnit als Test-Framework
  - FluentAssertions für Assertions (wo vorhanden)
  - Moq für Mocking (wo vorhanden)

### Dokumentation
?? **Dokumentationen nur auf explizite Nachfrage bearbeiten oder erstellen**
- Erstelle KEINE README-Dateien oder Markdown-Dokumentationen, es sei denn, der Benutzer fordert dies explizit an
- Bearbeite KEINE bestehende Dokumentation, es sei denn, der Benutzer fordert dies explizit an
- **Auf keinen Fall automatisch eine README oder .md-Datei erstellen, nur weil eine neue Klasse hinzugefügt wurde**
- XML-Dokumentationskommentare im Code sind davon nicht betroffen und sollten normal hinzugefügt werden

### Dokumentations-Struktur
?? **Wo Dokumentationen abgelegt werden dürfen:**
- ? **Solution-README**: Im Root-Verzeichnis der Solution
- ? **Projekt-README**: Im Root-Verzeichnis jedes Projekts
- ? **Docs-Ordner**: Pro Projekt ein `Docs/` Ordner (muss bei Bedarf erstellt werden)
  - Dort gehört auch die **API-Referenz** hin
  - Beispiel: `MyProject/Docs/API-Referenz.md`

? **Wo KEINE Dokumentationen erlaubt sind:**
- ? In Quellcode-Ordnern (z.B. `Services/`, `Models/`, `ViewModels/`, `DI/`)
- ? Neben einzelnen Klassen-Dateien
- ? In verschachtelten Unterordnern des Quellcodes

**Beispiel-Struktur:**
```
? MyProject/
   ??? README.md                    ? Erlaubt: Projekt-Übersicht
   ??? Docs/
   ?   ??? API-Referenz.md         ? Erlaubt: API-Dokumentation
   ?   ??? Getting-Started.md      ? Erlaubt: Tutorials
   ?   ??? Architecture.md         ? Erlaubt: Architektur-Docs
   ??? Services/
   ?   ??? MyService.cs            ? Code mit XML-Kommentaren
   ?   ??? ? README.md            ? VERBOTEN!
   ??? DI/
       ??? MyServiceModule.cs      ? Code mit XML-Kommentaren
       ??? ? README.md            ? VERBOTEN!
```

### Code-Kommentare
- Verwende XML-Dokumentationskommentare (`///`) für öffentliche APIs
- Deutsche Kommentare sind in diesem Projekt üblich - folge dem bestehenden Stil
- Halte Kommentare prägnant und wartbar

### Projekt-Struktur

#### Common.BootStrap
- Modulares DI-Framework mit `IServiceModule`
- Assembly-Scanning für automatische Service-Registrierung
- EqualityComparer-Management

#### DataToolKit
- Repository-Pattern-Implementierungen (JSON, LiteDB)
- DataStore-Abstraktionen (InMemory, Persistent)
- DataStoreProvider für Singleton-Management

#### CustomWPFControls
- WPF-Controls und ViewModels
- CollectionViewModel mit DataStore-Integration
- WindowLayoutService für Window-State-Persistierung

#### SolutionBundler
- Tool zum Erstellen von Markdown-Bundles aus Solution-Dateien
- Core-Bibliothek mit Scanner, Parser, Writer
- WPF-GUI

#### TypeTutor
- Typing-Tutor-Anwendung
- Logic-Bibliothek mit Engine, Lessons, Data-Klassen
- WPF-GUI mit Visual Keyboard

### Coding Conventions

#### Dependency Injection und Service Modules

**?? KRITISCHE REGEL: Keine Verschachtelung von ServiceModules!**

Ein `IServiceModule` darf **NIEMALS** innerhalb eines anderen `IServiceModule` instanziiert und ausgeführt werden!

##### ? FALSCH - Verschachtelte Module

```csharp
// ? NIEMALS SO MACHEN!
public class MyAppServiceModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // ? VERBOTEN: Verschachtelung von Modulen
        new DataToolKitServiceModule().Register(services);
        new OtherModule().Register(services);
        
        // Eigene Registrierungen...
        services.AddSingleton<IMyService, MyService>();
    }
}
```

**Warum ist das verboten?**
- Verstößt gegen das Single Responsibility Principle
- Erschwert die Wartbarkeit erheblich
- Macht Abhängigkeiten zwischen Modulen unklar
- Führt zu potentiellen Doppel-Registrierungen
- Umgeht das zentrale Bootstrap-System
- Macht die Reihenfolge der Registrierungen unkontrollierbar

##### ? KORREKT - AddModulesFromAssemblies

```csharp
// ? In Program.cs, Startup oder Test-Fixtures
var services = new ServiceCollection();

// Automatische Registrierung aller Module aus mehreren Assemblies
services.AddModulesFromAssemblies(
    typeof(DataToolKitServiceModule).Assembly,
    typeof(TypeTutorServiceModule).Assembly,
    typeof(MyAppModule).Assembly);

var provider = services.BuildServiceProvider();
```

**Reihenfolge der Assemblies ist wichtig:**
1. **Infrastruktur-Module zuerst** (z.B. Common.Bootstrap, DataToolKit)
2. **Domain-Module** (z.B. TypeTutor.Logic)
3. **Application-Module** (z.B. WPF-ViewModels)

##### ? KORREKT - ServiceModule-Struktur

```csharp
public sealed class MyAppServiceModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // ? Nur eigene Services registrieren
        RegisterComparers(services);
        RegisterRepositories(services);
        RegisterServices(services);
    }
    
    private static void RegisterComparers(IServiceCollection services)
    {
        services.AddSingleton<IEqualityComparer<MyEntity>>(
            new MyEntityComparer());
    }
    
    private static void RegisterRepositories(IServiceCollection services)
    {
        // ? Extension-Methoden aus anderen Bibliotheken verwenden
        services.AddJsonRepository<MyEntity>("MyApp", "entities", "Data");
    }
    
    private static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IMyService, MyService>();
        services.AddTransient<IMyRepository, MyRepository>();
    }
}
```

##### ? KORREKT - Tests mit ServiceModules

```csharp
public class ServiceProviderFixture : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public ServiceProviderFixture()
    {
        var services = new ServiceCollection();
        
        // ? KORREKT: AddModulesFromAssemblies verwenden
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,
            typeof(MyAppServiceModule).Assembly);
        
        _serviceProvider = services.BuildServiceProvider();
    }
    
    public T GetRequiredService<T>() where T : notnull
        => _serviceProvider.GetRequiredService<T>();
    
    public void Dispose()
    {
        _serviceProvider.Dispose();
    }
}

// Verwendung in Tests
[Fact]
public void Test_With_Correct_DI_Setup()
{
    // ? Fixture verwendet AddModulesFromAssemblies
    var fixture = new ServiceProviderFixture();
    var service = fixture.GetRequiredService<IMyService>();
    
    service.Should().NotBeNull();
}
```

##### Dokumentation von Abhängigkeiten

Wenn ein ServiceModule von anderen Modulen abhängt, dokumentiere dies im XML-Kommentar:

```csharp
/// <summary>
/// Service-Modul für MyApp.
/// </summary>
/// <remarks>
/// <para>
/// <b>Abhängigkeiten:</b> Dieses Modul setzt voraus, dass 
/// <see cref="DataToolKitServiceModule"/> bereits registriert wurde.
/// </para>
/// <para>
/// Bei Verwendung von <c>AddModulesFromAssemblies</c> mit beiden Assemblies 
/// wird dies automatisch sichergestellt.
/// </para>
/// </remarks>
public sealed class MyAppServiceModule : IServiceModule
{
    // ...
}
```

##### Häufige Fehler vermeiden

```csharp
// ? Manuelles Instanziieren in Tests
[Fact]
public void Wrong_Test_Setup()
{
    var services = new ServiceCollection();
    new MyAppServiceModule().Register(services);  // ? Abhängigkeiten fehlen!
    var provider = services.BuildServiceProvider();
}

// ? Korrekt: AddModulesFromAssemblies
[Fact]
public void Correct_Test_Setup()
{
    var services = new ServiceCollection();
    services.AddModulesFromAssemblies(
        typeof(DataToolKitServiceModule).Assembly,
        typeof(MyAppServiceModule).Assembly);
    var provider = services.BuildServiceProvider();
}
```

#### Repository-Pattern (DataToolKit)

##### JSON-Repositories
```csharp
// ? In einem ServiceModule
services.AddJsonRepository<MyEntity>(
    appSubFolder: "MyApp",
    fileNameBase: "entities",
    subFolder: "Data");

// Resultat: IRepositoryBase<MyEntity> ist verfügbar
// Datei: %USERPROFILE%\Documents\MyApp\Data\entities.json
```

##### LiteDB-Repositories
```csharp
// ? In einem ServiceModule
services.AddLiteDbRepository<MyEntity>(
    appSubFolder: "MyApp",
    fileNameBase: "entities",
    subFolder: "Databases");

// Resultat: IRepositoryBase<MyEntity> UND IRepository<MyEntity> sind verfügbar
// Datei: %USERPROFILE%\Documents\MyApp\Databases\entities.db
```

##### Unterschied zwischen IRepositoryBase und IRepository

- **IRepositoryBase<T>**: Basis-Interface für alle Repositories (Load, Write, Clear)
- **IRepository<T>**: Erweitert IRepositoryBase mit Update/Delete (nur LiteDB!)

```csharp
// ? JSON-Repository: Nur IRepositoryBase verfügbar
public class MyService
{
    public MyService(IRepositoryBase<MyPoco> repository) { }
}

// ? LiteDB-Repository: Beide Interfaces verfügbar
public class MyEntityService
{
    public MyEntityService(IRepository<MyEntity> repository) 
    {
        // repository.Update() und repository.Delete() verfügbar
    }
}
```

#### DataStore-Pattern

##### DataStoreProvider verwenden
```csharp
public class MyService
{
    private readonly PersistentDataStore<MyEntity> _store;
    
    public MyService(IDataStoreProvider provider, IRepositoryFactory factory)
    {
        _store = provider.GetPersistent<MyEntity>(
            factory,
            isSingleton: true,
            trackPropertyChanges: true,
            autoLoad: true);
    }
    
    public ReadOnlyObservableCollection<MyEntity> Items => _store.Items;
}
```

##### DataStoreWrapper Pattern
```csharp
public sealed class DataStoreWrapper
{
    private readonly PersistentDataStore<EntityA> _storeA;
    private readonly PersistentDataStore<EntityB> _storeB;

    public DataStoreWrapper(IDataStoreProvider provider, IRepositoryFactory factory)
    {
        _storeA = provider.GetPersistent<EntityA>(factory, isSingleton: true, autoLoad: true);
        _storeB = provider.GetPersistent<EntityB>(factory, isSingleton: true, autoLoad: true);
    }

    public ReadOnlyObservableCollection<EntityA> EntitiesA => _storeA.Items;
    public ReadOnlyObservableCollection<EntityB> EntitiesB => _storeB.Items;
}
```

#### Weitere DI-Conventions
- Verwende Constructor Injection
- Registriere EqualityComparer explizit für DataToolKit-Repositories
- Bevorzuge Singletons für Repositories und DataStores
- Verwende `TryAddSingleton` für optionale Registrierungen

#### Namespaces
```csharp
// ? Bevorzugt: File-scoped namespaces (C# 10+)
namespace MyProject.Feature;

public class MyClass { }
```

#### Records vs Classes
```csharp
// ? Records für DTOs und Value Objects
public sealed record LessonData(string Title, string Content);

// ? Classes für Entities mit Verhalten
public class LessonFactory : ILessonFactory { }
```

#### Nullable Reference Types
```csharp
// ? Immer aktiviert - nutze ? für nullable
public string? OptionalValue { get; set; }
public string RequiredValue { get; set; } = string.Empty;
```

#### PropertyChanged.Fody
- ViewModels nutzen `[DoNotNotify]` für berechnete Properties
- Keine manuelle PropertyChanged-Implementierung in Fody-Projekten

### NuGet-Pakete (Versionen)
- Microsoft.Extensions.DependencyInjection: 10.0.1
- xUnit: 2.9.3
- FluentAssertions: 8.8.0
- Moq: 4.20.72
- PropertyChanged.Fody: 4.1.0

### Verbotene Praktiken
- ? Keine Tests ohne explizite Aufforderung erstellen
- ? Keine Dokumentation ohne explizite Aufforderung erstellen/bearbeiten
- ? **Niemals automatisch README/Markdown-Dateien in Quellcode-Ordnern erstellen**
- ? **Niemals ServiceModules verschachteln (siehe kritische Regel oben)**
- ? Keine Breaking Changes an bestehenden APIs
- ? Keine `#pragma warning disable` ohne guten Grund
- ? Keine leeren catch-Blöcke ohne Kommentar

### Bevorzugte Muster

#### ViewModel-Pattern (CustomWPFControls)
```csharp
// ? CollectionViewModel mit DataStore
public class MyListViewModel : CollectionViewModel<MyModel, MyViewModel>
{
    public MyListViewModel(
        IDataStore<MyModel> dataStore,
        IViewModelFactory<MyModel, MyViewModel> factory,
        IEqualityComparer<MyModel> comparer)
        : base(dataStore, factory, comparer)
    {
    }
}
```

## Bei Unklarheiten
- Schaue dir bestehenden Code im Projekt an
- Bevorzuge Konsistenz mit bestehendem Code über "Best Practices"
- Frage nach, wenn unklar ist, ob Tests/Dokumentation gewünscht sind
- **Im Zweifelsfall: KEINE Dokumentations-Dateien erstellen**
- **Im Zweifelsfall bei DI: AddModulesFromAssemblies verwenden**
