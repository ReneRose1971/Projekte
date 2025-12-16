# API-Referenz - SolutionBundler.Core

Vollständige API-Dokumentation aller öffentlichen Interfaces und Klassen.

---

## Namespaces

- [Abstractions](#abstractions) - Interfaces
- [Implementations](#implementations) - Konkrete Implementierungen
- [BundleWriting (Internal)](#bundlewriting-internal-helper-classes) - Interne Helper-Klassen
- [Models](#models) - Domain Models
- [Storage](#storage) - Persistierung

---

## Abstractions

### IFileScanner

Scannt Dateien in einem Verzeichnis rekursiv.

```csharp
namespace SolutionBundler.Core.Abstractions;

public interface IFileScanner
{
    List<FileEntry> Scan(string rootPath, ScanSettings settings);
}
```

**Parameter:**
- `rootPath` - Absoluter Pfad zum Solution-Verzeichnis
- `settings` - Scan-Konfiguration

**Returns:** Liste aller gefundenen Dateien

**Implementierungen:**
- `DefaultFileScanner`

---

### IProjectMetadataReader

Liest Metadaten aus `.csproj`-Dateien.

```csharp
public interface IProjectMetadataReader
{
    ProjectMetadata Read(string projectPath);
}
```

**Parameter:**
- `projectPath` - Pfad zur `.csproj`-Datei

**Returns:** Projekt-Metadaten (TargetFramework, Dependencies, etc.)

**Implementierungen:**
- `MsBuildProjectMetadataReader`

---

### IContentClassifier

Klassifiziert Dateitypen für Syntax-Highlighting.

```csharp
public interface IContentClassifier
{
    string Classify(string filePath);
}
```

**Parameter:**
- `filePath` - Pfad zur Datei

**Returns:** Sprach-Identifier (z.B. `"csharp"`, `"xml"`, `"json"`)

**Implementierungen:**
- `SimpleContentClassifier`

---

### IHashCalculator

Berechnet Hashes für Dateiinhalte.

```csharp
public interface IHashCalculator
{
    string Sha1(byte[] content);
}
```

**Parameter:**
- `content` - Dateiinhalt als Byte-Array

**Returns:** SHA1-Hash als Hex-String

**Implementierungen:**
- `Sha1HashCalculator`

---

### ISecretMasker

Maskiert sensible Daten in Dateien.

```csharp
public interface ISecretMasker
{
    string Process(string relativePath, string content);
}
```

**Parameter:**
- `relativePath` - Relativer Pfad der Datei (für kontextabhängige Maskierung)
- `content` - Original-Dateiinhalt

**Returns:** Inhalt mit maskierten Secrets

**Implementierungen:**
- `RegexSecretMasker`

**Beispiel:**
```csharp
var masker = new RegexSecretMasker();
var content = "{\"apiKey\": \"secret123\"}";
var masked = masker.Process("appsettings.json", content);
// => {"apiKey": "***MASKED***"}
```

---

### IBundleWriter

Schreibt Bundle-Ausgabe in eine Datei.

```csharp
public interface IBundleWriter
{
    string Write(string outputPath, List<FileEntry> files, 
                 List<ProjectMetadata> projects);
}
```

**Parameter:**
- `outputPath` - Ziel-Pfad für Bundle-Datei
- `files` - Liste aller Dateien
- `projects` - Liste aller Projekte

**Returns:** Pfad zur erstellten Datei

**Implementierungen:**
- `MarkdownBundleWriter`

---

### IBundleOrchestrator

Koordiniert den gesamten Bundle-Prozess.

```csharp
public interface IBundleOrchestrator
{
    string Run(string rootDirectory, ScanSettings settings);
}
```

**Parameter:**
- `rootDirectory` - Pfad zum Solution-Verzeichnis
- `settings` - Konfiguration

**Returns:** Pfad zur Bundle-Datei

**Implementierungen:**
- `BundleOrchestrator`

---

## Implementations

### DefaultFileScanner

Standard-Implementierung des File-Scannings.

```csharp
public class DefaultFileScanner : IFileScanner
{
    public List<FileEntry> Scan(string rootPath, ScanSettings settings)
    {
        // Rekursives Scannen mit Filterung
    }
}
```

**Features:**
- Rekursives Directory-Scanning
- Filterung nach Dateiendungen
- Ausschluss von `bin/`, `obj/`, `.git/`
- Optional: Testprojekte ausschließen

---

### MsBuildProjectMetadataReader

Liest `.csproj`-Dateien mit System.Xml.Linq.

```csharp
public class MsBuildProjectMetadataReader : IProjectMetadataReader
{
    public ProjectMetadata Read(string projectPath)
    {
        var doc = XDocument.Load(projectPath);
        // Parse TargetFramework, PackageReferences, etc.
    }
    
    public void EnrichBuildActions(List<FileEntry> files, string rootPath)
    {
        // Reichert FileEntry mit BuildAction-Werten an
    }
}
```

**Extrahiert:**
- TargetFramework
- OutputType
- NuGet PackageReferences
- ProjectReferences
- BuildAction-Werte (Compile, Page, Resource, Content, None)

**BuildAction-Ermittlung:**
1. Parst alle `.csproj`-Dateien im Projekt
2. Liest `<ItemGroup>`-Einträge (Compile, Page, Resource, Content, etc.)
3. Matched Dateipfade mit `Include`-Attributen
4. Fallback-Logik basierend auf Dateiendung bei fehlenden Einträgen

---

### SimpleContentClassifier

Klassifiziert Dateien nach Extension.

```csharp
public class SimpleContentClassifier : IContentClassifier
{
    public string Classify(string filePath)
    {
        return Path.GetExtension(filePath) switch
        {
            ".cs" => "csharp",
            ".xaml" => "xml",
            ".json" => "json",
            ".xml" => "xml",
            ".csproj" => "xml",
            _ => ""
        };
    }
}
```

---

### Sha1HashCalculator

SHA1-Hash-Berechnung mit System.Security.Cryptography.

```csharp
public class Sha1HashCalculator : IHashCalculator
{
    public string Sha1(byte[] content)
    {
        using var sha1 = System.Security.Cryptography.SHA1.Create();
        var hash = sha1.ComputeHash(content);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}
```

---

### RegexSecretMasker

Maskiert Secrets mit regulären Ausdrücken.

```csharp
public class RegexSecretMasker : ISecretMasker
{
    public string Process(string relativePath, string content)
    {
        // Maskiert nur in JSON/Config-Dateien
        if (!IsJsonOrConfig(relativePath))
            return content;
            
        // Maskiert: API-Keys, Passwörter, Connection-Strings
        return MaskSecrets(content);
    }
}
```

**Patterns:**
- API-Keys: `apiKey="..."`
- Passwörter: `password="..."`
- Connection-Strings: `Server=...;Password=...`

**Nur für:** `.json`, `.config`, `appsettings.*`

---

### MarkdownBundleWriter

Generiert Markdown-Ausgabe.

```csharp
public class MarkdownBundleWriter : IBundleWriter
{
    public MarkdownBundleWriter(ISecretMasker masker);
    
    public string Write(string outputPath, List<FileEntry> files, 
                        List<ProjectMetadata> projects)
    {
        // Generiert strukturiertes Markdown
    }
}
```

**Ausgabe-Format:**
```markdown
---
project_root: ProjectName
generated_at: 2024-12-15 10:30:00
tool: SolutionBundler v1
---

# Inhaltsverzeichnis
* [File1.cs](#file1-cs)
* [File2.cs](#file2-cs)

# Dateien

## File1.cs
_size_: 1234 bytes - _sha1_: abc123... - _action_: Compile

--- FILE: File1.cs | HASH: abc123... | ACTION: Compile ---

```csharp
// Code
```
```

**Intern verwendet:**
- `MarkdownGenerator` - Content-Generierung
- `FileContentReader` - Datei-Lesen mit Error-Handling
- `OutputPathResolver` - Pfad-Auflösung
- `MarkdownAnchorGenerator` - Anker-Generierung

---

### BundleOrchestrator

Koordiniert alle Komponenten.

```csharp
public class BundleOrchestrator : IBundleOrchestrator
{
    public BundleOrchestrator(
        IFileScanner scanner,
        IProjectMetadataReader metadataReader,
        IContentClassifier classifier,
        IHashCalculator hasher,
        IBundleWriter writer)
    {
        // Dependency Injection
    }

    public string Run(string rootDirectory, ScanSettings settings)
    {
        // 1. Scannen
        var files = _scanner.Scan(rootDirectory, settings);
        
        // 2. Projekte lesen
        var projects = files
            .Where(f => f.Path.EndsWith(".csproj"))
            .Select(f => _metadataReader.Read(f.Path))
            .ToList();
        
        // 3. Klassifizieren & Hashen
        foreach (var file in files)
        {
            file.ContentType = _classifier.Classify(file.Path);
            file.Hash = _hasher.Sha1(File.ReadAllBytes(file.Path));
        }
        
        // 4. Bundle schreiben
        return _writer.Write(outputPath, files, projects);
    }
}
```

---

## BundleWriting (Internal Helper Classes)

Diese Klassen sind `internal` und Teil des Refactorings von `MarkdownBundleWriter`.

### MarkdownGenerator

Generiert Markdown-Inhalt für Bundle-Dateien.

```csharp
namespace SolutionBundler.Core.Implementations.BundleWriting;

internal static class MarkdownGenerator
{
    public static string Generate(
        string projectName,
        IList<FileEntry> files,
        FileContentReader contentReader,
        bool maskSecrets);
}
```

**Parameter:**
- `projectName` - Projektname für FrontMatter
- `files` - Liste aller zu bundelnden Dateien
- `contentReader` - Reader für Dateiinhalte
- `maskSecrets` - Secrets maskieren?

**Returns:** Vollständiges Markdown-Dokument

**Generiert:**
1. **FrontMatter** - YAML-Header mit Metadaten
2. **Inhaltsverzeichnis** - Anker-Links zu allen Dateien
3. **Datei-Sektionen** - Code-Fences mit Sprach-Tags und Metadaten

**Beispiel-Output:**
```markdown
---
project_root: MyProject
generated_at: 2024-12-15 10:30:00
tool: SolutionBundler v1
---

# Inhaltsverzeichnis
* [Program.cs](#program-cs)

# Dateien

## Program.cs
_size_: 500 bytes - _sha1_: abc123 - _action_: Compile

--- FILE: Program.cs | HASH: abc123 | ACTION: Compile ---

```csharp
// Code hier
```
```

---

### FileContentReader

Liest Dateiinhalte mit Error-Handling und Secret-Masking.

```csharp
internal sealed class FileContentReader
{
    public FileContentReader(ISecretMasker masker);
    
    public string ReadContent(string fullPath, string relativePath, bool maskSecrets);
}
```

**Features:**
- Try-Catch für robustes Datei-Lesen
- Gibt Fehlermeldung bei nicht lesbaren Dateien zurück
- Integriert Secret-Masking wenn aktiviert

**Beispiel:**
```csharp
var reader = new FileContentReader(secretMasker);
var content = reader.ReadContent(@"C:\Path\file.cs", "file.cs", true);
// Bei Fehler: "/* FEHLER: Datei konnte nicht gelesen werden: ... */"
```

---

### OutputPathResolver

Auflösung von Ausgabepfaden und Dateinamen.

```csharp
internal static class OutputPathResolver
{
    public static string ResolveOutputPath(ScanSettings settings, string projectName);
}
```

**Logik:**
1. Bestimmt Dateinamen aus `settings.OutputFileName` oder nutzt `projectName` als Fallback
2. Fügt `.md`-Extension hinzu falls fehlend (case-insensitive)
3. Erstellt vollständigen Pfad: `%USERPROFILE%\Documents\SolutionBundler\Bundles\`
4. Erstellt Verzeichnis falls nicht vorhanden

**Beispiele:**
```csharp
// Custom-Name
var settings1 = new ScanSettings { OutputFileName = "MyBundle" };
var path1 = OutputPathResolver.ResolveOutputPath(settings1, "ProjectX");
// => C:\Users\...\Documents\SolutionBundler\Bundles\MyBundle.md

// Fallback zu Projektname
var settings2 = new ScanSettings { OutputFileName = null };
var path2 = OutputPathResolver.ResolveOutputPath(settings2, "MyProject");
// => C:\Users\...\Documents\SolutionBundler\Bundles\MyProject.md

// Extension wird nicht dupliziert
var settings3 = new ScanSettings { OutputFileName = "bundle.MD" };
var path3 = OutputPathResolver.ResolveOutputPath(settings3, "Project");
// => C:\Users\...\Documents\SolutionBundler\Bundles\bundle.MD (nicht .MD.md)
```

---

### MarkdownAnchorGenerator

Generiert URL-sichere Anker für Markdown-Links.

```csharp
internal static class MarkdownAnchorGenerator
{
    public static string Generate(string text);
}
```

**Transformation:**
- Leerzeichen ? `-`
- Slash (`/`) ? `-`
- Backslash (`\`) ? `-`
- Sonderzeichen ? entfernt
- Großbuchstaben ? Kleinbuchstaben

**Beispiele:**
```csharp
MarkdownAnchorGenerator.Generate("Folder/My File.cs")
// => "folder-my-file-cs"

MarkdownAnchorGenerator.Generate("Sub\\Path\\Class.cs")
// => "sub-path-class-cs"

MarkdownAnchorGenerator.Generate("Data (Model).cs")
// => "data-model-cs"
```

**Verwendung:** Generiert konsistente Anker für das Inhaltsverzeichnis.

---

## Models

### ScanSettings

Konfiguration für den Scan-Prozess.

```csharp
public class ScanSettings
{
    public bool MaskSecrets { get; set; }
    public bool IncludeTestProjects { get; set; }
    public string OutputFileName { get; set; }
}
```

---

### FileEntry

Repräsentiert eine gescannte Datei.

```csharp
public class FileEntry
{
    public string RelativePath { get; set; }
    public string FullPath { get; set; }
    public string Language { get; set; }
    public string Sha1 { get; set; }
    public long Size { get; set; }
    public BuildAction Action { get; set; }
}
```

---

### BuildAction

Enum für MSBuild BuildAction-Werte.

```csharp
public enum BuildAction
{
    Unknown,
    Compile,
    Page,
    Resource,
    Content,
    None
}
```

---

### ProjectMetadata

Projekt-Informationen aus `.csproj`.

```csharp
public class ProjectMetadata
{
    public string Name { get; set; }
    public string TargetFramework { get; set; }
    public List<string> Dependencies { get; set; }
}
```

---

### ProjectInfo

Projekt-Verwaltung in der UI.

```csharp
public class ProjectInfo
{
    public string Path { get; set; }
    // Weitere Properties...
}
```

---

## Storage

### ProjectStore

Wrapper für DataToolKit-Persistierung.

```csharp
public class ProjectStore
{
    public ReadOnlyObservableCollection<ProjectInfo> Projects { get; }
    
    public bool AddProject(string projectPath);
    public bool RemoveProject(string projectName);
    public void Clear();
}
```

**Verwendung:**

```csharp
var store = new ProjectStore(dataStoreProvider, repositoryFactory);
store.AddProject(@"C:\Projects\MyProject\MyProject.csproj");

foreach (var project in store.Projects)
{
    Console.WriteLine(project.Path);
}
```

**Persistierung:** Automatisch in `%AppData%/SolutionBundler/`

---

## Erweiterungen

### Custom Scanner

```csharp
public class GitIgnoreScanner : IFileScanner
{
    public List<FileEntry> Scan(string rootPath, ScanSettings settings)
    {
        // Respektiert .gitignore-Regeln
    }
}
```

### Custom Writer

```csharp
public class JsonBundleWriter : IBundleWriter
{
    public string Write(string outputPath, List<FileEntry> files, 
                        List<ProjectMetadata> projects)
    {
        var json = JsonSerializer.Serialize(new { files, projects });
        File.WriteAllText(outputPath, json);
        return outputPath;
    }
}
```

---

## Siehe auch

- [SolutionBundler.Core README](../README.md)
- [Workflow-Dokumentation](../../Docs/Workflow.md)
- [Developer Guide](../../Docs/Developer.md)
- [CHANGELOG](../../CHANGELOG.md)
