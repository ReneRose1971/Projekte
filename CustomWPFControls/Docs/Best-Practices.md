# Best Practices - CustomWPFControls

Bewährte Praktiken, Tipps und Tricks für die Verwendung von CustomWPFControls.

## ?? Inhaltsverzeichnis

- [ViewModel-Design](#viewmodel-design)
- [DataStore-Integration](#datastore-integration)
- [Commands](#commands)
- [Performance](#performance)
- [Testing](#testing)
- [Common Pitfalls](#common-pitfalls)

---

## ?? ViewModel-Design

### **? DO: Properties von Model delegieren**

```csharp
public class CustomerViewModel : ViewModelBase<Customer>
{
    // ? RICHTIG: Read-only, delegiert an Model
    public string Name => Model.Name;
    public string Email => Model.Email;
    
    // ? UI-spezifische Properties
    public bool IsSelected { get; set; }
}
```

### **? DON'T: Model-Properties duplizieren**

```csharp
// ? FALSCH: Duplikation mit Backing-Field
private string _name;
public string Name
{
    get => _name;
    set { _name = value; OnPropertyChanged(); }
}
```

**Warum?**
- Redundanz ? Fehlerquelle
- Synchronisation nötig ? Komplexität
- Model ist Source of Truth

---

### **? DO: Computed Properties für UI-Logik**

```csharp
public class CustomerViewModel : ViewModelBase<Customer>
{
    public string Name => Model.Name;
    public string Email => Model.Email;
    
    // ? Computed Property für Display
    public string DisplayName => $"{Name} ({Email})";
    public string ShortName => Name.Length > 20 
        ? Name.Substring(0, 20) + "..." 
        : Name;
}
```

---

### **? DO: ViewModelBase als Basis verwenden**

```csharp
// ? RICHTIG: Von ViewModelBase<T> ableiten
public class CustomerViewModel : ViewModelBase<Customer>
{
    public CustomerViewModel(Customer model) : base(model) { }
}

// ? FALSCH: INotifyPropertyChanged manuell
public class CustomerViewModel : INotifyPropertyChanged
{
    private Customer _model;
    // ... manuelles PropertyChanged-Handling
}
```

---

## ??? DataStore-Integration

### **? DO: IEqualityComparer implementieren**

```csharp
public class CustomerComparer : IEqualityComparer<Customer>
{
    public bool Equals(Customer? x, Customer? y)
    {
        if (x == null || y == null) return false;
        return x.Id == y.Id; // ? Stabile Property (Id)
    }

    public int GetHashCode(Customer obj)
    {
        return obj.Id.GetHashCode();
    }
}

// DI-Registrierung
services.AddSingleton<IEqualityComparer<Customer>>(new CustomerComparer());
```

**Warum Id verwenden?**
- Id ist unveränderlich nach DB-Insert
- HashCode bleibt stabil
- Dictionary-Lookups funktionieren zuverlässig

---

### **? DON'T: Mutable Properties in Comparer**

```csharp
// ? FALSCH: Name kann sich ändern!
public bool Equals(Customer? x, Customer? y)
    => x?.Name == y?.Name;

public int GetHashCode(Customer obj)
    => obj.Name?.GetHashCode() ?? 0;
```

**Problem:**
```csharp
var customer = new Customer { Id = 1, Name = "Alice" };
var vm = factory.Create(customer);
dict[vm] = "Value";

customer.Name = "Bob"; // ? Name ändert sich!
// ? dict[vm] findet nichts mehr! ? HashCode hat sich geändert
```

---

### **? DO: DataStore via DI injizieren**

```csharp
public class ViewModelModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // ? Singleton für shared state
        services.AddSingleton<IDataStore<Customer>>(provider =>
        {
            var comparer = provider.GetRequiredService<IEqualityComparer<Customer>>();
            return new InMemoryDataStore<Customer>(comparer);
        });
    }
}
```

---

## ?? Commands

### **? DO: CreateModel für neue Objekte**

```csharp
var viewModel = new EditableCollectionViewModel<Customer, CustomerViewModel>(
    dataStore, factory, comparer);

viewModel.CreateModel = () => new Customer
{
    Name = "New Customer",
    Email = "new@example.com",
    CreatedAt = DateTime.Now
};
```

---

### **? DO: EditModel für Bearbeitung**

```csharp
viewModel.EditModel = customer =>
{
    var dialog = new CustomerEditDialog
    {
        DataContext = new CustomerEditViewModel(customer)
    };
    
    if (dialog.ShowDialog() == true)
    {
        // Model wurde bearbeitet
        // DataStore-PropertyChanged-Tracking aktualisiert automatisch
    }
};
```

---

### **? DO: Commands via Binding verwenden**

```xml
<Button Content="Add" 
        Command="{Binding AddCommand}"
        ToolTip="Add new customer"/>

<Button Content="Delete" 
        Command="{Binding DeleteCommand}"
        ToolTip="Delete selected customer"
        IsEnabled="{Binding DeleteCommand.CanExecute}"/>
```

---

### **? DON'T: Commands manuell aufrufen**

```csharp
// ? FALSCH: Manueller Aufruf
private void AddButton_Click(object sender, RoutedEventArgs e)
{
    viewModel.AddCommand.Execute(null);
}
```

**Besser:**
```xml
<!-- ? RICHTIG: XAML-Binding -->
<Button Content="Add" Command="{Binding AddCommand}"/>
```

---

## ? Performance

### **? DO: Virtualization für große Listen**

```xml
<ListBox ItemsSource="{Binding Items}"
         VirtualizingPanel.IsVirtualizing="True"
         VirtualizingPanel.VirtualizationMode="Recycling"
         VirtualizingPanel.CacheLength="20"
         VirtualizingPanel.CacheLengthUnit="Item">
    <!-- ItemTemplate -->
</ListBox>
```

**Vorteile:**
- Nur sichtbare Items werden gerendert
- Recycling reduziert Memory-Footprint
- Performance bleibt konstant bei 10.000+ Items

---

### **? DO: ICollectionView für Filtering**

```csharp
var view = CollectionViewSource.GetDefaultView(viewModel.Items);
view.Filter = item =>
{
    var vm = (CustomerViewModel)item;
    return vm.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase);
};

// Filter aktualisieren
searchTextBox.TextChanged += (s, e) =>
{
    view.Refresh();
};
```

**Vorteile:**
- Kein Re-Rendering der gesamten Liste
- Filter läuft auf UI-Thread (smooth)
- DataStore bleibt unberührt

---

### **? DON'T: LINQ in Binding**

```xml
<!-- ? FALSCH: LINQ in Binding -->
<ListBox ItemsSource="{Binding Items.Where(x => x.IsActive)}"/>
```

**Problem:**
- Wird bei jedem PropertyChanged neu evaluiert
- Performance-Killer bei großen Listen

**Besser:**
```csharp
// ? RICHTIG: Separate Property
public ObservableCollection<CustomerViewModel> ActiveItems 
    => new(Items.Where(x => x.IsActive));
```

---

### **? DO: Lazy Loading bei Bedarf**

```csharp
public class LazyCustomerViewModel : ViewModelBase<Customer>
{
    private ObservableCollection<OrderViewModel>? _orders;
    
    public ObservableCollection<OrderViewModel> Orders
    {
        get
        {
            // ? Lazy Load: Nur wenn benötigt
            if (_orders == null)
            {
                _orders = new ObservableCollection<OrderViewModel>(
                    orderRepository.GetByCustomerId(Model.Id)
                        .Select(o => new OrderViewModel(o)));
            }
            return _orders;
        }
    }
}
```

---

## ?? Testing

### **? DO: Unit-Tests für ViewModels**

```csharp
[Fact]
public void CustomerViewModel_Name_ReturnsModelName()
{
    // Arrange
    var customer = new Customer { Name = "Alice" };
    var viewModel = new CustomerViewModel(customer);

    // Act & Assert
    Assert.Equal("Alice", viewModel.Name);
}
```

---

### **? DO: Integration-Tests für Synchronisation**

```csharp
[Fact]
public void CollectionViewModel_DataStoreAdd_CreatesViewModel()
{
    // Arrange
    var dataStore = new InMemoryDataStore<Customer>();
    var viewModel = new CollectionViewModel<Customer, CustomerViewModel>(
        dataStore, factory, comparer);
    
    // Act
    dataStore.Add(new Customer { Name = "Bob" });
    
    // Assert
    Assert.Equal(1, viewModel.Count);
    Assert.Equal("Bob", viewModel.Items[0].Name);
}
```

---

### **? DO: Behavior-Tests für Commands**

```csharp
[Fact]
public void AddCommand_WithCreateModel_AddsToDataStore()
{
    // Arrange
    var viewModel = new EditableCollectionViewModel<Customer, CustomerViewModel>(
        dataStore, factory, comparer);
    viewModel.CreateModel = () => new Customer { Name = "New" };
    
    // Act
    viewModel.AddCommand.Execute(null);
    
    // Assert
    Assert.Equal(1, viewModel.Count);
}
```

---

## ?? Common Pitfalls

### **1. ViewModel ohne Model-Property**

```csharp
// ? FALSCH: Kein Model-Property
public class CustomerViewModel : INotifyPropertyChanged
{
    private Customer _customer;
    // ... kein public Model-Property
}
```

**Problem:**
- `IViewModelWrapper<T>` nicht implementiert
- CollectionViewModel kann Model nicht extrahieren
- Compiler-Fehler: Constraint nicht erfüllt

**Lösung:**
```csharp
// ? RICHTIG: Von ViewModelBase ableiten
public class CustomerViewModel : ViewModelBase<Customer>
{
    public CustomerViewModel(Customer model) : base(model) { }
    // Model-Property von ViewModelBase geerbt
}
```

---

### **2. Vergessene IEqualityComparer-Registrierung**

```csharp
// ? FALSCH: Kein Comparer registriert
services.AddSingleton<IDataStore<Customer>>(
    provider => new InMemoryDataStore<Customer>());
```

**Problem:**
- DataStore verwendet `EqualityComparer<Customer>.Default`
- Duplikate werden nicht erkannt
- Synchronisation funktioniert nicht richtig

**Lösung:**
```csharp
// ? RICHTIG: Comparer explizit registrieren
services.AddSingleton<IEqualityComparer<Customer>>(
    new FallbackEqualsComparer<Customer>());

services.AddSingleton<IDataStore<Customer>>(provider =>
{
    var comparer = provider.GetRequiredService<IEqualityComparer<Customer>>();
    return new InMemoryDataStore<Customer>(comparer);
});
```

---

### **3. SelectedItem-Binding ohne Mode=TwoWay**

```xml
<!-- ? FALSCH: OneWay-Binding (Default) -->
<ListBox SelectedItem="{Binding SelectedItem}"/>
```

**Problem:**
- User-Selection wird nicht an ViewModel propagiert
- DeleteCommand.CanExecute() bleibt false
- Keine Interaktion möglich

**Lösung:**
```xml
<!-- ? RICHTIG: TwoWay-Binding -->
<ListBox SelectedItem="{Binding SelectedItem, Mode=TwoWay}"/>
```

---

### **4. Memory-Leaks durch nicht-disposed ViewModels**

```csharp
// ? FALSCH: DataStore wird nicht disposed
public class MainWindow : Window
{
    private CollectionViewModel<Customer, CustomerViewModel> _viewModel;
    
    public MainWindow(CollectionViewModel<Customer, CustomerViewModel> viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
        // ? Kein Dispose beim Schließen!
    }
}
```

**Problem:**
- Event-Handler bleiben subscribed
- Dictionary behält Referenzen
- Memory-Leak!

**Lösung:**
```csharp
// ? RICHTIG: Dispose beim Schließen
public class MainWindow : Window
{
    private CollectionViewModel<Customer, CustomerViewModel> _viewModel;
    
    public MainWindow(CollectionViewModel<Customer, CustomerViewModel> viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
        
        Closed += (s, e) => _viewModel.Dispose(); // ? Dispose
    }
}
```

---

### **5. Fody PropertyChanged nicht konfiguriert**

```xml
<!-- ? FALSCH: Kein FodyWeavers.xml -->
<!-- Datei fehlt im Projekt-Root -->
```

**Problem:**
- PropertyChanged wird nicht geweaved
- UI aktualisiert nicht
- Bindings funktionieren nicht

**Lösung:**
```xml
<!-- ? RICHTIG: FodyWeavers.xml erstellen -->
<?xml version="1.0" encoding="utf-8"?>
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <PropertyChanged FilterType="Explicit" InjectOnPropertyNameChanged="false" />
</Weavers>
```

---

## ?? Quick Reference

### **ViewModel-Checklist**

- ? Von `ViewModelBase<TModel>` ableiten
- ? Constructor mit `TModel` Parameter
- ? Domain-Properties read-only (delegiert an Model)
- ? UI-Properties mit Auto-Property
- ? Computed Properties für UI-Logik

### **DI-Registrierung-Checklist**

- ? IEqualityComparer registriert
- ? IDataStore registriert
- ? ViewModelFactory registriert (via AddViewModelFactory)
- ? CollectionViewModel registriert

### **Performance-Checklist**

- ? Virtualization aktiviert (bei > 100 Items)
- ? ICollectionView für Filtering
- ? Lazy Loading für Child-Collections
- ? Avoid LINQ in Bindings

---

## ?? See Also

- [Getting Started](Getting-Started.md) - Schnellstart
- [Architecture](Architecture.md) - Architektur-Übersicht
- [API Reference](API-Reference.md) - Vollständige API
