# Teststrategie - CustomWPFControls

## ?? Testziele

### **1. Unit-Tests (Komponenten-Ebene)**
- **ViewModelBase<T>**
  - Equals/GetHashCode-Verhalten
  - Model-Property-Zugriff
  - ToString-Delegation
  - PropertyChanged-Events (Fody)

- **ViewModelFactory<T, TViewModel>**
  - ViewModel-Erstellung mit DI
  - Exception-Handling bei fehlenden Dependencies
  - ArgumentNullException bei null-Model

- **ViewModelFactoryExtensions**
  - Korrekte DI-Registrierung
  - Singleton-Verhalten

---

### **2. Integration-Tests (System-Ebene)**
- **CollectionViewModel<TModel, TViewModel>**
  - DataStore-Integration
  - Bidirektionale Synchronisation (DataStore ? ViewModels)
  - IEqualityComparer-Verwendung
  - ViewModel-Lifecycle (Create/Dispose)

- **EditableCollectionViewModel<TModel, TViewModel>**
  - Command-Ausführung
  - CreateModel/EditModel-Callbacks
  - CanExecute-Logik

---

### **3. Behavior-Tests (Szenarien)**
- **Synchronisation DataStore ? ViewModels**
  - Add in DataStore ? ViewModel erstellt
  - Remove in DataStore ? ViewModel disposed
  - Clear in DataStore ? Alle ViewModels disposed

- **Synchronisation ViewModels ? DataStore**
  - AddModel() ? DataStore.Add()
  - RemoveViewModel() ? DataStore.Remove()
  - Clear() ? DataStore.Clear()

- **Commands**
  - AddCommand ? CreateModel + AddModel
  - DeleteCommand ? RemoveViewModel
  - ClearCommand ? Clear
  - EditCommand ? EditModel-Callback

---

## ?? Test-Kategorien

### **Unit-Tests (Fast, Isolated)**
```
[Fact] - Einzelne Methode/Property
[Theory] - Parametrisierte Tests
```

### **Integration-Tests (Medium, Dependencies)**
```
[Fact] - DataStore + Factory + Comparer
```

### **Behavior-Tests (Slow, E2E)**
```
[Fact] - Komplette Szenarien mit mehreren Operationen
```

---

## ??? Test-Struktur

```
CustomWPFControls.Tests/
??? Unit/
?   ??? ViewModelBaseTests.cs
?   ??? ViewModelFactoryTests.cs
?   ??? ViewModelFactoryExtensionsTests.cs
??? Integration/
?   ??? CollectionViewModelIntegrationTests.cs
?   ??? EditableCollectionViewModelIntegrationTests.cs
??? Behavior/
?   ??? BidirectionalSyncTests.cs
?   ??? CommandBehaviorTests.cs
??? Testing/
    ??? TestModel.cs
    ??? TestViewModel.cs
    ??? TestHelpers.cs
```

---

## ? Test-Abdeckungsziele

| Komponente | Ziel | Priorität |
|------------|------|-----------|
| ViewModelBase | 95% | Hoch |
| Factory | 90% | Hoch |
| CollectionViewModel | 90% | Kritisch |
| EditableCollectionViewModel | 85% | Hoch |
| Extensions | 80% | Mittel |

---

## ?? Test-Patterns

### **AAA-Pattern (Arrange-Act-Assert)**
```csharp
[Fact]
public void AddModel_NewModel_AddsToDataStore()
{
    // Arrange
    var dataStore = CreateDataStore();
    var viewModel = CreateCollectionViewModel(dataStore);
    var model = new TestModel { Id = 1 };

    // Act
    var result = viewModel.AddModel(model);

    // Assert
    Assert.True(result);
    Assert.Equal(1, dataStore.Count);
}
```

### **Given-When-Then (Behavior)**
```csharp
[Fact]
public void GivenDataStoreWithItem_WhenItemRemoved_ThenViewModelDisposed()
{
    // Given
    var model = new TestModel { Id = 1 };
    var dataStore = CreateDataStore();
    dataStore.Add(model);
    var viewModel = CreateCollectionViewModel(dataStore);

    // When
    dataStore.Remove(model);

    // Then
    Assert.Equal(0, viewModel.Count);
    // Verify ViewModel was disposed (via mock/spy)
}
```

---

## ?? Kritische Testszenarien

### **1. Memory Leaks verhindern**
```csharp
- ViewModels werden disposed bei Remove
- Event-Handler werden unsubscribed bei Dispose
- Dictionary wird geleert bei Clear
```

### **2. Thread-Safety (via DataStore)**
```csharp
- Concurrent Add/Remove
- Synchronisation auf UI-Thread (WPF)
```

### **3. Edge Cases**
```csharp
- Null-Model ? ArgumentNullException
- Duplicate Add ? Ignoriert (via IEqualityComparer)
- Remove nicht-existierendes Item ? false
- Clear bei leerem DataStore ? Kein Crash
```

---

## ?? Test-Pyramide

```
       /\
      /  \     E2E (Behavior) - 10%
     /____\    
    /      \   Integration - 30%
   /________\  
  /          \ Unit - 60%
 /____________\
```

---

## ?? CI/CD Integration

### **Build-Pipeline**
```yaml
- restore
- build
- test (all)
- coverage report (>= 80%)
- publish test results
```

### **Test-Kategorien für CI**
```csharp
[Trait("Category", "Unit")]
[Trait("Category", "Integration")]
[Trait("Category", "Behavior")]
```

---

## ?? Best Practices

1. ? **Lesbare Test-Namen**
   - `Method_Scenario_ExpectedBehavior`
   - Beispiel: `AddModel_DuplicateModel_ReturnsFalse`

2. ? **Ein Assert pro Test** (wo möglich)
   - Fokussiert auf eine Assertion
   - Ausnahme: Setup-Validierung

3. ? **Test-Isolation**
   - Jeder Test erstellt eigene Instanzen
   - Keine shared state zwischen Tests

4. ? **Mocks sparsam einsetzen**
   - Nur für externe Dependencies
   - Prefer Fakes über Mocks (z.B. FakeDataStore)

5. ? **Arrange-Phase klein halten**
   - Test-Helpers für komplexes Setup
   - Factory-Methods für Test-Objekte

---

## ?? Code-Coverage-Tools

- **Visual Studio**: Built-in Code Coverage
- **Coverlet**: Cross-platform .NET coverage
- **ReportGenerator**: HTML-Reports

---

## ?? Metriken

### **Erfolgs-Kriterien**
- ? Alle Tests grün
- ? Code Coverage >= 80%
- ? Keine Memory Leaks
- ? Performance: < 1s für alle Unit-Tests

### **Quality Gates**
- ? Fehlschlagende Tests blockieren Merge
- ? Coverage < 80% blockiert Merge
- ?? Warning bei > 100ms pro Unit-Test
