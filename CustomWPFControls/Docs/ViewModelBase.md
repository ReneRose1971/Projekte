# ViewModelBase<TModel>

Basisklasse für alle ViewModels mit automatischem PropertyChanged-Support und Model-Wrapping.

## ?? Inhaltsverzeichnis

- [Übersicht](#übersicht)
- [Features](#features)
- [Verwendung](#verwendung)
- [PropertyChanged via Fody](#propertychanged-via-fody)
- [Equals und GetHashCode](#equals-und-gethashcode)
- [Best Practices](#best-practices)
- [Beispiele](#beispiele)

## Übersicht

`ViewModelBase<TModel>` ist die **abstrakte Basisklasse** für alle ViewModels in CustomWPFControls. Sie wraps ein Domain-Model und bietet:

- ? **Automatisches PropertyChanged** via Fody.PropertyChanged
- ? **Referenz-basierte Gleichheit** (zwei ViewModels mit gleichem Model sind gleich)
- ? **Model-Wrapping** für klare Trennung zwischen Domain und View
- ? **INotifyPropertyChanged** ohne manuellen Code

### Definition

```csharp
namespace CustomWPFControls.ViewModels;

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
}
```

## Features

### 1. Model-Wrapping

ViewModels wraps Domain-Models ohne sie zu modifizieren:

```csharp
// Domain-Model (unverändert)
public class Customer : EntityBase
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}

// ViewModel wraps das Model
public class CustomerViewModel : ViewModelBase<Customer>
{
    public CustomerViewModel(Customer model) : base(model) { }
    
    // Domain-Properties (delegiert an Model)
    public string Name
    {
        get => Model.Name;
        set
        {
            if (Model.Name != value)
            {
                Model.Name = value;
                OnPropertyChanged();
            }
        }
    }
    
    // UI-spezifische Properties
    public bool IsSelected { get; set; }  // Nur im ViewModel
    public bool IsExpanded { get; set; }  // Nur im ViewModel
}
```

**Vorteile:**
- ? Domain-Model bleibt UI-unabhängig
- ? UI-Properties nur im ViewModel
- ? Klare Trennung zwischen Business und View
- ? Testbarkeit (Model ohne UI-Code)

### 2. Automatisches PropertyChanged

Durch `[AddINotifyPropertyChangedInterface]` wird INotifyPropertyChanged automatisch implementiert:

```csharp
[AddINotifyPropertyChangedInterface]
public class CustomerViewModel : ViewModelBase<Customer>
{
    // ? PropertyChanged wird automatisch gefeuert!
    public bool IsSelected { get; set; }
    public bool IsExpanded { get; set; }
    public string SearchFilter { get; set; } = "";
}

// Verwendung in WPF:
<CheckBox IsChecked="{Binding IsSelected}" />
<!-- ? UI wird automatisch aktualisiert -->
```

**Wie es funktioniert:**

1. Fody.PropertyChanged weaves IL-Code zur Kompilierzeit
2. Jeder Setter ruft automatisch `OnPropertyChanged()` auf
3. Kein manueller PropertyChanged-Code nötig

**Vor Fody (manuell):**
```csharp
private bool _isSelected;
public bool IsSelected
{
    get => _isSelected;
    set
    {
        if (_isSelected != value)
        {
            _isSelected = value;
            OnPropertyChanged(nameof(IsSelected));
        }
    }
}
```

**Mit Fody (automatisch):**
```csharp
public bool IsSelected { get; set; }  // ? Das war's!
```

### 3. Referenz-basierte Gleichheit

ViewModels sind gleich, wenn sie **die gleiche Model-Instanz** wrappen:

```csharp
var customer = new Customer { Id = 1, Name = "Alice" };

var vm1 = new CustomerViewModel(customer);
var vm2 = new CustomerViewModel(customer);
var vm3 = new CustomerViewModel(new Customer { Id = 1, Name = "Alice" });

// ? Gleiche Model-Referenz
Assert.True(vm1.Equals(vm2));  // True
Assert.Equal(vm1.GetHashCode(), vm2.GetHashCode());

// ? Unterschiedliche Model-Referenz
Assert.False(vm1.Equals(vm3));  // False (anderes Model-Objekt)
```

**Wichtig:**
- Gleichheit basiert auf **Referenz**, nicht auf **Inhalt**
- Zwei ViewModels mit "identischen" aber unterschiedlichen Model-Instanzen sind **nicht** gleich
- Dies ist korrekt für MVVM-Szenarien (ein ViewModel pro Model-Instanz)

## Verwendung

### Minimal-Beispiel

```csharp
// 1. Domain-Model
public class Product : EntityBase
{
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}

// 2. ViewModel
public class ProductViewModel : ViewModelBase<Product>
{
    public ProductViewModel(Product model) : base(model) { }
    
    // Domain-Properties (delegiert an Model)
    public string Name
    {
        get => Model.Name;
        set
        {
            if (Model.Name != value)
            {
                Model.Name = value;
                OnPropertyChanged();
            }
        }
    }
    
    public decimal Price
    {
        get => Model.Price;
        set
        {
            if (Model.Price != value)
            {
                Model.Price = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PriceFormatted));  // Calculated property
            }
        }
    }
    
    // UI-Properties
    public string PriceFormatted => $"{Price:C}";
    public bool IsOnSale { get; set; }
}

// 3. Verwendung
var product = new Product { Name = "Laptop", Price = 999.99m };
var viewModel = new ProductViewModel(product);

viewModel.Price = 899.99m;
// ? Model.Price wird aktualisiert
// ? PropertyChanged für "Price" und "PriceFormatted" gefeuert
```

### Mit DependencyInjection

```csharp
// ViewModel mit zusätzlichen Services
public class CustomerViewModel : ViewModelBase<Customer>
{
    private readonly IMessageService _messageService;
    
    // Constructor: Model + DI-Services
    public CustomerViewModel(
        Customer model,
        IMessageService messageService) : base(model)
    {
        _messageService = messageService;
    }
    
    public void SendEmail()
    {
        _messageService.Send(Model.Email, "Hello!");
    }
}

// Erstellen via ViewModelFactory (siehe ViewModelFactory.md)
var factory = serviceProvider.GetRequiredService<IViewModelFactory<Customer, CustomerViewModel>>();
var viewModel = factory.Create(customer);
// ? IMessageService wird automatisch injiziert
```

## PropertyChanged via Fody

### Setup (bereits konfiguriert)

```xml
<!-- CustomWPFControls.csproj -->
<ItemGroup>
  <PackageReference Include="PropertyChanged.Fody" Version="4.x" />
  <PackageReference Include="Fody" Version="6.x" />
</ItemGroup>
```

```xml
<!-- FodyWeavers.xml -->
<Weavers>
  <PropertyChanged />
</Weavers>
```

### Verwendung

```csharp
[AddINotifyPropertyChangedInterface]  // ? Auf Klasse
public class MyViewModel : ViewModelBase<MyModel>
{
    // ? Automatisch PropertyChanged
    public string Title { get; set; }
    public int Count { get; set; }
    
    // ? Kein PropertyChanged (explizit deaktiviert)
    [DoNotNotify]
    public string InternalCache { get; set; }
    
    // ? Abhängige Properties
    [DependsOn(nameof(Count))]
    public string CountText => $"Items: {Count}";
}
```

### Erweiterte Fody-Features

**1. DependsOn - Verkettete Properties:**
```csharp
public string FirstName { get; set; } = "";
public string LastName { get; set; } = "";

[DependsOn(nameof(FirstName), nameof(LastName))]
public string FullName => $"{FirstName} {LastName}";
// ? PropertyChanged für FullName, wenn FirstName oder LastName ändert
```

**2. AlsoNotifyFor - Alternative Syntax:**
```csharp
[AlsoNotifyFor(nameof(PriceFormatted))]
public decimal Price { get; set; }

public string PriceFormatted => $"{Price:C}";
// ? PropertyChanged für beide Properties
```

**3. OnChanged - Custom Logic:**
```csharp
public string SearchText { get; set; } = "";

// Wird automatisch aufgerufen nach PropertyChanged
private void OnSearchTextChanged()
{
    // Custom logic
    FilterResults();
}
```

## Equals und GetHashCode

### Implementierung

```csharp
public override int GetHashCode()
{
    // RuntimeHelpers.GetHashCode = Referenz-basierter HashCode
    return RuntimeHelpers.GetHashCode(Model);
}

public override bool Equals(object? obj)
{
    if (obj is not ViewModelBase<TModel> other)
        return false;

    // ReferenceEquals = Vergleicht Referenzen, nicht Werte
    return ReferenceEquals(Model, other.Model);
}
```

### Warum Referenz-basiert?

```csharp
// Szenario: DataStore mit Models
var dataStore = new InMemoryDataStore<Customer>();
var customer = new Customer { Id = 1, Name = "Alice" };
dataStore.Add(customer);

// ViewModels erstellen
var vm1 = new CustomerViewModel(customer);
var vm2 = new CustomerViewModel(customer);

// ? Beide ViewModels sind gleich (gleiche Model-Referenz)
Assert.True(vm1.Equals(vm2));

// ? Kann für Collections verwendet werden
var collection = new HashSet<CustomerViewModel>();
collection.Add(vm1);
collection.Add(vm2);  // Wird nicht hinzugefügt (Duplikat)
Assert.Equal(1, collection.Count);
```

**Wichtig für:**
- CollectionViewModel (Model-to-ViewModel-Mapping)
- UI-Selections (SelectedItem basiert auf Referenz)
- Performance (kein teurer Value-Vergleich)

## Best Practices

### ? Do's

**1. Domain-Properties delegieren an Model:**
```csharp
public string Name
{
    get => Model.Name;
    set
    {
        if (Model.Name != value)
        {
            Model.Name = value;
            OnPropertyChanged();
        }
    }
}
```

**2. UI-Properties nur im ViewModel:**
```csharp
// ? Nur im ViewModel (nicht im Model)
public bool IsSelected { get; set; }
public bool IsExpanded { get; set; }
public Visibility Visibility { get; set; }
```

**3. Calculated Properties mit DependsOn:**
```csharp
[DependsOn(nameof(FirstName), nameof(LastName))]
public string FullName => $"{FirstName} {LastName}";
```

**4. Commands als Properties:**
```csharp
private ICommand? _saveCommand;
public ICommand SaveCommand => _saveCommand ??= new RelayCommand(
    _ => Save(),
    _ => CanSave());
```

### ? Don'ts

**1. Keine Geschäftslogik im ViewModel:**
```csharp
// ? Schlecht: Geschäftslogik im ViewModel
public decimal CalculateTotalPrice()
{
    return Items.Sum(x => x.Price * x.Quantity);
}

// ? Gut: Geschäftslogik im Model oder Service
public decimal TotalPrice => _priceCalculator.Calculate(Model);
```

**2. Kein direktes Model-Mutation ohne PropertyChanged:**
```csharp
// ? Schlecht: Model direkt ändern
Model.Name = "New Name";  // UI wird nicht aktualisiert!

// ? Gut: Via Property mit PropertyChanged
public string Name
{
    get => Model.Name;
    set
    {
        if (Model.Name != value)
        {
            Model.Name = value;
            OnPropertyChanged();
        }
    }
}
```

**3. Keine Abhängigkeiten vom UI-Framework im Model:**
```csharp
// ? Schlecht: WPF-Typen im Model
public class Customer : EntityBase
{
    public Visibility Visibility { get; set; }  // WPF-Typ!
}

// ? Gut: UI-Typen nur im ViewModel
public class CustomerViewModel : ViewModelBase<Customer>
{
    public Visibility Visibility { get; set; }
}
```

## Beispiele

### Beispiel 1: Einfaches ViewModel

```csharp
public class TaskViewModel : ViewModelBase<TodoTask>
{
    public TaskViewModel(TodoTask model) : base(model) { }
    
    public string Title
    {
        get => Model.Title;
        set
        {
            if (Model.Title != value)
            {
                Model.Title = value;
                OnPropertyChanged();
            }
        }
    }
    
    public bool IsCompleted
    {
        get => Model.IsCompleted;
        set
        {
            if (Model.IsCompleted != value)
            {
                Model.IsCompleted = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusText));
            }
        }
    }
    
    public string StatusText => IsCompleted ? "? Erledigt" : "? Offen";
}
```

### Beispiel 2: ViewModel mit Commands

```csharp
public class CustomerViewModel : ViewModelBase<Customer>
{
    private readonly ICustomerService _service;
    
    public CustomerViewModel(Customer model, ICustomerService service) 
        : base(model)
    {
        _service = service;
    }
    
    public string Name
    {
        get => Model.Name;
        set
        {
            if (Model.Name != value)
            {
                Model.Name = value;
                OnPropertyChanged();
            }
        }
    }
    
    // Commands
    private ICommand? _saveCommand;
    public ICommand SaveCommand => _saveCommand ??= new RelayCommand(
        _ => _service.Save(Model),
        _ => !string.IsNullOrWhiteSpace(Name));
    
    private ICommand? _deleteCommand;
    public ICommand DeleteCommand => _deleteCommand ??= new RelayCommand(
        _ => _service.Delete(Model),
        _ => Model.Id > 0);
}
```

### Beispiel 3: ViewModel mit Validierung

```csharp
public class UserViewModel : ViewModelBase<User>, INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> _errors = new();
    
    public UserViewModel(User model) : base(model) { }
    
    public string Email
    {
        get => Model.Email;
        set
        {
            if (Model.Email != value)
            {
                Model.Email = value;
                OnPropertyChanged();
                ValidateEmail();
            }
        }
    }
    
    private void ValidateEmail()
    {
        ClearErrors(nameof(Email));
        
        if (string.IsNullOrWhiteSpace(Email))
        {
            AddError(nameof(Email), "E-Mail ist erforderlich");
        }
        else if (!Email.Contains("@"))
        {
            AddError(nameof(Email), "Ungültige E-Mail-Adresse");
        }
    }
    
    // INotifyDataErrorInfo Implementation
    public bool HasErrors => _errors.Count > 0;
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    
    public IEnumerable GetErrors(string? propertyName)
    {
        return propertyName != null && _errors.ContainsKey(propertyName)
            ? _errors[propertyName]
            : Enumerable.Empty<string>();
    }
    
    private void AddError(string propertyName, string error)
    {
        if (!_errors.ContainsKey(propertyName))
            _errors[propertyName] = new List<string>();
        
        _errors[propertyName].Add(error);
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }
    
    private void ClearErrors(string propertyName)
    {
        if (_errors.Remove(propertyName))
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}
```

## Siehe auch

- ?? [CollectionViewModel](CollectionViewModel.md) - Collection-Verwaltung mit ViewModels
- ?? [EditableCollectionViewModel](EditableCollectionViewModel.md) - Mit Commands
- ?? [ViewModelFactory](ViewModelFactory.md) - DI-basierte ViewModel-Erstellung
- ?? [Getting Started](Getting-Started.md) - Schnellstart-Guide
- ?? [API Reference](API-Reference.md) - Vollständige API
