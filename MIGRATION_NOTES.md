# Migration DataToolKit.Tests ? TestHelper.DataToolKit

## Übersicht

Alle wiederverwendbaren Test-Helper, Fakes, Fixtures, Builder und Utilities aus `DataToolKit.Tests` wurden in ein neues Klassenbibliotheks-Projekt `TestHelper.DataToolKit` ausgelagert.

**Wichtig:** Die **Examples**-Dateien in `DataToolKit.Tests/Examples/` sind **NICHT Teil der Migration**. Sie bleiben in `DataToolKit.Tests`, da sie Dokumentations- und Demo-Tests für die Test-Helper sind, keine Helper selbst.

## Datum der Migration
**Durchgeführt am:** 2024 (automatische Migration)

## Verschobene Dateien

### 1. Fakes/Repositories ? TestHelper.DataToolKit/Fakes/Repositories

| Alter Namespace | Neuer Namespace |
|-----------------|-----------------|
| `DataToolKit.Tests.Fakes.Repositories.FakeJsonRepository<T>` | `TestHelper.DataToolKit.Fakes.Repositories.FakeJsonRepository<T>` |
| `DataToolKit.Tests.Fakes.Repositories.FakeLiteDbRepository<T>` | `TestHelper.DataToolKit.Fakes.Repositories.FakeLiteDbRepository<T>` |
| `DataToolKit.Tests.Fakes.Repositories.FakeRepositoryFactory` | `TestHelper.DataToolKit.Fakes.Repositories.FakeRepositoryFactory` |
| `DataToolKit.Tests.Common.FakeRepositoryBase<T>` | `TestHelper.DataToolKit.Fakes.Repositories.FakeRepositoryBase<T>` |
| `DataToolKit.Tests.Common.FakeRepository<T>` | `TestHelper.DataToolKit.Fakes.Repositories.FakeRepository<T>` |

**Zusätzliche Typen in diesem Namespace:**
- `RepositoryOperation` (Record für Test-Assertions in FakeJsonRepository)

### 2. Fakes/Providers ? TestHelper.DataToolKit/Fakes/Providers

| Alter Namespace | Neuer Namespace |
|-----------------|-----------------|
| `DataToolKit.Tests.Fakes.Providers.FakeDataStoreProvider` | `TestHelper.DataToolKit.Fakes.Providers.FakeDataStoreProvider` |

### 3. Builders ? TestHelper.DataToolKit/Builders

| Alter Namespace | Neuer Namespace |
|-----------------|-----------------|
| `DataToolKit.Tests.Fakes.Builders.RepositoryScenarioBuilder<T>` | `TestHelper.DataToolKit.Builders.RepositoryScenarioBuilder<T>` |
| `DataToolKit.Tests.Fakes.Builders.TestEntityBuilder` | `TestHelper.DataToolKit.Builders.TestEntityBuilder` |

### 4. Fixtures ? TestHelper.DataToolKit/Fixtures

| Alter Namespace | Neuer Namespace |
|-----------------|-----------------|
| `DataToolKit.Tests.Fakes.Builders.DataStoreTestFixture<T>` | `TestHelper.DataToolKit.Fixtures.DataStoreTestFixture<T>` |

### 5. Testing (Test-Entities & Module) ? TestHelper.DataToolKit/Testing

| Alter Namespace | Neuer Namespace |
|-----------------|-----------------|
| `DataToolKit.Tests.Common.TestDto` | `TestHelper.DataToolKit.Testing.TestDto` |
| `DataToolKit.Tests.Common.TestDtoComparer` | `TestHelper.DataToolKit.Testing.TestDtoComparer` |
| `DataToolKit.Tests.Testing.TestEntity` | `TestHelper.DataToolKit.Testing.TestEntity` |
| `DataToolKit.Tests.Testing.TestEntityComparer` | `TestHelper.DataToolKit.Testing.TestEntityComparer` |
| `DataToolKit.Tests.Common.IntegrationTestModule` | `TestHelper.DataToolKit.Testing.IntegrationTestModule` |
| `DataToolKit.Tests.Common.ConfigurableFakeRepositoryFactory` | `TestHelper.DataToolKit.Testing.ConfigurableFakeRepositoryFactory` |

## Wichtige Änderungen

### Namespace-Änderungen in Tests

Alle Tests in `DataToolKit.Tests` und anderen Projekten müssen ihre `using`-Statements aktualisieren:

**Vorher:**
```csharp
using DataToolKit.Tests.Fakes.Repositories;
using DataToolKit.Tests.Fakes.Providers;
using DataToolKit.Tests.Fakes.Builders;
using DataToolKit.Tests.Common;
using DataToolKit.Tests.Testing;
```

**Nachher:**
```csharp
using TestHelper.DataToolKit.Fakes.Repositories;
using TestHelper.DataToolKit.Fakes.Providers;
using TestHelper.DataToolKit.Builders;
using TestHelper.DataToolKit.Fixtures;
using TestHelper.DataToolKit.Testing;
```

### Spezielle Namespace-Korrekturen

Einige Production-Code-Typen erforderten zusätzliche using-Statements:

```csharp
// Für PropertyChangedBinder<T> (internal, aber über InternalsVisibleTo zugänglich):
using DataToolKit.Storage.Persistence;

// Für InMemoryDataStore<T>, DataStoreFactory, DataStoreProvider:
using DataToolKit.Storage.DataStores;

// Für IDataStore<T>:
using DataToolKit.Abstractions.DataStores;

// Für SyncWith Extension-Methode:
using DataToolKit.Storage.Extensions;
```

### Projekt-Referenzen

**DataToolKit.Tests.csproj** hat jetzt eine neue Projekt-Referenz:
```xml
<ProjectReference Include="..\TestHelper.DataToolKit\TestHelper.DataToolKit.csproj" />
```

**SolutionBundler.Tests.csproj** wurde ebenfalls aktualisiert:
```xml
<!-- ENTFERNT: -->
<ProjectReference Include="..\DataToolKit.Tests\DataToolKit.Tests.csproj" />

<!-- HINZUGEFÜGT: -->
<ProjectReference Include="..\TestHelper.DataToolKit\TestHelper.DataToolKit.csproj" />
```

## Projektstruktur TestHelper.DataToolKit

```
TestHelper.DataToolKit/
??? Fakes/
?   ??? Repositories/
?   ?   ??? FakeJsonRepository.cs          (mit RepositoryOperation record)
?   ?   ??? FakeLiteDbRepository.cs
?   ?   ??? FakeRepositoryFactory.cs
?   ?   ??? FakeRepositoryBase.cs
?   ?   ??? FakeRepository.cs
?   ??? Providers/
?       ??? FakeDataStoreProvider.cs
??? Builders/
?   ??? RepositoryScenarioBuilder.cs
?   ??? TestEntityBuilder.cs
??? Fixtures/
?   ??? DataStoreTestFixture.cs
??? Testing/
?   ??? TestDto.cs                         (mit TestDtoComparer)
?   ??? TestEntity.cs                      (mit TestEntityComparer)
?   ??? IntegrationTestModule.cs
?   ??? ConfigurableFakeRepositoryFactory.cs
??? TestHelper.DataToolKit.csproj
```

## Betroffene Dateien

### ? DataToolKit.Tests - Vollständig aktualisiert

#### Beispiel-Tests (Examples/) - **VERBLEIBEN in DataToolKit.Tests**
Diese Dateien sind **Dokumentations-Tests** und wurden **NICHT verschoben**:
- `Examples/Fakes/FakeJsonRepository_Example_Tests.cs` *(Demo für FakeJsonRepository)*
- `Examples/Fakes/FakeLiteDbRepository_Example_Tests.cs` *(Demo für FakeLiteDbRepository)*
- `Examples/Fakes/FakeDataStoreProvider_Example_Tests.cs` *(Demo für FakeDataStoreProvider)*

**Zweck der Examples:**
- ? Zeigen Best Practices für die Verwendung der Test-Helper
- ? Dienen als lebende Dokumentation ("Documentation by Code")
- ? Validieren, dass die Helper korrekt funktionieren
- ? Werden weiterhin gepflegt und aktualisiert

#### Integrations-Tests (Tests/Integration/)
- `Tests/Integration/JsonRepository_IntegrationTests.cs`
- `Tests/Integration/LiteDbRepository_IntegrationTests.cs`
- `Tests/Integration/DataStoreProvider_Json_IntegrationTests.cs`
- `Tests/Integration/DataStoreProvider_LiteDb_IntegrationTests.cs`

#### DataStore-Tests (Tests/DataStores/)
- `Tests/DataStores/Factory/DataStoreFactoryTests.cs`
- `Tests/DataStores/Provider/DataStoreProvider_AutoLoadTests.cs`
- `Tests/DataStores/Provider/DataStoreProvider_CacheManagementTests.cs`
- `Tests/DataStores/Provider/DataStoreProvider_SingletonTests.cs`
- `Tests/DataStores/Provider/DataStoreProvider_ThreadSafetyTests.cs`

#### Storage-Tests (Tests/Storage/)
- `Tests/Storage/DataStores/PersistentDataStore/PersistentDataStoreTests.cs`
- `Tests/Storage/DataStores/PersistentDataStore/PropertyChangedBinderTests.cs`
- `Tests/Storage/DataStores/PersistentDataStore/PropertyChangedBinder_DataStoreMode_Tests.cs`
- `Tests/Storage/Extensions/DataStoreSyncExtensions_Tests.cs`

#### Relationship-Tests (Tests/Relationships/)
- `Tests/Relationships/ParentChildRelationship_Tests.cs`
- `Tests/Relationships/ParentChildRelationship_EdgeCases_Tests.cs`

### ? SolutionBundler.Tests - Vollständig aktualisiert

- `Storage/ProjectStoreTests.cs`
- `ViewModels/ProjectListEditorViewModelTests.cs`

## Häufige Fehler und Lösungen

### Problem: "Der Typ PropertyChangedBinder<> wurde nicht gefunden"
**Lösung:** Füge hinzu:
```csharp
using DataToolKit.Storage.Persistence;
```

### Problem: "Der Typ InMemoryDataStore<> wurde nicht gefunden"
**Lösung:** Füge hinzu:
```csharp
using DataToolKit.Storage.DataStores;
```

### Problem: "SyncWith Extension-Methode wurde nicht gefunden"
**Lösung:** Füge hinzu:
```csharp
using DataToolKit.Storage.Extensions;
```

### Problem: "FakeDataStoreProvider wurde nicht gefunden"
**Lösung:** 
1. Füge Projekt-Referenz hinzu: `<ProjectReference Include="..\TestHelper.DataToolKit\TestHelper.DataToolKit.csproj" />`
2. Aktualisiere using: `using TestHelper.DataToolKit.Fakes.Providers;`

## Benefits der Migration

? **Wiederverwendbarkeit:** Test-Helper können jetzt von anderen Test-Projekten verwendet werden  
? **Klarheit:** Klare Trennung zwischen echten Tests und Test-Infrastruktur  
? **Wartbarkeit:** Zentrale Verwaltung aller DataToolKit-bezogenen Test-Helper  
? **Konsistenz:** Einheitliche Test-Infrastruktur für alle DataToolKit-Tests  
? **Keine Zirkelbezüge:** Andere Test-Projekte können Helper nutzen ohne DataToolKit.Tests zu referenzieren

## Status

### ? Abgeschlossen
- [x] TestHelper.DataToolKit Projekt erstellt
- [x] Alle Test-Helper verschoben
- [x] DataToolKit.Tests vollständig aktualisiert (30+ Dateien)
- [x] SolutionBundler.Tests vollständig aktualisiert (2 Dateien)
- [x] Build erfolgreich (keine Compiler-Fehler)

### ?? Potenziell betroffen (noch nicht migriert)
Falls weitere Test-Projekte existieren, die alte Namespaces verwenden, müssen diese separat aktualisiert werden.

## Rückbau (falls notwendig)

Falls die Migration rückgängig gemacht werden muss:
1. Alle Dateien von `TestHelper.DataToolKit/*` zurück nach `DataToolKit.Tests/` verschieben
2. Namespaces in allen verschobenen Dateien zurückändern
3. Alle using-Statements in Test-Dateien zurückändern
4. Projekt-Referenzen entfernen
5. `TestHelper.DataToolKit.csproj` löschen

**Hinweis:** Der Rückbau wird NICHT empfohlen, da die neue Struktur erhebliche Vorteile bietet.
