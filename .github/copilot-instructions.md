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

#### Dependency Injection
- Verwende `IServiceModule` für Service-Registrierungen
- Registriere EqualityComparer explizit für DataToolKit-Repositories
- Nutze Constructor Injection

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
- ? Keine Breaking Changes an bestehenden APIs
- ? Keine `#pragma warning disable` ohne guten Grund
- ? Keine leeren catch-Blöcke ohne Kommentar

### Bevorzugte Muster

#### Repository-Pattern (DataToolKit)
```csharp
// ? Storage Options registrieren
services.AddSingleton<IStorageOptions<Customer>>(
    new JsonStorageOptions<Customer>("MyApp", "customers", "Data"));

// ? EqualityComparer registrieren
services.AddSingleton<IEqualityComparer<Customer>>(
    new CustomerComparer());

// ? Repository registrieren
services.AddJsonRepository<Customer>();
```

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
