# PropertyChangedBinder

## Übersicht

`PropertyChangedBinder<T>` ist ein Hilfsklasse zur idempotenten Verwaltung von `INotifyPropertyChanged`-Event-Handlern mit automatischer Doppelbindungs-Verhinderung.

## Kernmerkmale

- ? **Idempotentes Attach** - Mehrfaches Attach derselben Entität ist sicher
- ? **Zwei Modi** - Manueller Modus oder automatischer DataStore-Modus
- ? **Referenzbasiertes Tracking** - Verhindert Doppelbindungen zuverlässig
- ? **Automatische Synchronisation** - Im DataStore-Modus reagiert auf CollectionChanged
- ? **Memory-effizient** - Keine Duplikate, sauberes Cleanup

## Zwei Betriebsmodi

### 1. Manueller Modus

Explizite Kontrolle über Attach/Detach-Operationen.

**Verwendung:**
```csharp
using DataToolKit.Storage.Persistence;

var binder = new PropertyChangedBinder<Customer>(
    enabled: true,
    onEntityChanged: customer => 
    {
        Console.WriteLine($"Customer {customer.Id} changed");
        repository.Update(customer);
    });

// Manuell binden
var customer = new Customer { Id = 1, Name = "Alice" };
binder.Attach(customer);

// Änderungen werden erkannt
customer.Name = "Bob";  // ? Callback wird aufgerufen

// Manuell entbinden
binder.Detach(customer);
```

**Anwendungsfälle:**
- Volle Kontrolle über Binding-Lifecycle
- Spezifische Entitäten überwachen
- Custom Event-Handling-Logik

### 2. DataStore-Modus (Empfohlen)

Automatische Synchronisation mit einem DataStore via `AttachToDataStore()`.

**Verwendung:**
```csharp
using DataToolKit.Storage.Persistence;
using DataToolKit.Storage.DataStores;

var binder = new PropertyChangedBinder<Customer>(
    enabled: true,
    onEntityChanged: customer => 
    {
        Console.WriteLine($"Customer {customer.Id} changed");
        repository.Update(customer);
    });

var store = new InMemoryDataStore<Customer>();

// Automatische Synchronisation aktivieren
using var subscription = binder.AttachToDataStore(store);

// Neue Items werden automatisch gebunden
store.Add(new Customer { Id = 1, Name = "Alice" });
// ? Automatisch an PropertyChanged gebunden

// Entfernte Items werden automatisch entbunden
store.Remove(customer);
// ? Automatisch von PropertyChanged entbunden

// Dispose beendet DataStore-Synchronisation
subscription.Dispose();
```

**Anwendungsfälle:**
- PersistentDataStore (automatische Persistierung)
- ParentChildRelationship (ForeignKey-Tracking)
- Validation-Tracking über Collections
- Audit-Logging für DataStores

## Konstruktor

```csharp
public PropertyChangedBinder(bool enabled, Action<T> onEntityChanged)
```

**Parameter:**
- `enabled` (`bool`): Wenn `false`, werden alle Operationen übersprungen (Performance-Optimierung)
- `onEntityChanged` (`Action<T>`): Callback, der bei PropertyChanged-Events aufgerufen wird

**Exceptions:**
- `ArgumentNullException`: Wenn `onEntityChanged` null ist

## Manueller Modus - Methoden

### Attach

```csharp
public void Attach(T entity)
```

Bindet eine Entität idempotent an PropertyChanged-Events.

**Idempotenz:**
```csharp
var customer = new Customer { Id = 1 };

binder.Attach(customer);
binder.Attach(customer);  // ? Sicher, kein Duplikat
binder.Attach(customer);  // ? Sicher, kein Duplikat

customer.Name = "Changed";
// ? Callback wird nur EINMAL aufgerufen
```

**Implementierung:**
- Event-Handler wird vor dem Anfügen entfernt
- Dann genau einmal angefügt
- Referenz-basiertes Tracking in HashSet

### AttachRange

```csharp
public void AttachRange(IEnumerable<T> entities)
```

Bindet mehrere Entitäten.

**Verwendung:**
```csharp
var customers = new List<Customer>
{
    new Customer { Id = 1, Name = "Alice" },
    new Customer { Id = 2, Name = "Bob" },
    new Customer { Id = 3, Name = "Charlie" }
};

binder.AttachRange(customers);
// Alle drei Customers werden überwacht
```

### Detach

```csharp
public void Detach(T entity)
```

Entfernt die Bindung zu einer Entität.

**Verwendung:**
```csharp
binder.Detach(customer);

customer.Name = "Changed";  // ? Kein Callback mehr
```

### DetachAll

```csharp
public void DetachAll()
```

Entfernt alle Bindings.

**Verwendung:**
```csharp
binder.AttachRange(customers);  // 100 Customers gebunden

binder.DetachAll();  // Alle 100 Bindings entfernt
```

## DataStore-Modus - Methoden

### AttachToDataStore

```csharp
public IDisposable AttachToDataStore(IDataStore<T> dataStore)
```

Bindet den Binder automatisch an einen DataStore.

**Parameter:**
- `dataStore` (`IDataStore<T>`): Der zu überwachende DataStore

**Rückgabewert:**
- `IDisposable`: Subscription-Objekt. Dispose beendet nur die DataStore-Bindung, nicht den Binder selbst.

**Exceptions:**
- `ArgumentNullException`: Wenn `dataStore` null ist

**Automatisches Verhalten:**

| DataStore-Operation | Binder-Reaktion |
|---------------------|----------------|
| **Initiales Attach** | Alle existierenden Items werden gebunden |
| **CollectionChanged.Add** | Neue Items werden gebunden |
| **CollectionChanged.Remove** | Entfernte Items werden entbunden |
| **CollectionChanged.Replace** | Alte Items entbunden, neue Items gebunden |
| **CollectionChanged.Reset** | Alle Items entbunden |

**Verwendungsbeispiel:**
```csharp
var store = new InMemoryDataStore<Customer>();
var binder = new PropertyChangedBinder<Customer>(
    enabled: true,
    onEntityChanged: c => Console.WriteLine($"{c.Name} changed"));

// Existierende Items
store.Add(new Customer { Id = 1, Name = "Alice" });
store.Add(new Customer { Id = 2, Name = "Bob" });

// DataStore-Modus aktivieren
using var subscription = binder.AttachToDataStore(store);
// ? Alice und Bob sind jetzt gebunden

// Neue Items werden automatisch gebunden
store.Add(new Customer { Id = 3, Name = "Charlie" });
// ? Charlie ist jetzt auch gebunden

// Items entfernen
store.Remove(customer1);
// ? Alice ist nicht mehr gebunden

// Dispose beendet Synchronisation
subscription.Dispose();

// Neue Items nach Dispose werden NICHT gebunden
store.Add(new Customer { Id = 4, Name = "Dave" });
// ? Dave ist NICHT gebunden
```

### Dispose

```csharp
public void Dispose()
```

Gibt alle Ressourcen frei:
1. DataStore-Subscription wird disposed (falls aktiv)
2. Alle PropertyChanged-Bindings werden entfernt
3. Tracking-HashSet wird geleert

**Wichtig:**
```csharp
using var binder = new PropertyChangedBinder<Customer>(
    enabled: true,
    onEntityChanged: c => repository.Update(c));

// ... Verwendung ...

// Dispose wird automatisch aufgerufen
```

## Anwendungsbeispiele

### 1. PersistentDataStore (Automatische Persistierung)

```csharp
public class PersistentDataStore<T> : InMemoryDataStore<T>, IDisposable
    where T : class, IEntity
{
    private readonly IRepository<T> _repository;
    private readonly PropertyChangedBinder<T> _binder;

    public PersistentDataStore(
        IRepository<T> repository,
        bool trackPropertyChanges = true)
    {
        _repository = repository;
        
        // PropertyChangedBinder im manuellen Modus
        _binder = new PropertyChangedBinder<T>(
            trackPropertyChanges,
            entity => _repository.Update(entity));
    }

    public override bool Add(T item)
    {
        var added = base.Add(item);
        if (added)
        {
            _binder.Attach(item);  // Manuell binden
            _repository.Update(item);
        }
        return added;
    }

    public override bool Remove(T item)
    {
        var removed = base.Remove(item);
        if (removed)
        {
            _binder.Detach(item);  // Manuell entbinden
            _repository.Delete(item);
        }
        return removed;
    }

    public void Dispose()
    {
        _binder.Dispose();
    }
}
```

### 2. ParentChildRelationship (ForeignKey-Tracking)

```csharp
public class ParentChildRelationship<TParent, TChild> : IDisposable
    where TChild : class
{
    private readonly PropertyChangedBinder<TChild> _binder;
    private readonly InMemoryDataStore<TChild> _childStore;

    public ParentChildRelationship(IDataStoreProvider provider)
    {
        _childStore = new InMemoryDataStore<TChild>();
        
        // Binder im DataStore-Modus für automatische Synchronisation
        _binder = new PropertyChangedBinder<TChild>(
            enabled: true,
            onEntityChanged: OnChildPropertyChanged);
    }

    public IDataStore<TChild>? DataSource
    {
        set
        {
            if (_dataSource != null)
            {
                // Alte Bindings entfernen
                _binder.DetachAll();
            }

            _dataSource = value;

            if (_dataSource != null)
            {
                // Neue Bindings im DataStore-Modus
                _binder.AttachToDataStore(_dataSource);
                SynchronizeChilds();
            }
        }
    }

    private void OnChildPropertyChanged(TChild child)
    {
        // Prüfen, ob Child noch zum Parent gehört
        var shouldInclude = ShouldIncludeChild(child);
        var isIncluded = _childStore.Items.Contains(child);

        if (shouldInclude && !isIncluded)
        {
            _childStore.Add(child);  // Zum Parent hinzufügen
        }
        else if (!shouldInclude && isIncluded)
        {
            _childStore.Remove(child);  // Vom Parent entfernen
        }
    }

    public void Dispose()
    {
        _binder.Dispose();
    }
}
```

### 3. Validation-Tracking

```csharp
public class ValidationManager<T> where T : class
{
    private readonly PropertyChangedBinder<T> _binder;
    private readonly Dictionary<T, List<string>> _errors;

    public ValidationManager()
    {
        _errors = new Dictionary<T, List<string>>();
        _binder = new PropertyChangedBinder<T>(
            enabled: true,
            onEntityChanged: ValidateEntity);
    }

    public void TrackEntities(IDataStore<T> dataStore)
    {
        _binder.AttachToDataStore(dataStore);
    }

    private void ValidateEntity(T entity)
    {
        var errors = new List<string>();

        // Validierungslogik
        if (entity is Customer customer)
        {
            if (string.IsNullOrEmpty(customer.Name))
                errors.Add("Name is required");
            
            if (string.IsNullOrEmpty(customer.Email))
                errors.Add("Email is required");
        }

        _errors[entity] = errors;
        
        // Event auslösen für UI-Update
        ValidationChanged?.Invoke(this, EventArgs.Empty);
    }

    public IReadOnlyList<string> GetErrors(T entity)
    {
        return _errors.TryGetValue(entity, out var errors) 
            ? errors 
            : Array.Empty<string>();
    }

    public event EventHandler? ValidationChanged;
}
```

### 4. Audit-Logging

```csharp
public class AuditLogger<T> where T : class
{
    private readonly PropertyChangedBinder<T> _binder;
    private readonly List<AuditEntry> _log;

    public AuditLogger()
    {
        _log = new List<AuditEntry>();
        _binder = new PropertyChangedBinder<T>(
            enabled: true,
            onEntityChanged: LogChange);
    }

    public void TrackDataStore(IDataStore<T> dataStore)
    {
        using var subscription = _binder.AttachToDataStore(dataStore);
        // Synchronisation läuft automatisch
    }

    private void LogChange(T entity)
    {
        _log.Add(new AuditEntry
        {
            Timestamp = DateTime.UtcNow,
            Entity = entity,
            EntityType = typeof(T).Name,
            Operation = "PropertyChanged"
        });
    }

    public IReadOnlyList<AuditEntry> GetLog() => _log.AsReadOnly();
}

public class AuditEntry
{
    public DateTime Timestamp { get; set; }
    public object? Entity { get; set; }
    public string EntityType { get; set; } = "";
    public string Operation { get; set; } = "";
}
```

## Performance-Hinweise

### Optimiert für:
- ? Kleine bis mittlere Datenmengen (< 1000 Entitäten)
- ? Moderate Änderungshäufigkeit
- ? UI-Szenarien mit PropertyChanged-Tracking

### Nicht optimiert für:
- ? Sehr große Collections (> 10.000 Items)
- ? Hochfrequente Property-Änderungen (> 100/Sekunde)
- ? Entitäten ohne INotifyPropertyChanged

### Performance-Tipps:

1. **Disabled-Modus nutzen:**
```csharp
// Bei großen Batch-Operationen temporär deaktivieren
var binder = new PropertyChangedBinder<Customer>(
    enabled: false,  // ? Kein Overhead
    onEntityChanged: c => { });
```

2. **Dispose nicht vergessen:**
```csharp
using var binder = new PropertyChangedBinder<Customer>(...);
// Automatisches Cleanup
```

3. **INotifyPropertyChanged implementieren:**
```csharp
// ? EntityBase implementiert INotifyPropertyChanged via Fody
public class Customer : EntityBase
{
    public string Name { get; set; }  // Automatisch mit PropertyChanged
}

// ? Ohne INotifyPropertyChanged funktioniert es nicht
public class SimpleCustomer
{
    public string Name { get; set; }  // Keine Events
}
```

## Fehlerbehandlung

### Häufige Fehler

#### 1. Entität implementiert kein INotifyPropertyChanged

```csharp
public class Customer  // ? Kein INotifyPropertyChanged
{
    public string Name { get; set; }
}

var binder = new PropertyChangedBinder<Customer>(...);
binder.Attach(customer);

customer.Name = "Changed";  // ? Kein Callback, keine Events
```

**Lösung:** EntityBase vererben oder INotifyPropertyChanged implementieren:
```csharp
public class Customer : EntityBase  // ? Hat INotifyPropertyChanged
{
    public string Name { get; set; }
}
```

#### 2. DataStore-Subscription nicht disposed

```csharp
var subscription = binder.AttachToDataStore(store);
// ? Vergessen: subscription.Dispose()
// ? Memory Leak: Event-Handler bleiben registriert
```

**Lösung:** Using-Pattern verwenden:
```csharp
using var subscription = binder.AttachToDataStore(store);
// ? Automatisches Dispose
```

#### 3. Callback wirft Exception

```csharp
var binder = new PropertyChangedBinder<Customer>(
    enabled: true,
    onEntityChanged: c => 
    {
        throw new Exception("Oops");  // ? Exception im Callback
    });

customer.Name = "Changed";  // ? Exception wird propagiert
```

**Lösung:** Try-Catch im Callback:
```csharp
var binder = new PropertyChangedBinder<Customer>(
    enabled: true,
    onEntityChanged: c => 
    {
        try
        {
            repository.Update(c);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update customer");
        }
    });
```

## Verwandte Komponenten

- [PersistentDataStore](API-Referenz.md#persistentdatastoret) - Nutzt PropertyChangedBinder für automatische Persistierung
- [ParentChildRelationship](ParentChildRelationship.md) - Nutzt PropertyChangedBinder im DataStore-Modus
- [EntityBase](API-Referenz.md#entitybase) - Implementiert INotifyPropertyChanged

## Best Practices

### ? Do's

1. **Using-Pattern für Subscriptions**
   ```csharp
   using var subscription = binder.AttachToDataStore(store);
   ```

2. **EntityBase vererben**
   ```csharp
   public class Customer : EntityBase  // ? PropertyChanged automatisch
   ```

3. **Try-Catch im Callback**
   ```csharp
   onEntityChanged: entity => 
   {
       try { repository.Update(entity); }
       catch { /* Log error */ }
   }
   ```

4. **DataStore-Modus bevorzugen**
   ```csharp
   using var sub = binder.AttachToDataStore(store);  // ? Automatisch
   // Statt manuell Attach/Detach
   ```

### ? Don'ts

1. **Kein Dispose**
2. **Keine INotifyPropertyChanged-Implementierung**
3. **Exception im Callback werfen ohne Handling**
4. **Sehr große Collections ohne Disabled-Modus**

## Siehe auch

- [API-Referenz](API-Referenz.md) - Vollständige API-Dokumentation
- [PersistentDataStore](API-Referenz.md#persistentdatastoret) - Verwendung im Kontext
- [ParentChildRelationship](ParentChildRelationship.md) - DataStore-Modus Beispiel
