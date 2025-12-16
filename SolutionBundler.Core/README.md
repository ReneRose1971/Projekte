# SolutionBundler.Core

Business Logic und Abstractions für den Scan- und Bundle-Prozess von .NET Solutions.

## Übersicht

`SolutionBundler.Core` enthält die Kernlogik zum Scannen von Solution-Verzeichnissen, Extrahieren von Projekt-Metadaten und Generieren von Markdown-Dokumentationen.

---

## Features

- **File Scanning** - Rekursives Durchsuchen von Solution-Verzeichnissen mit konfigurierbaren Filtern
- **Projekt-Metadaten** - Extraktion von Informationen aus `.csproj`-Dateien (TargetFramework, Dependencies, BuildActions)
- **Content-Klassifizierung** - Automatische Erkennung von Dateitypen (C#, XAML, JSON, XML, etc.)
- **Hash-Berechnung** - SHA1-Hashes für Dateiinhalte zur Versionierung
- **Secret-Maskierung** - Automatisches Erkennen und Maskieren von sensiblen Daten (API-Keys, Passwörter)
- **Bundle-Writer** - Markdown-Generierung mit strukturierter Ausgabe

---

## Architektur

### Refactoring: BundleWriting

Die `MarkdownBundleWriter`-Klasse wurde in kleinere, fokussierte Komponenten aufgeteilt:

```
BundleWriting/
??? MarkdownBundleWriter.cs      - Haupt-Interface-Implementierung
??? MarkdownGenerator.cs         - Content-Generierung (FrontMatter, TOC, Sections)
??? FileContentReader.cs         - Datei-Lesen mit Error-Handling
??? OutputPathResolver.cs        - Pfad-Auflösung und Verzeichnis-Erstellung
??? MarkdownAnchorGenerator.cs   - URL-sichere Anker für Links
```

**Vorteile:**
- Bessere Testbarkeit (jede Komponente einzeln testbar)
- Klarere Verantwortlichkeiten (Single Responsibility Principle)
- Einfachere Wartbarkeit und Erweiterbarkeit

---

## Verwendung

### Basic Scan

```csharp
using SolutionBundler.Core.Abstractions;
using SolutionBundler.Core.Implementations;
using SolutionBundler.Core.Models;

// Scanner erstellen
var scanner = new DefaultFileScanner();

// Settings konfigurieren
var settings = new ScanSettings
{
    MaskSecrets = true,
    IncludeTestProjects = false,
    OutputFileName = "MyProject.md"
};

// Scannen
var files = scanner.Scan(@"C:\Projects\MyProject", settings);
Console.WriteLine($"Gefundene Dateien: {files.Count}");
```

### Bundle-Orchestrierung

```csharp
var orchestrator = new BundleOrchestrator(
    scanner: new DefaultFileScanner(),
    metadataReader: new MsBuildProjectMetadataReader(),
    classifier: new SimpleContentClassifier(),
    hasher: new Sha1HashCalculator(),
    writer: new MarkdownBundleWriter(new RegexSecretMasker())
);

var outputPath = orchestrator.Run(@"C:\Projects\MyProject", settings);
Console.WriteLine($"Bundle erstellt: {outputPath}");
```

---

## Namespaces

- **`Abstractions`** - Interfaces für alle Komponenten
- **`Implementations`** - Konkrete Implementierungen
  - **`BundleWriting`** - Markdown-Generierung (refactored)
  - **`Scanning`** - File-Scanner
  - Weitere Helper-Klassen
- **`Models`** - Domain Models (ScanSettings, FileEntry, ProjectInfo, etc.)
- **`Storage`** - Persistierung (ProjectStore)

---

## Dependency Injection

```csharp
// In App.xaml.cs oder Program.cs
builder.Services.AddSingleton<IFileScanner, DefaultFileScanner>();
builder.Services.AddSingleton<IProjectMetadataReader, MsBuildProjectMetadataReader>();
builder.Services.AddSingleton<IContentClassifier, SimpleContentClassifier>();
builder.Services.AddSingleton<IHashCalculator, Sha1HashCalculator>();
builder.Services.AddSingleton<ISecretMasker, RegexSecretMasker>();
builder.Services.AddSingleton<IBundleWriter, MarkdownBundleWriter>();
builder.Services.AddSingleton<IBundleOrchestrator, BundleOrchestrator>();
```

**Oder mit Module-Registration:**
```csharp
builder.Services.AddModulesFromAssemblies(
    typeof(SolutionBundlerCoreModule).Assembly);
```

---

## Dokumentation

- [API-Referenz](Docs/API-Referenz.md) - Vollständige API-Dokumentation aller Interfaces und Klassen
- [Workflow](../Docs/Workflow.md) - Detaillierter Ablauf des Bundle-Prozesses
- [Developer Guide](../Docs/Developer.md) - Entwickler-Hinweise und Erweiterungspunkte
- [CHANGELOG](../CHANGELOG.md) - Versions-Historie

---

## Dependencies

- **.NET 8.0** - Target Framework
- **System.Text.Json** - JSON-Serialisierung für Settings
- **System.Xml.Linq** - XML-Parsing für `.csproj`-Dateien

---

## Erweiterungspunkte

### Eigene Scanner-Implementierung

```csharp
public class CustomFileScanner : IFileScanner
{
    public List<FileEntry> Scan(string rootPath, ScanSettings settings)
    {
        // Custom Scan-Logik
        // z.B. .gitignore respektieren
    }
}
```

### Eigene Bundle-Writer

```csharp
public class HtmlBundleWriter : IBundleWriter
{
    public string Write(string outputPath, List<FileEntry> files, 
                        List<ProjectMetadata> projects)
    {
        // HTML statt Markdown generieren
    }
}
```

### Eigene Secret-Masker

```csharp
public class CustomSecretMasker : ISecretMasker
{
    public string Process(string relativePath, string content)
    {
        // Custom Secret-Detection
        // z.B. mit Machine Learning
    }
}
```

---

## Ausgabe-Format

Das generierte Markdown-Bundle hat folgende Struktur:

```markdown
---
project_root: ProjectName
generated_at: 2024-12-15 10:30:00
tool: SolutionBundler v1
---

# Inhaltsverzeichnis
* [File1.cs](#file1-cs)
* [File2.xaml](#file2-xaml)

# Dateien

## File1.cs
_size_: 1234 bytes - _sha1_: abc123... - _action_: Compile

--- FILE: File1.cs | HASH: abc123... | ACTION: Compile ---

```csharp
// Code hier
```

## File2.xaml
_size_: 567 bytes - _sha1_: def456... - _action_: Page

--- FILE: File2.xaml | HASH: def456... | ACTION: Page ---

```xml
<Window ...>
```
```

**Ausgabe-Verzeichnis:** `%USERPROFILE%\Documents\SolutionBundler\Bundles\`

---

## Testing

Unit-Tests befinden sich in `SolutionBundler.Tests`:

```bash
# Alle Core-Tests
dotnet test --filter FullyQualifiedName~SolutionBundler.Tests

# Spezifische Test-Kategorie
dotnet test --filter FullyQualifiedName~BundleWriting
dotnet test --filter FullyQualifiedName~MetadataReading
```

**Test-Coverage:** >85% für alle Core-Komponenten

---

## Changelog

Siehe [CHANGELOG.md](../CHANGELOG.md) für eine vollständige Liste aller Änderungen und Releases.

**Wichtige Änderungen:**
- Refactoring von `MarkdownBundleWriter` in kleinere Komponenten
- Verbesserte Error-Handling für Datei-Operationen
- Kontextabhängige Secret-Maskierung

---

## Siehe auch

- [SolutionBundler.WPF](../SolutionBundler.WPF/README.md) - UI Layer
- [Solution README](../README.md) - Projekt-Übersicht
- [Libraries Repository](https://github.com/ReneRose1971/Libraries) - Externe Dependencies
