# Test-Strategie: DataStoreProvider & DataStoreFactory

Umfassender Test-Plan für das Provider/Factory-Pattern zur DataStore-Verwaltung.

## ?? Test-Kategorien

### 1. **DataStoreFactory Tests**
Ziel: Validierung der Factory-Erzeugungslogik (ohne Singleton-Management)

#### 1.1 InMemoryDataStore Creation
- ? `CreateInMemoryStore_Returns_NewInstance`
- ? `CreateInMemoryStore_WithComparer_UsesComparer`
- ? `CreateInMemoryStore_WithoutComparer_UsesDefault`
- ? `CreateInMemoryStore_Multiple_ReturnsDistinctInstances`

#### 1.2 PersistentDataStore Creation
- ? `CreatePersistentStore_Returns_NewInstance`
- ? `CreatePersistentStore_WithTrackingEnabled_BindsPropertyChanged`
- ? `CreatePersistentStore_WithTrackingDisabled_DoesNotBindPropertyChanged`
- ? `CreatePersistentStore_DoesNotAutoLoad` (Factory lädt nie automatisch)
- ? `CreatePersistentStore_WithNullRepository_ThrowsArgumentNullException`

---

### 2. **DataStoreProvider Singleton Tests**
Ziel: Validierung des Singleton-Managements

#### 2.1 InMemoryDataStore Singleton
- ? `GetInMemory_WithSingletonTrue_ReturnsSameInstance`
- ? `GetInMemory_WithSingletonFalse_ReturnsNewInstance`
- ? `GetInMemory_TwoCallsWithSingleton_ReturnsSameReference`
- ? `GetInMemory_DifferentTypes_ReturnsDifferentInstances`

#### 2.2 PersistentDataStore Singleton
- ? `GetPersistent_WithSingletonTrue_ReturnsSameInstance`
- ? `GetPersistent_WithSingletonFalse_ReturnsNewInstance`
- ? `GetPersistent_TwoCallsWithSingleton_ReturnsSameReference`
- ? `GetPersistent_DifferentTypes_ReturnsDifferentInstances`

---

### 3. **AutoLoad Tests**
Ziel: Validierung der AutoLoad-Funktionalität im Provider

#### 3.1 Synchroner AutoLoad
- ? `GetPersistent_WithAutoLoadTrue_LoadsData`
- ? `GetPersistent_WithAutoLoadFalse_DoesNotLoadData`
- ? `GetPersistent_NonSingleton_WithAutoLoad_LoadsData`

#### 3.2 Asynchroner AutoLoad
- ? `GetPersistentAsync_WithAutoLoadTrue_LoadsDataAsync`
- ? `GetPersistentAsync_WithAutoLoadFalse_DoesNotLoadData`
- ? `GetPersistentAsync_Singleton_LoadsOnlyOnce`

---

### 4. **Thread-Safety Tests**
Ziel: Validierung der Thread-Sicherheit

#### 4.1 Concurrent Singleton Access
- ? `GetInMemory_ConcurrentCalls_ReturnsSameSingleton`
- ? `GetPersistent_ConcurrentCalls_ReturnsSameSingleton`
- ? `GetInMemoryAsync_ConcurrentCalls_ReturnsSameSingleton`
- ? `GetPersistentAsync_ConcurrentCalls_ReturnsSameSingleton`

#### 4.2 Mixed Operations
- ? `ConcurrentGetAndRemove_ThreadSafe`
- ? `ConcurrentGetAndClearAll_ThreadSafe`

---

### 5. **Cache-Management Tests**
Ziel: Validierung von RemoveSingleton und ClearAll

#### 5.1 RemoveSingleton
- ? `RemoveSingleton_ExistingSingleton_ReturnsTrue_DisposesStore`
- ? `RemoveSingleton_NonExistingSingleton_ReturnsFalse`
- ? `RemoveSingleton_ThenGetAgain_CreatesNewInstance`

#### 5.2 ClearAll
- ? `ClearAll_DisposesAllSingletons`
- ? `ClearAll_ThenGetAgain_CreatesNewInstances`
- ? `ClearAll_EmptyCache_DoesNotThrow`

---

### 6. **IRepositoryFactory Integration Tests**
Ziel: Validierung der Integration mit IRepositoryFactory

#### 6.1 Repository Resolution
- ? `GetPersistent_UsesRepositoryFactory_ToResolveRepository`
- ? `GetPersistent_WithMockedFactory_CallsGetJsonRepository`
- ? `GetPersistent_FactoryThrows_PropagatesToCaller`

#### 6.2 JSON vs. LiteDB
- ? `GetPersistent_WithJsonRepository_CreatesJsonBasedStore`
- ? `GetPersistent_WithLiteDbRepository_CreatesLiteDbBasedStore`

---

### 7. **Async Pattern Tests**
Ziel: Validierung der async-Implementierung

#### 7.1 GetInMemoryAsync
- ? `GetInMemoryAsync_CompletesSuccessfully`
- ? `GetInMemoryAsync_WithSingleton_ThreadSafe`

#### 7.2 GetPersistentAsync
- ? `GetPersistentAsync_WithAutoLoad_LoadsDataAsync`
- ? `GetPersistentAsync_WithSingleton_ReturnsExistingInstance`
- ? `GetPersistentAsync_CancellationToken_Honored` (falls hinzugefügt)

---

### 8. **Dispose Tests**
Ziel: Validierung des Dispose-Verhaltens

#### 8.1 Provider Dispose
- ? `Dispose_CallsClearAll`
- ? `Dispose_DisposesAllManagedSingletons`
- ? `Dispose_CanBeCalledMultipleTimes`

#### 8.2 Singleton Disposal
- ? `RemoveSingleton_CallsDisposeOnStore`
- ? `ClearAll_CallsDisposeOnAllStores`
- ? `NonDisposableStore_DoesNotThrow`

---

### 9. **Error Handling Tests**
Ziel: Validierung des Fehlerverhaltens

#### 9.1 Null Arguments
- ? `Constructor_NullFactory_ThrowsArgumentNullException`
- ? `GetPersistent_NullRepositoryFactory_ThrowsArgumentNullException`

#### 9.2 Repository Errors
- ? `GetPersistent_AutoLoad_RepositoryLoadFails_PropagatesException`
- ? `GetPersistent_FactoryResolveFails_PropagatesException`

---

### 10. **Integration Scenario Tests**
Ziel: End-to-End Szenarien

#### 10.1 Typical Usage
- ? `Scenario_ViewModel_UsesProvider_WithAutoLoad`
- ? `Scenario_MultipleViewModels_ShareSingleton`
- ? `Scenario_LocalStore_DoesNotAffectSingleton`

#### 10.2 Mixed Store Types
- ? `Scenario_InMemory_And_Persistent_Independent`
- ? `Scenario_MultipleTypes_ManagedSeparately`

---

## ?? Test-Coverage Ziele

| Komponente | Ziel | Priorität |
|------------|------|-----------|
| DataStoreFactory | 100% | Hoch |
| DataStoreProvider | 95%+ | Kritisch |
| Thread-Safety | 100% | Kritisch |
| AutoLoad | 100% | Hoch |
| Dispose | 100% | Hoch |
| Error Handling | 90%+ | Mittel |

---

## ?? Test-Tools & Patterns

### Mocking Strategy
```csharp
// Mock IRepositoryFactory
var mockFactory = new Mock<IRepositoryFactory>();
mockFactory.Setup(f => f.GetJsonRepository<Customer>())
    .Returns(mockRepository.Object);
```

### Thread-Safety Testing
```csharp
// Parallel Test Pattern
var tasks = Enumerable.Range(0, 100)
    .Select(_ => Task.Run(() => provider.GetInMemory<Customer>()))
    .ToArray();

await Task.WhenAll(tasks);
var distinct = tasks.Select(t => t.Result).Distinct().Count();
Assert.Equal(1, distinct); // Alle sollten gleiche Instanz sein
```

### AutoLoad Verification
```csharp
// FakeRepository mit Tracking
var fakeRepo = new FakeRepositoryBase<Customer>();
fakeRepo.SetData(testData);

var store = provider.GetPersistent(factory, autoLoad: true);
Assert.Equal(testData.Count, store.Count); // Daten geladen
```

---

## ?? Test-Struktur

```
DataToolKit.Tests/
??? Tests/
    ??? DataStores/
        ??? Factory/
        ?   ??? DataStoreFactoryTests.cs
        ?   ??? DataStoreFactory_CreationTests.cs
        ??? Provider/
            ??? DataStoreProviderTests.cs
            ??? DataStoreProvider_SingletonTests.cs
            ??? DataStoreProvider_AutoLoadTests.cs
            ??? DataStoreProvider_ThreadSafetyTests.cs
            ??? DataStoreProvider_CacheManagementTests.cs
            ??? DataStoreProvider_IntegrationTests.cs
```

---

## ?? Implementierungs-Reihenfolge

1. **Phase 1:** Factory Tests (einfach, isoliert)
2. **Phase 2:** Provider Singleton Tests (Kern-Funktionalität)
3. **Phase 3:** AutoLoad Tests (wichtiges Feature)
4. **Phase 4:** Thread-Safety Tests (kritisch)
5. **Phase 5:** Cache-Management Tests
6. **Phase 6:** Integration & Scenarios

---

**Soll ich mit der Implementierung der Tests beginnen?** ??
