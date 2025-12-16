# DataToolKit.Tests

Unit- und Integrationstests für das DataToolKit-Projekt mit umfangreichem Fake-Framework für schnelle, isolierte Tests.

[![.NET Version](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Test Framework](https://img.shields.io/badge/Test%20Framework-xUnit-green.svg)](https://xunit.net/)

---

## ?? Überblick

**DataToolKit.Tests** enthält alle Tests für die DataToolKit-Komponenten:

- ? **Unit-Tests** - Isolierte Tests einzelner Komponenten
- ? **Integration-Tests** - Tests für Zusammenspiel mehrerer Komponenten
- ? **Fake-Framework** - In-Memory Fakes ohne I/O für schnelle Tests
- ? **Test-Helpers** - Builder, Fixtures und Utilities

### Technologie-Stack

| Komponente | Version | Zweck |
|------------|---------|-------|
| **xUnit** | 2.9.3 | Test-Framework |
| **TestHelper** | - | Test-Utilities (TestDirectorySandbox) |
| **LiteDB** | 5.0.21 | Für LiteDB-Repository-Tests |
| **.NET** | 8.0 | Target Framework |

---

## ?? Schnellstart

### Einfacher Test mit Fakes

```csharp
using DataToolKit.Tests.Fakes.Providers;
using Xunit;

public class MyTests
{
    [Fact]
    public void Test_With_Fake_Provider()
    {
        // Arrange
        var provider = new FakeDataStoreProvider();
        var store = provider.GetPersistent<Customer>(
            provider.RepositoryFactory,
            autoLoad: false
        );
        
        // Act
        store.Add(new Customer { Id = 0, Name = "Alice" });
        
        // Assert
        Assert.Equal(1, store.Count);
    }
}
```

### Mit xUnit-Fixture

```csharp
using DataToolKit.Tests.Fakes.Builders;
using Xunit;

public class CustomerTests : IClassFixture<DataStoreTestFixture<Customer>>
{
    private readonly DataStoreTestFixture<Customer> _fixture;
    
    public CustomerTests(DataStoreTestFixture<Customer> fixture)
    {
        _fixture = fixture;
        _fixture.Reset();
    }
    
    [Fact]
    public void Add_Should_Increase_Count()
    {
        _fixture.SeedData(new Customer { Id = 1, Name = "Alice" });
        _fixture.DataStore.Add(new Customer { Id = 2, Name = "Bob" });
        
        Assert.Equal(2, _fixture.DataStore.Count);
    }
}
```

---

## ?? Projekt-Struktur

```
DataToolKit.Tests/
??? README.md                           # Diese Datei
??? Docs/                               # Dokumentation
?   ??? API-Referenz.md                # Vollständige API-Dokumentation
?   ??? Fake-Framework.md              # Fake-Framework Guide
?   ??? Test-Patterns.md               # Best Practices
??? Fakes/                              # In-Memory Fake-Implementierungen
?   ??? Repositories/                  # Fake-Repositories
?   ??? Providers/                     # Fake-Provider
?   ??? Builders/                      # Test-Helpers & Builders
??? Examples/                           # Beispiel-Tests (29 Tests)
?   ??? Fakes/
??? Tests/                              # Produktions-Tests
    ??? Storage/
    ??? Abstractions/
```

---

## ?? Dokumentation

| Dokument | Beschreibung |
|----------|--------------|
| **[API-Referenz](./Docs/API-Referenz.md)** | Vollständige API-Dokumentation aller Fakes und Helpers |
| **[Fake-Framework](./Docs/Fake-Framework.md)** | Einführung, Features und Schnellstart |
| **[Test-Patterns](./Docs/Test-Patterns.md)** | Best Practices und bewährte Patterns |

---

## ?? Fake-Framework

Das **Fake-Framework** bietet In-Memory-Implementierungen aller wichtigen DataToolKit-Komponenten **ohne echte I/O**:

| Fake | Beschreibung |
|------|--------------|
| **FakeJsonRepository<T>** | In-Memory JSON-Repository mit History-Tracking |
| **FakeLiteDbRepository<T>** | In-Memory LiteDB mit Auto-ID und Delta-Sync |
| **FakeRepositoryFactory** | Factory für typsichere Repository-Verwaltung |
| **FakeDataStoreProvider** | DataStore-Provider mit Singleton-Management |
| **TestEntityBuilder<T>** | Fluent Builder für Test-Entities |
| **DataStoreTestFixture<T>** | xUnit-Fixture mit Auto-Setup/Cleanup |
| **RepositoryScenarioBuilder<T>** | Builder für komplexe Test-Szenarien |

?? **Details:** [Fake-Framework Guide](./Docs/Fake-Framework.md)

---

## ?? Tests ausführen

```bash
# Alle Tests
dotnet test DataToolKit.Tests/DataToolKit.Tests.csproj

# Mit Verbosity
dotnet test DataToolKit.Tests/DataToolKit.Tests.csproj -v detailed

# Nur Fake-Examples
dotnet test --filter "FullyQualifiedName~Example"

# In Visual Studio: Test Explorer ? Run All Tests
```

---

## ?? Test-Statistiken

| Kategorie | Anzahl Tests | Status |
|-----------|--------------|--------|
| **Repository-Tests** | ~20 | ? Passing |
| **DataStore-Tests** | ~15 | ? Passing |
| **Fake-Examples** | 29 | ? Passing |
| **Integration-Tests** | ~10 | ? Passing |

**Gesamtabdeckung:** Tests decken alle öffentlichen APIs von DataToolKit ab.

---

## ?? Best Practices

1. **Fakes statt echte I/O verwenden** - Tests laufen 100x schneller
2. **TestDirectorySandbox für Datei-Tests** - Automatisches Cleanup
3. **Fixtures für Setup/Teardown** - Gemeinsame Konfiguration
4. **Builder für komplexe Testdaten** - Lesbare und wartbare Tests
5. **History-Tracking für Assertions** - Verifiziere Repository-Aufrufe

?? **Details:** [Test-Patterns Guide](./Docs/Test-Patterns.md)

---

## ?? Siehe auch

- ?? [API-Referenz](./Docs/API-Referenz.md) - Vollständige Dokumentation aller Komponenten
- ?? [Fake-Framework Guide](./Docs/Fake-Framework.md) - Einführung in das Fake-Framework
- ?? [DataToolKit README](../DataToolKit/README.md) - Überblick über getestete Komponenten
- ?? [TestHelper README](../TestHelper/README.md) - Test-Utilities

---

## ?? Beitragen

Bei neuen Features in DataToolKit:

1. **Unit-Tests schreiben** - Isolierte Tests mit Fakes
2. **Fakes erweitern** - Falls neue Komponenten hinzukommen
3. **Beispiel-Test erstellen** - Dokumentation durch lebende Tests
4. **Build prüfen** - Alle Tests müssen bestehen

---

## ?? Lizenz

Dieses Projekt unterliegt der MIT-Lizenz - siehe [../LICENSE](../LICENSE) für Details.
