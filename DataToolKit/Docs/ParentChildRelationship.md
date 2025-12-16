# ParentChildRelationship

## Übersicht

`ParentChildRelationship<TParent, TChild>` verwaltet automatisch 1:n-Beziehungen zwischen einem Parent-Objekt und einer gefilterten Collection von Child-Elementen mit vollständiger PropertyChanged- und CollectionChanged-Synchronisation.

## Kernmerkmale

- ? **Automatische Filterung** basierend auf konfigurierbarer Filter-Funktion
- ? **PropertyChanged-Tracking** für dynamische Updates bei ForeignKey-Änderungen
- ? **CollectionChanged-Synchronisation** bei Add/Remove/Clear in DataSource
- ? **Lazy DataSource Initialization** via IDataStoreProvider
- ? **Immutable Parent** (einmalig setzbar, danach unveränderlich)
- ? **Memory-effizient** (keine Datenduplikation)
- ? **UI-freundlich** (ObservableCollection-basiert)

## Architektur

```
DataSource (alle Orders)
    ?
    ?? CollectionChanged überwacht ? Add/Remove/Clear
    ?? PropertyChanged auf ALLEN Items ? ForeignKey-Änderungen
        ?
    Filter: (parent, child) => child.CustomerId == parent.Id
        ?
Childs (nur gefilterte Orders für selectedCustomer)
```

## Verwendung

### Basis-Setup

```csharp
using DataToolKit.Relationships;
using DataToolKit.Abstractions.DataStores;

public class CustomerViewModel
{
    private readonly ParentChildRelationship<Customer, Order> _orderRelationship;

    public CustomerViewModel(IDataStoreProvider provider)
    {
        // Relationship erstellen
        _orderRelationship = new ParentChildRelationship<Customer, Order>(provider)
        {
            Parent = selectedCustomer,
            IsChildFilter = (customer, order) => order.CustomerId == customer.Id
        };
    }

    // UI bindet an diese Collection
    public ReadOnlyObservableCollection<Order> Orders 
        => _orderRelationship.Childs.Items;
}
```

### Explizite DataSource-Zuweisung

```csharp
// Explizite DataSource statt Lazy Loading
var customOrderStore = new InMemoryDataStore<Order>();
var relationship = new ParentChildRelationship<Customer, Order>(provider)
{
    DataSource = customOrderStore,  // ? Explizit setzen
    Parent = customer,
    IsChildFilter = (c, o) => o.CustomerId == c.Id
};
```

### Komplexe Filter

```csharp
// Mehrere Bedingungen kombinieren
relationship.IsChildFilter = (project, task) =>
    task.ProjectId == project.Id &&
    task.Status != TaskStatus.Archived &&
    task.AssignedTo != null &&
    task.DueDate >= DateTime.Today;
```

### Filter-Änderung zur Laufzeit

```csharp
// Ursprünglicher Filter: Alle Orders
relationship.IsChildFilter = (c, o) => o.CustomerId == c.Id;

// Filter ändern: Nur offene Orders
relationship.IsChildFilter = (c, o) => 
    o.CustomerId == c.Id && o.Status == OrderStatus.Open;
// ? Childs wird automatisch neu synchronisiert
```

## Automatische Synchronisation

### 1. CollectionChanged-Events

```csharp
// Neue Order in DataSource hinzufügen
orderStore.Add(new Order 
{ 
    Id = 42, 
    CustomerId = customer.Id  // Matched Filter
});
// ? Wird automatisch zu relationship.Childs hinzugefügt
```

### 2. PropertyChanged-Events

```csharp
// Order hat CustomerId = 1 (matched)
var order = orderStore.Items.First();

// ForeignKey ändern
order.CustomerId = 2;  // ? PropertyChanged wird gefeuert
// ? Order wird automatisch aus Childs entfernt (Filter nicht mehr erfüllt)
```

### 3. Remove-Operations

```csharp
// Order aus DataSource entfernen
orderStore.Remove(order);
// ? Wird automatisch aus relationship.Childs entfernt
```

### 4. Clear-Operations

```csharp
// DataSource komplett leeren
orderStore.Clear();
// ? relationship.Childs wird automatisch geleert
```

## Anwendungsbeispiele

### 1. Master-Detail-View (Customer ? Orders)

```csharp
public class CustomerDetailViewModel : IDisposable
{
    private readonly ParentChildRelationship<Customer, Order> _orders;
    
    public CustomerDetailViewModel(
        IDataStoreProvider provider, 
        Customer customer)
    {
        _orders = new ParentChildRelationship<Customer, Order>(provider)
        {
            Parent = customer,
            IsChildFilter = (c, o) => o.CustomerId == c.Id
        };
    }

    public ReadOnlyObservableCollection<Order> Orders => _orders.Childs.Items;

    public void Dispose() => _orders.Dispose();
}
```

### 2. Hierarchische Navigation (Folder ? Files)

```csharp
public class FolderViewModel
{
    private readonly ParentChildRelationship<Folder, File> _files;
    
    public FolderViewModel(IDataStoreProvider provider, Folder folder)
    {
        _files = new ParentChildRelationship<Folder, File>(provider)
        {
            Parent = folder,
            IsChildFilter = (folder, file) => file.ParentFolderId == folder.Id
        };
    }

    public ReadOnlyObservableCollection<File> Files => _files.Childs.Items;
}
```

### 3. Zeit-basierte Filterung

```csharp
public class TimelineViewModel
{
    private readonly ParentChildRelationship<TimeRange, Event> _events;
    
    public TimelineViewModel(IDataStoreProvider provider, TimeRange range)
    {
        _events = new ParentChildRelationship<TimeRange, Event>(provider)
        {
            Parent = range,
            IsChildFilter = (range, evt) =>
                evt.Timestamp >= range.Start &&
                evt.Timestamp <= range.End
        };
    }

    public ReadOnlyObservableCollection<Event> Events => _events.Childs.Items;
}
```

### 4. Tag/Label-System

```csharp
public class TagViewModel
{
    private readonly ParentChildRelationship<Tag, Document> _documents;
    
    public TagViewModel(IDataStoreProvider provider, Tag tag)
    {
        _documents = new ParentChildRelationship<Tag, Document>(provider)
        {
            Parent = tag,
            IsChildFilter = (tag, doc) => doc.Tags.Contains(tag.Name)
        };
    }

    public ReadOnlyObservableCollection<Document> Documents => _documents.Childs.Items;
}
```

## Properties

### Parent

```csharp
public TParent? Parent { get; set; }
```

**Einschränkungen:**
- ? Kann **nur einmal** gesetzt werden (immutable)
- ? Kann auf `null` gesetzt werden
- ? Kann **nicht** erneut gesetzt werden nach erstem Set

```csharp
// ? Erlaubt
relationship.Parent = customer1;

// ? Exception: InvalidOperationException
relationship.Parent = customer2;  
// "Das Parent-Objekt kann nur einmal gesetzt werden..."
```

**Workaround für Parent-Wechsel:**
```csharp
// Alte Relationship disposen
oldRelationship.Dispose();

// Neue Relationship mit neuem Parent erstellen
var newRelationship = new ParentChildRelationship<Customer, Order>(provider)
{
    Parent = newCustomer,
    IsChildFilter = (c, o) => o.CustomerId == c.Id
};
```

### IsChildFilter

```csharp
public Func<TParent, TChild, bool>? IsChildFilter { get; set; }
```

**Merkmale:**
- ? Kann **mehrfach** geändert werden
- ? Triggert **automatische Re-Synchronisation** bei Änderung
- ? Kann auf `null` gesetzt werden (? leere Childs)

```csharp
// Initial filter
relationship.IsChildFilter = (c, o) => o.CustomerId == c.Id;

// Filter ändern (triggert Re-Sync)
relationship.IsChildFilter = (c, o) => 
    o.CustomerId == c.Id && o.Status == OrderStatus.Open;
```

### DataSource

```csharp
public IDataStore<TChild>? DataSource { get; set; }
```

**Lazy Initialization:**
```csharp
// Beim ersten Zugriff wird automatisch geladen
var dataSource = relationship.DataSource;  
// ? Ruft provider.GetDataStore<TChild>() auf
```

**Explizite Zuweisung:**
```csharp
// Eigene DataSource setzen
relationship.DataSource = myCustomStore;
// ? Alte Subscriptions werden entfernt
// ? Neue Subscriptions werden erstellt
// ? Childs wird neu synchronisiert
```

**Auf null setzen:**
```csharp
relationship.DataSource = null;
// ? Alle Subscriptions werden entfernt
// ? Childs wird geleert
```

### Childs

```csharp
public InMemoryDataStore<TChild> Childs { get; }
```

**Schreibgeschützte Collection der gefilterten Child-Elemente:**
```csharp
// UI-Binding
DataGrid.ItemsSource = relationship.Childs.Items;

// Anzahl
int count = relationship.Childs.Count;

// Iteration
foreach (var child in relationship.Childs.Items)
{
    // ...
}
```

## Dispose

```csharp
relationship.Dispose();
```

**Beim Dispose wird:**
1. ? CollectionChanged-Event von DataSource abgemeldet
2. ? PropertyChangedBinder disposed (alle Bindings entfernt)
3. ? Childs-Collection geleert

**Wichtig:**
```csharp
// ? Dispose aufrufen, wenn nicht mehr benötigt
using var relationship = new ParentChildRelationship<Customer, Order>(provider);
// ...

// Oder explizit:
try
{
    var rel = new ParentChildRelationship<Customer, Order>(provider);
    // ...
}
finally
{
    rel?.Dispose();
}
```

## Performance-Hinweise

### Optimiert für:
- ? Kleine bis mittlere Datenmengen (< 100 Child-Items pro Parent)
- ? Moderate Änderungshäufigkeit
- ? UI-Szenarien mit ObservableCollection-Binding

### Nicht optimiert für:
- ? Sehr große Collections (> 1000 Items)
- ? Hochfrequente Property-Änderungen
- ? Komplexe Filterbedingungen mit aufwändigen Berechnungen

### Performance-Tipps:

1. **Lazy Loading nutzen:**
```csharp
// ? DataSource wird nur geladen, wenn benötigt
var relationship = new ParentChildRelationship<Customer, Order>(provider);
// DataSource wird erst geladen bei Parent/IsChildFilter-Set
```

2. **Filter-Komplexität minimieren:**
```csharp
// ? Einfacher Vergleich
IsChildFilter = (c, o) => o.CustomerId == c.Id;

// ?? Komplexe Berechnung (langsamer)
IsChildFilter = (c, o) => 
    CalculateDistance(c.Location, o.DeliveryLocation) < 100;
```

3. **INotifyPropertyChanged implementieren:**
```csharp
// ? Für PropertyChanged-Tracking erforderlich
public class Order : EntityBase  // EntityBase hat INotifyPropertyChanged
{
    public int? CustomerId { get; set; }  // Wird überwacht
}
```

## Fehlerbehandlung

### Häufige Fehler

#### 1. Parent zweimal setzen

```csharp
relationship.Parent = customer1;
relationship.Parent = customer2;  // ? InvalidOperationException
```

**Lösung:** Neue Relationship-Instanz erstellen

#### 2. DataSource nicht registriert

```csharp
// Provider hat keinen Store für Order registriert
var relationship = new ParentChildRelationship<Customer, Order>(provider)
{
    Parent = customer,
    IsChildFilter = (c, o) => o.CustomerId == c.Id
};
// ? DataSource bleibt null, Childs bleibt leer
```

**Lösung:** Store vorher registrieren:
```csharp
var orderStore = provider.GetInMemory<Order>(isSingleton: true);
```

#### 3. Items ohne INotifyPropertyChanged

```csharp
public class SimpleOrder  // ? Kein INotifyPropertyChanged
{
    public int? CustomerId { get; set; }
}
```

**Problem:** PropertyChanged-Events werden nicht gefeuert  
**Lösung:** EntityBase vererben oder INotifyPropertyChanged implementieren

## Verwandte Komponenten

- [PropertyChangedBinder](PropertyChangedBinder.md) - Intern für PropertyChanged-Tracking
- [DataStoreSyncExtensions](API-Referenz.md#datastoresyncextensions) - Collection-Synchronisation
- [IDataStoreProvider](API-Referenz.md#idatastoreprovider) - DataStore-Management

## Best Practices

### ? Do's

1. **Dispose aufrufen**
   ```csharp
   using var relationship = new ParentChildRelationship<Customer, Order>(provider);
   ```

2. **EntityBase vererben**
   ```csharp
   public class Order : EntityBase  // ? PropertyChanged automatisch
   ```

3. **Filter einfach halten**
   ```csharp
   IsChildFilter = (c, o) => o.CustomerId == c.Id;  // ? Einfach
   ```

4. **DataSource registrieren**
   ```csharp
   var orderStore = provider.GetInMemory<Order>(isSingleton: true);
   ```

### ? Don'ts

1. **Parent nicht mehrfach setzen**
2. **Keine komplexen Filter**
3. **Kein Dispose vergessen**
4. **Keine sehr großen Collections**

## Siehe auch

- [API-Referenz](API-Referenz.md) - Vollständige API-Dokumentation
- [DataStore Provider](DataStore-Provider.md) - DataStore-Management
- [Repositories](Repositories.md) - Persistierung
