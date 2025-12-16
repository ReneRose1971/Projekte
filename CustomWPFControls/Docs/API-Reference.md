# API Reference - CustomWPFControls

Vollständige API-Dokumentation aller öffentlichen Typen, Methoden und Properties.

## ?? Namespaces

- [CustomWPFControls.ViewModels](#customwpfcontrolsviewmodels)
- [CustomWPFControls.Factories](#customwpfcontrolsfactories)

---

## CustomWPFControls.ViewModels

### `IViewModelWrapper<TModel>`

Interface für ViewModels, die ein Domain-Model wrappen.

```csharp
public interface IViewModelWrapper<out TModel> where TModel : class
{
    TModel Model { get; }
}
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| **Model** | `TModel` | Das gewrappte Domain-Model (read-only) |

---

### `ViewModelBase<TModel>`

Basisklasse für alle ViewModels mit automatischem PropertyChanged.

```csharp
[AddINotifyPropertyChangedInterface]
public abstract class ViewModelBase<TModel> : IViewModelWrapper<TModel>, INotifyPropertyChanged
    where TModel : class
{
    public TModel Model { get; }
    protected ViewModelBase(TModel model);
    
    public override int GetHashCode();
    public override bool Equals(object? obj);
    public override string ToString();
    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null);
}
```

#### Constructor

```csharp
protected ViewModelBase(TModel model)
```

**Parameter:**
- `model` - Das zu wrappende Domain-Model

**Exceptions:**
- `ArgumentNullException` - Wenn `model` null ist

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| **Model** | `TModel` | Das gewrappte Domain-Model |

#### Methods

| Method | Return | Description |
|--------|--------|-------------|
| **GetHashCode()** | `int` | Basiert auf Model-Referenz (RuntimeHelpers) |
| **Equals(obj)** | `bool` | Prüft auf gleiche Model-Referenz (ReferenceEquals) |
| **ToString()** | `string` | Delegiert an Model.ToString() |
| **OnPropertyChanged(propertyName)** | `void` | Feuert PropertyChanged-Event |

#### Events

| Event | Type | Description |
|-------|------|-------------|
| **PropertyChanged** | `PropertyChangedEventHandler` | INotifyPropertyChanged-Event |

#### Example

```csharp
public class CustomerViewModel : ViewModelBase<Customer>
{
    public CustomerViewModel(Customer model) : base(model) { }
    
    // Domain-Properties
    public string Name => Model.Name;
    
    // UI-Properties (mit PropertyChanged)
    public bool IsSelected { get; set; }
}
```

---

### `CollectionViewModel<TModel, TViewModel>`

Collection-ViewModel mit bidirektionaler DataStore-Synchronisation.

```csharp
public class CollectionViewModel<TModel, TViewModel> : INotifyPropertyChanged, IDisposable
    where TModel : class
    where TViewModel : class, IViewModelWrapper<TModel>
{
    public CollectionViewModel(
        IDataStore<TModel> dataStore,
        IViewModelFactory<TModel, TViewModel> viewModelFactory,
        IEqualityComparer<TModel> modelComparer);
    
    public ReadOnlyObservableCollection<TViewModel> Items { get; }
    public TViewModel? SelectedItem { get; set; }
    public int Count { get; }
    
    public bool AddModel(TModel model);
    public bool RemoveModel(TModel model);
    public bool RemoveViewModel(TViewModel viewModel);
    public void Clear();
    public void Dispose();
    
    protected virtual void OnViewModelRemoving(TViewModel viewModel);
    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null);
}
```

#### Constructor

```csharp
public CollectionViewModel(
    IDataStore<TModel> dataStore,
    IViewModelFactory<TModel, TViewModel> viewModelFactory,
    IEqualityComparer<TModel> modelComparer)
```

**Parameter:**
- `dataStore` - DataStore für Models
- `viewModelFactory` - Factory zur Erstellung von ViewModels
- `modelComparer` - Comparer zum Vergleich von Models

**Exceptions:**
- `ArgumentNullException` - Wenn einer der Parameter null ist

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| **Items** | `ReadOnlyObservableCollection<TViewModel>` | Schreibgeschützte ViewModels-Collection |
| **SelectedItem** | `TViewModel?` | Aktuell ausgewähltes ViewModel |
| **Count** | `int` | Anzahl der ViewModels |

#### Methods

| Method | Return | Description |
|--------|--------|-------------|
| **AddModel(model)** | `bool` | Fügt Model hinzu (erstellt ViewModel). Returns true wenn hinzugefügt |
| **RemoveModel(model)** | `bool` | Entfernt Model (disposed ViewModel). Returns true wenn entfernt |
| **RemoveViewModel(viewModel)** | `bool` | Entfernt ViewModel (entfernt Model). Returns true wenn entfernt |
| **Clear()** | `void` | Leert Collection (disposed alle ViewModels) |
| **Dispose()** | `void` | Gibt Ressourcen frei, unsubscribed Events |
| **OnViewModelRemoving(viewModel)** | `void` | Hook vor Dispose (protected virtual) |
| **OnPropertyChanged(propertyName)** | `void` | Feuert PropertyChanged-Event |

#### Events

| Event | Type | Description |
|-------|------|-------------|
| **PropertyChanged** | `PropertyChangedEventHandler` | INotifyPropertyChanged-Event |

#### Behavior

**Bidirektionale Synchronisation:**

| Aktion | Ergebnis |
|--------|----------|
| `dataStore.Add(model)` | ? ViewModel erstellt ? `Items` aktualisiert |
| `dataStore.Remove(model)` | ? ViewModel disposed ? `Items` aktualisiert |
| `AddModel(model)` | ? DataStore.Add() ? ViewModel erstellt |
| `RemoveViewModel(vm)` | ? DataStore.Remove() ? ViewModel disposed |

**SelectedItem Auto-Nullsetzung:**
- Bei Remove: SelectedItem = null wenn entferntes Item ausgewählt war
- Bei Clear: SelectedItem = null immer

#### Example

```csharp
var viewModel = new CollectionViewModel<Customer, CustomerViewModel>(
    dataStore, factory, comparer);

viewModel.AddModel(new Customer { Name = "Alice" });
Console.WriteLine(viewModel.Count); // 1

viewModel.SelectedItem = viewModel.Items.First();
viewModel.RemoveViewModel(viewModel.SelectedItem);
Console.WriteLine(viewModel.SelectedItem); // null
```

---

### `EditableCollectionViewModel<TModel, TViewModel>`

Erweitert CollectionViewModel um Commands.

```csharp
public class EditableCollectionViewModel<TModel, TViewModel> : CollectionViewModel<TModel, TViewModel>
    where TModel : class
    where TViewModel : class, IViewModelWrapper<TModel>
{
    public EditableCollectionViewModel(
        IDataStore<TModel> dataStore,
        IViewModelFactory<TModel, TViewModel> viewModelFactory,
        IEqualityComparer<TModel> modelComparer);
    
    public Func<TModel>? CreateModel { get; set; }
    public Action<TModel>? EditModel { get; set; }
    
    public ICommand AddCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand EditCommand { get; }
}
```

#### Constructor

Siehe [CollectionViewModel](#collectionviewmodeltmodel-tviewmodel) Constructor.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| **CreateModel** | `Func<TModel>?` | Factory-Funktion für neue Models |
| **EditModel** | `Action<TModel>?` | Callback zum Bearbeiten eines Models |
| **AddCommand** | `ICommand` | Command zum Hinzufügen eines neuen Elements |
| **DeleteCommand** | `ICommand` | Command zum Löschen des ausgewählten Elements |
| **ClearCommand** | `ICommand` | Command zum Löschen aller Elemente |
| **EditCommand** | `ICommand` | Command zum Bearbeiten des ausgewählten Elements |

#### Commands

| Command | CanExecute | Execute |
|---------|-----------|---------|
| **AddCommand** | `CreateModel != null` | `AddModel(CreateModel())` |
| **DeleteCommand** | `SelectedItem != null` | `RemoveViewModel(SelectedItem)` |
| **EditCommand** | `SelectedItem != null && EditModel != null` | `EditModel(SelectedItem.Model)` |
| **ClearCommand** | `Count > 0` | `Clear()` |

#### Example

```csharp
var viewModel = new EditableCollectionViewModel<Customer, CustomerViewModel>(
    dataStore, factory, comparer);

viewModel.CreateModel = () => new Customer { Name = "New" };
viewModel.EditModel = customer => OpenDialog(customer);

viewModel.AddCommand.Execute(null); // Fügt "New" hinzu
viewModel.SelectedItem = viewModel.Items.First();
viewModel.EditCommand.Execute(null); // Öffnet Dialog
viewModel.DeleteCommand.Execute(null); // Löscht "New"
```

---

## CustomWPFControls.Factories

### `IViewModelFactory<TModel, TViewModel>`

Factory-Interface für ViewModel-Erstellung.

```csharp
public interface IViewModelFactory<in TModel, out TViewModel>
    where TModel : class
    where TViewModel : class
{
    TViewModel Create(TModel model);
}
```

#### Methods

| Method | Return | Description |
|--------|--------|-------------|
| **Create(model)** | `TViewModel` | Erstellt ViewModel für das gegebene Model |

**Exceptions:**
- `ArgumentNullException` - Wenn `model` null ist
- `InvalidOperationException` - Wenn ViewModel nicht erstellt werden kann

---

### `ViewModelFactory<TModel, TViewModel>`

DI-basierte Factory-Implementierung.

```csharp
public sealed class ViewModelFactory<TModel, TViewModel> : IViewModelFactory<TModel, TViewModel>
    where TModel : class
    where TViewModel : class
{
    public ViewModelFactory(IServiceProvider serviceProvider);
    public TViewModel Create(TModel model);
}
```

#### Constructor

```csharp
public ViewModelFactory(IServiceProvider serviceProvider)
```

**Parameter:**
- `serviceProvider` - DI-Container für Dependency-Auflösung

**Exceptions:**
- `ArgumentNullException` - Wenn `serviceProvider` null ist

#### Methods

| Method | Return | Description |
|--------|--------|-------------|
| **Create(model)** | `TViewModel` | Erstellt ViewModel via ActivatorUtilities |

**Exceptions:**
- `ArgumentNullException` - Wenn `model` null ist
- `InvalidOperationException` - Wenn ViewModel nicht erstellt werden kann

#### Behavior

Verwendet `ActivatorUtilities.CreateInstance<TViewModel>()`:
1. Sucht Constructor mit `TModel` als ersten Parameter
2. Löst weitere Constructor-Parameter via DI auf
3. Erstellt ViewModel-Instanz

**Beispiel:**
```csharp
// ViewModel mit DI-Dependencies
public class CustomerViewModel : ViewModelBase<Customer>
{
    public CustomerViewModel(
        Customer model,                    // ? Von Factory übergeben
        IDialogService dialogService)      // ? Von DI aufgelöst
    {
        // ...
    }
}
```

#### Example

```csharp
var factory = new ViewModelFactory<Customer, CustomerViewModel>(serviceProvider);
var customer = new Customer { Name = "Alice" };
var viewModel = factory.Create(customer);
```

---

### `ViewModelFactoryExtensions`

DI-Extensions für Factory-Registrierung.

```csharp
public static class ViewModelFactoryExtensions
{
    public static IServiceCollection AddViewModelFactory<TModel, TViewModel>(
        this IServiceCollection services)
        where TModel : class
        where TViewModel : class;
}
```

#### Methods

| Method | Return | Description |
|--------|--------|-------------|
| **AddViewModelFactory<TModel, TViewModel>(services)** | `IServiceCollection` | Registriert ViewModelFactory als Singleton |

#### Example

```csharp
services.AddViewModelFactory<Customer, CustomerViewModel>();

// Equivalent zu:
services.AddSingleton<IViewModelFactory<Customer, CustomerViewModel>, 
                      ViewModelFactory<Customer, CustomerViewModel>>();
```

---

## ?? Type Constraints

### **TModel Constraints**

```csharp
where TModel : class
```

- **Muss Referenztyp sein** - Keine Structs/Value-Types
- **Für IDataStore kompatibel** - DataStore akzeptiert nur `class`

### **TViewModel Constraints**

```csharp
where TViewModel : class, IViewModelWrapper<TModel>
```

- **Muss Referenztyp sein** - Keine Structs
- **Muss IViewModelWrapper implementieren** - Model-Property vorhanden
- **Typischerweise von ViewModelBase<TModel> abgeleitet**

---

## ?? Best Practices

### **1. ViewModel Constructor**

```csharp
// ? RICHTIG: Model als erster Parameter
public CustomerViewModel(Customer model, IDialogService dialog)
    : base(model) { }

// ? FALSCH: Model nicht als erster Parameter
public CustomerViewModel(IDialogService dialog, Customer model)
    : base(model) { }
```

### **2. Property-Delegation**

```csharp
// ? RICHTIG: Read-only Domain-Properties
public string Name => Model.Name;

// ? FALSCH: Backing-Field für Domain-Property
private string _name;
public string Name { get => _name; set => _name = value; }
```

### **3. UI-Properties**

```csharp
// ? RICHTIG: Auto-Property mit PropertyChanged
public bool IsSelected { get; set; }

// ? FALSCH: Manuelles PropertyChanged (Fody macht das)
private bool _isSelected;
public bool IsSelected
{
    get => _isSelected;
    set { _isSelected = value; OnPropertyChanged(); }
}
```

### **4. IEqualityComparer**

```csharp
// ? RICHTIG: Basiert auf stabilen Properties (Id)
public class CustomerComparer : IEqualityComparer<Customer>
{
    public bool Equals(Customer? x, Customer? y)
        => x?.Id == y?.Id;
    public int GetHashCode(Customer obj)
        => obj.Id.GetHashCode();
}

// ? FALSCH: Basiert auf mutable Properties
public bool Equals(Customer? x, Customer? y)
    => x?.Name == y?.Name; // Name kann sich ändern!
```

---

## ?? See Also

- [Getting Started](Getting-Started.md) - Schnellstart-Guide
- [Architecture](Architecture.md) - Architektur-Übersicht
- [Best Practices](Best-Practices.md) - Tipps & Tricks
