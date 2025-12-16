# SolutionBundler.WPF

WPF-Benutzeroberfläche für SolutionBundler mit MVVM-Pattern und modernen UI-Controls.

## Übersicht

`SolutionBundler.WPF` ist die grafische Benutzeroberfläche für den SolutionBundler, gebaut mit WPF und MVVM-Pattern. Die Anwendung nutzt die Libraries **CustomWPFControls** und **DataToolKit** für eine moderne, wartbare Architektur.

---

## Features

- **Split-View Layout** - Projekt-Verwaltung (33%) + Log-Ausgabe (66%)
- **Project Management** - Hinzufügen, Bearbeiten, Löschen von Projekten
- **Live-Log-Ausgabe** - Echtzeit-Anzeige des Scan-Fortschritts
- **Dependency Injection** - Microsoft.Extensions.Hosting
- **MVVM-Pattern** - Vollständige Trennung von UI und Logik
- **Custom Controls** - ProjectListEditorView, LogOutputView
- **Persistierung** - Automatisches Speichern der Projekt-Liste mit DataToolKit

---

## Verwendung

### Application starten

```bash
dotnet run --project SolutionBundler.WPF
```

### UI-Komponenten

#### 1. Project List Editor (Links, 33%)
- ? Projekte hinzufügen (.csproj-Dateien)
- ? Status-Anzeige (? = Datei existiert, ? = nicht gefunden)
- ?? Details anzeigen
- ? Projekte entfernen
- ??? Alle löschen

#### 2. Log Output (Rechts, 66%)
- ?? Echtzeit-Log-Ausgabe (Consolas-Font)
- ?? ProgressBar (0-100%)
- ?? Status-Text
- ?? Auto-Scroll zum Ende

---

## Architektur

### MVVM-Pattern

```
Models       ? SolutionBundler.Core/Models/
ViewModels   ? SolutionBundler.WPF/ViewModels/
Views        ? SolutionBundler.WPF/*.xaml
```

### Dependency Injection

**App.xaml.cs:**

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    var builder = Host.CreateApplicationBuilder();

    // 1. SolutionBundler.Core-Module registrieren
    builder.Services.AddModulesFromAssemblies(
        typeof(SolutionBundlerCoreModule).Assembly);

    // 2. SolutionBundler.WPF-Module registrieren
    builder.Services.AddModulesFromAssemblies(
        typeof(SolutionBundlerWpfModule).Assembly);

    _host = builder.Build();

    // MainWindowWithSplitView starten
    var mainWindow = _host.Services.GetRequiredService<MainWindowWithSplitView>();
    mainWindow.Show();
}
```

**Vorteile:**
- Automatische Registrierung aller Services via Module
- Saubere Trennung von Concerns
- Einfaches Austauschen von Implementierungen

---

## Custom Controls

### ProjectListEditorView

**Pfad:** `Controls/ProjectListEditorView.cs`

**Features:**
- ListBox mit Custom ItemTemplate
- Command-Buttons (Add, Details, Remove, Clear All)
- Empty-State Placeholder
- Status-Icons (?/?)

**XAML:**

```xaml
<controls:ProjectListEditorView 
    Title="Projekte"
    EmptyMessage="Keine Projekte hinzugefügt."
    ItemsSource="{Binding Projects}"
    SelectedItem="{Binding SelectedProject}"
    AddCommand="{Binding AddCommand}"
    EditCommand="{Binding EditCommand}"
    DeleteCommand="{Binding DeleteCommand}"
    ClearCommand="{Binding ClearCommand}"/>
```

---

### LogOutputView

**Pfad:** `Controls/LogOutputView.cs`

**Features:**
- Mehrzeilige TextBox (read-only)
- ProgressBar (0-100%)
- Status-Text
- Monospace-Font (Consolas)

**XAML:**

```xaml
<controls:LogOutputView 
    Title="Ausgabe"
    LogText="{Binding LogText}"
    StatusText="{Binding Status}"
    Progress="{Binding ProgressValue}"
    IsProgressVisible="True"/>
```

---

## ViewModels

### ProjectInfoViewModel

Wrapper für `ProjectInfo`-Model.

```csharp
public class ProjectInfoViewModel : ViewModelBase<ProjectInfo>
{
    public string Name { get; }         // Projektname
    public string Path { get; }         // .csproj Pfad
    public bool FileExists { get; }     // Datei vorhanden?
    public string StatusIcon { get; }   // ? oder ?
    public string StatusText { get; }   // Tooltip
}
```

**Verwendung:**
```csharp
var projectInfo = new ProjectInfo { Path = @"C:\MyApp\MyApp.csproj" };
var viewModel = new ProjectInfoViewModel(projectInfo);

Console.WriteLine(viewModel.Name);        // "MyApp"
Console.WriteLine(viewModel.StatusIcon);  // "?" oder "?"
Console.WriteLine(viewModel.FileExists);  // true/false
```

---

### ProjectListEditorViewModel

Collection-ViewModel mit CRUD-Operationen.

```csharp
public class ProjectListEditorViewModel 
    : EditableCollectionViewModel<ProjectInfo, ProjectInfoViewModel>
{
    public ReadOnlyObservableCollection<ProjectInfoViewModel> Projects { get; }
    public ProjectInfoViewModel? SelectedProject { get; set; }
    
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ClearCommand { get; }
}
```

**Commands:**
- `AddCommand` - Öffnet FileDialog für .csproj-Auswahl
- `EditCommand` - Zeigt Details des selektierten Projekts
- `DeleteCommand` - Entfernt selektiertes Projekt (nur wenn SelectedProject != null)
- `ClearCommand` - Löscht alle Projekte (nur wenn Projects.Count > 0)

---

## Dokumentation

- [API-Referenz](Docs/API-Referenz.md) - Vollständige API-Dokumentation
- [Workflow](../Docs/Workflow.md) - Anwendungs-Ablauf
- [Developer Guide](../Docs/Developer.md) - Entwickler-Hinweise
- [CHANGELOG](../CHANGELOG.md) - Versions-Historie

---

## Dependencies

### NuGet-Pakete

- **Microsoft.Extensions.Hosting** `10.0.1` - Dependency Injection
- **Fody** `6.9.3` - PropertyChanged Weaving
- **PropertyChanged.Fody** `4.1.0` - INotifyPropertyChanged
- **System.Text.Json** `10.0.1` - JSON-Serialisierung

### Projekt-Referenzen

- **SolutionBundler.Core** - Business Logic
- **CustomWPFControls** (aus Libraries) - MVVM-Framework
- **DataToolKit** (aus Libraries) - Persistierung
- **Common.BootStrap** (aus Libraries) - DI-Module

---

## Themes

### Generic.xaml
Original-Style für `ProjectListEditorView`.

### Generic2.xaml
Erweiterte Styles für `ProjectListEditorView` + `LogOutputView`.

**In App.xaml einbinden:**

```xaml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="/SolutionBundler.WPF;component/Themes/Generic.xaml"/>
            <ResourceDictionary Source="/SolutionBundler.WPF;component/Themes/Generic2.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

---

## Persistierung

Projekt-Liste wird automatisch gespeichert in:
```
%AppData%\SolutionBundler\projects.json
```

**Automatisches Laden:** Beim Start der Anwendung
**Automatisches Speichern:** Bei jeder Änderung (Add/Remove/Clear)

---

## Troubleshooting

### Problem: "Generic2.xaml Theme nicht gefunden"

**Lösung:** Prüfen Sie `App.xaml` - beide ResourceDictionaries müssen eingebunden sein.

### Problem: "ViewModelFactory kann nicht aufgelöst werden"

**Lösung:** Stellen Sie sicher, dass Module korrekt registriert sind:

```csharp
builder.Services.AddModulesFromAssemblies(
    typeof(SolutionBundlerWpfModule).Assembly);
```

### Problem: "Zwei Windows starten"

**Lösung:** Entfernen Sie `StartupUri` aus `App.xaml` - Window wird programmatisch gestartet:

```xaml
<!-- ENTFERNEN: StartupUri="MainWindow.xaml" -->
<Application x:Class="SolutionBundler.WPF.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
```

### Problem: "Projekte werden nicht persistiert"

**Lösung:** Prüfen Sie, ob `DataToolKit` korrekt registriert ist und Schreibrechte für `%AppData%` vorhanden sind.

---

## Build & Publish

```bash
# Debug-Build
dotnet build SolutionBundler.WPF

# Release-Build
dotnet build SolutionBundler.WPF -c Release

# Publish (Self-contained)
dotnet publish SolutionBundler.WPF -c Release -r win-x64 --self-contained -o ./publish

# Publish (Framework-dependent)
dotnet publish SolutionBundler.WPF -c Release -o ./publish
```

---

## Changelog

Siehe [CHANGELOG.md](../CHANGELOG.md) für eine vollständige Liste aller Änderungen.

---

## Siehe auch

- [SolutionBundler.Core](../SolutionBundler.Core/README.md) - Business Logic
- [Solution README](../README.md) - Projekt-Übersicht
- [CustomWPFControls Docs](../../Libraries/CustomWPFControls/README.md) - MVVM-Framework
- [DataToolKit Docs](../../Libraries/DataToolKit/README.md) - Persistierung
