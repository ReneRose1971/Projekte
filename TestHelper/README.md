# TestHelper

Hilfsklassen und Utilities für Unit- und Integrationstests in .NET 8 Projekten.

---

## ?? Installation

```bash
# Lokale Referenz (in Test-Projekten)
<ProjectReference Include="..\TestHelper\TestHelper.csproj" />
```

---

## ? Schnellstart

### TestDirectorySandbox

Automatisch verwaltetes temporäres Verzeichnis für I/O-Tests:

```csharp
using TestHelper.TestUtils;
using Xunit;

public class FileSystemTests : IDisposable
{
    private readonly TestDirectorySandbox _sandbox;

    public FileSystemTests()
    {
        _sandbox = new TestDirectorySandbox();
        // Temporäres Verzeichnis ist erstellt
    }

    [Fact]
    public void Test_FileOperations()
    {
        var filePath = _sandbox.PathOf("test.txt");
        File.WriteAllText(filePath, "Hello World");
        
        Assert.True(File.Exists(filePath));
    }

    public void Dispose()
    {
        _sandbox.Dispose(); // Automatisches Cleanup
    }
}
```

### RecordingSynchronizationContext

Hilfsklasse für Threading- und SynchronizationContext-Tests:

```csharp
using TestHelpers;

[Fact]
public void Test_SynchronizationContext()
{
    var context = new RecordingSynchronizationContext();
    
    using (context.Use())
    {
        // Code wird im Recording-Context ausgeführt
        SynchronizationContext.Current.Send(_ => 
        {
            // Aktion wird protokolliert
        }, null);
    }
    
    Assert.Single(context.SendThreadIds);
}
```

---

## ?? Features

| Feature | Beschreibung |
|---------|-------------|
| **TestDirectorySandbox** | Temporäre Testverzeichnisse mit Auto-Cleanup |
| **RecordingSynchronizationContext** | Thread-Testing für async/await Patterns |
| **Auto-Cleanup** | Keine manuellen Aufräumarbeiten nötig |
| **Thread-Safe** | Sichere parallele Test-Ausführung |

---

## ?? API-Übersicht

### TestDirectorySandbox

```csharp
public sealed class TestDirectorySandbox : IDisposable
{
    public string Root { get; }
    public string PathOf(string relative);
    public string EnsureFolder(string relativeFolder);
    public void Dispose();
}
```

**Verwendung:**
- Erstellt eindeutiges Temp-Verzeichnis
- `PathOf()` kombiniert Pfade sicher
- `EnsureFolder()` erstellt Unterverzeichnisse
- `Dispose()` löscht alles automatisch

### RecordingSynchronizationContext

```csharp
public sealed class RecordingSynchronizationContext : SynchronizationContext
{
    public ConcurrentQueue<int> SendThreadIds { get; }
    public int PostCount { get; }
    public IDisposable Use();
    public void Reset();
}
```

**Verwendung:**
- Protokolliert alle `Send()` und `Post()` Aufrufe
- `Use()` setzt als aktuellen Context
- `Reset()` löscht Protokoll

---

## ? Best Practices

### Do's
- ? Verwenden Sie `TestDirectorySandbox` für alle I/O-Tests
- ? Nutzen Sie `using` oder `IDisposable` für Auto-Cleanup
- ? Verwenden Sie relative Pfade über `PathOf()`
- ? Testen Sie SynchronizationContext mit `RecordingSynchronizationContext`

### Don'ts
- ? Keine hartcodierten Pfade außerhalb der Sandbox
- ? Kein manuelles Löschen von Test-Dateien
- ? Keine Test-Dateien im Projekt-Verzeichnis

---

## ?? Links

- **Repository**: https://github.com/ReneRose1971/Libraries
- **Issues**: https://github.com/ReneRose1971/Libraries/issues

---

## ?? Lizenz

MIT License
