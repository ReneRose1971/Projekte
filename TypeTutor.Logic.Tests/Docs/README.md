# TypeTutor.Logic.Tests

Umfassendes Testprojekt für die TypeTutor.Logic-Bibliothek.

## Struktur

### DI/
Tests für Dependency Injection und Service-Registrierung
- `TypeTutorServiceModuleTests.cs` - Validiert die korrekte Service-Registrierung

### Data/
Tests für Datenmodelle und Persistierung
- `LessonDataTests.cs` - Tests für LessonData Record
- `LessonGuideDataTests.cs` - Tests für LessonGuideData Record
- `LessonDataEqualityComparerTests.cs` - Tests für Title-basierte Equality
- `LessonGuideDataEqualityComparerTests.cs` - Tests für LessonGuide Equality
- `DataStoreWrapperTests.cs` - Tests für DataStore-Integration

### Core/
Tests für Kern-Domänenlogik
- `TypingEngineStateTests.cs` - Tests für State-Objekte
- **Lesson/**
  - `LessonTests.cs` - Tests für Lesson-Klasse
  - `LessonFactoryTests.cs` - Tests für Lesson-Erstellung und Text-Normalisierung
  - `LessonMetricsTests.cs` - Tests für Metrik-Berechnungen

### Engine/
Tests für die Typing-Engine
- `TypingEngineTests.cs` - Umfassende Tests für die Kern-Engine

### Helpers/
Test-Utilities
- `ServiceProviderFixture.cs` - DI-Container für Tests
- `TestDataBuilder.cs` - Builder für konsistente Test-Daten

### _TestData/
Test-Ressourcen
- `sample-lessons.json` - Beispiel-Lektionen
- `sample-guides.json` - Beispiel-Guides

## Test-Konventionen

### Naming
- Test-Klassen: `{KlassenName}Tests`
- Test-Methoden: `{MethodName}_{Scenario}_{ExpectedBehavior}`

### Struktur (AAA-Pattern)
```csharp
[Fact]
public void MethodName_WithCondition_ShouldDoSomething()
{
    // Arrange
    var sut = new SystemUnderTest();
    
    // Act
    var result = sut.DoSomething();
    
    // Assert
    result.Should().Be(expected);
}
```

### Verwendete Frameworks
- **xUnit 2.9.3** - Test-Framework
- **FluentAssertions 8.8.0** - Assertions
- **Moq 4.20.72** - Mocking
- **Microsoft.Extensions.DependencyInjection 10.0.1** - DI

## Ausführen der Tests

### Visual Studio
- Test Explorer: `Test` ? `Run All Tests`
- Einzelner Test: Rechtsklick ? `Run Test(s)`

### Kommandozeile
```bash
dotnet test TypeTutor.Logic.Tests\TypeTutor.Logic.Tests.csproj
```

### Mit Code Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test-Kategorien

### Unit Tests
Testen isolierte Komponenten ohne externe Abhängigkeiten:
- Alle Data-Tests
- Core/Lesson-Tests
- Engine/TypingEngine-Tests (mit gemocktem Mapper)

### Integration Tests
Testen Zusammenspiel mehrerer Komponenten:
- DI/TypeTutorServiceModule-Tests
- Data/DataStoreWrapper-Tests

## Best Practices

### 1. Isolation
Jeder Test ist unabhängig und kann in beliebiger Reihenfolge ausgeführt werden.

### 2. Test Data Builder
Verwende `TestDataBuilder` für konsistente Test-Daten:
```csharp
var lesson = TestDataBuilder.CreateLessonData(
    title: "Test Lesson",
    difficulty: 1
);
```

### 3. FluentAssertions
Nutze lesbare Assertions:
```csharp
result.Should().NotBeNull();
result.Should().HaveCount(3);
result.Should().Contain(x => x.Title == "Test");
```

### 4. Theory Tests
Verwende `[Theory]` für parametrisierte Tests:
```csharp
[Theory]
[InlineData("input1", "expected1")]
[InlineData("input2", "expected2")]
public void Test_WithVariousInputs(string input, string expected)
{
    // Test implementation
}
```

## Erweiterung

### Neue Tests hinzufügen
1. Identifiziere die richtige Kategorie (DI/Data/Core/Engine)
2. Erstelle Test-Klasse mit `{ClassName}Tests` Suffix
3. Implementiere Tests nach AAA-Pattern
4. Verwende FluentAssertions für Assertions
5. Nutze TestDataBuilder für Test-Daten

### Neue Test-Kategorie hinzufügen
1. Erstelle neuen Ordner unter `TypeTutor.Logic.Tests/`
2. Folge der bestehenden Namenskonvention
3. Aktualisiere diese Dokumentation

## Code Coverage Ziele

- **Gesamt**: > 80%
- **Core/Engine**: > 90%
- **Data**: > 85%
- **DI**: > 75%

## Bekannte Einschränkungen

- Repository-Integration benötigt temporäre Dateien
- DataStore-Tests können Datei-I/O durchführen
- Einige Tests sind auf das DataToolKit-Framework angewiesen
