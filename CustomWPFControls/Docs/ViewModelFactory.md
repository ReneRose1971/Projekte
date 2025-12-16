# ViewModelFactory<TModel, TViewModel>

DI-basierte Factory für die Erstellung von ViewModels mit automatischer Service-Injektion.

## ?? Inhaltsverzeichnis

- [Übersicht](#übersicht)
- [Features](#features)
- [Verwendung](#verwendung)
- [DI-Integration](#di-integration)
- [ActivatorUtilities](#activatorutilities)
- [Best Practices](#best-practices)
- [Beispiele](#beispiele)

## Übersicht

`ViewModelFactory<TModel, TViewModel>` ist eine **generische Factory**, die ViewModels via `ActivatorUtilities` erstellt und dabei automatisch Services aus dem DI-Container injiziert.

### Problemstellung ohne Factory

```csharp
// ? Ohne Factory: Manuelles Service-Resolving
public CustomerViewModel CreateViewModel(Customer customer)
{
    var messageService = _serviceProvider.GetRequiredService<IMessageService>();
    var validationService = _serviceProvider.GetRequiredService<IValidationService>();
    var logger = _serviceProvider.GetRequiredService<ILogger<CustomerViewModel>>();
    
    return new CustomerViewModel(
        customer,
        messageService,
        validationService,
        logger
    );  // Fehleranfällig, schwer wartbar
}
```

### Lösung mit Factory

```csharp
// ? Mit Factory: Automatisches Service-Resolving
var factory = _serviceProvider.GetRequiredService<IViewModelFactory<Customer, CustomerViewModel>>();
var viewModel = factory.Create(customer);
// ? Alle Services automatisch injiziert!
```

### Definition

```csharp
namespace CustomWPFControls.Factories;

public interface IViewModelFactory<TModel, TViewModel>
    where TModel : class
    where TViewModel : class
{
    TViewModel Create(TModel model);
}

public sealed class ViewModelFactory<TModel, TViewModel> : IViewModelFactory<TModel, TViewModel>
    where TModel : class
    where TViewModel : class
{
    public ViewModelFactory(IServiceProvider serviceProvider);
    public TViewModel Create(TModel model);
}
```

## Features

### 1. Automatische Service-Injektion

**ViewModel-Constructor:**
```csharp
public class CustomerViewModel : ViewModelBase<Customer>
{
    private readonly IMessageService _messageService;
    private readonly IValidationService _validationService;
    
    // Constructor: Model + DI-Services
    public CustomerViewModel(
        Customer model,                      // Wird von Factory übergeben
        IMessageService messageService,      // Automatisch injiziert
        IValidationService validationService) // Automatisch injiziert
        : base(model)
    {
        _messageService = messageService;
        _validationService = validationService;
    }
}
```

**Factory-Aufruf:**
```csharp
var factory = serviceProvider.GetRequiredService<IViewModelFactory<Customer, CustomerViewModel>>();
var viewModel = factory.Create(customer);
// ? IMessageService und IValidationService automatisch injiziert!
```

### 2. ActivatorUtilities-Integration

Factory nutzt `ActivatorUtilities.CreateInstance` von Microsoft.Extensions.DependencyInjection:

```csharp
public TViewModel Create(TModel model)
{
    // ActivatorUtilities löst Constructor-Parameter automatisch auf:
    // 1. model wird explizit übergeben
    // 2. Alle anderen Parameter werden aus IServiceProvider aufgelöst
    return ActivatorUtilities.CreateInstance<TViewModel>(_serviceProvider, model);
}
```

**Vorteile:**
- ? Automatisches Service-Resolving
- ? Unterstützt optionale Parameter
- ? Klare Fehlermeldungen bei Missing-Services
- ? Keine manuelle Service-Lokalisierung

### 3. Type-Safe Factory

**Compile-Time Safety:**
```csharp
// ? Type-Safe: Compiler prüft Typen
IViewModelFactory<Customer, CustomerViewModel> factory = ...;
CustomerViewModel vm = factory.Create(customer);

// ? Compile-Error bei falschen Typen
IViewModelFactory<Order, CustomerViewModel> wrongFactory = ...;  // Error!
OrderViewModel vm = factory.Create(customer);  // Error!
```

### 4. Fehlerbehandlung

```csharp
public TViewModel Create(TModel model)
{
    if (model == null)
        throw new ArgumentNullException(nameof(model));

    try
    {
        return ActivatorUtilities.CreateInstance<TViewModel>(_serviceProvider, model);
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException(
            $"Fehler beim Erstellen von {typeof(TViewModel).Name} für Model {typeof(TModel).Name}. " +
            $"Stellen Sie sicher, dass der ViewModel-Constructor (TModel model, ...) definiert ist.",
            ex);
    }
}
```

**Häufige Fehler:**
- **Constructor nicht gefunden**: ViewModel hat keinen passenden Constructor
- **Service nicht registriert**: Benötigter Service fehlt im DI-Container
- **Ambiguous Constructor**: Mehrere Constructor mit gleicher Anzahl Parameter

## Verwendung

### Registrierung via Extension-Method

```csharp
using CustomWPFControls.Factories;
using Microsoft.Extensions.DependencyInjection;

public static class ViewModelFactoryExtensions
{
    public static IServiceCollection AddViewModelFactory<TModel, TViewModel>(
        this IServiceCollection services)
        where TModel : class
        where TViewModel : class
    {
        services.AddSingleton<IViewModelFactory<TModel, TViewModel>, 
                              ViewModelFactory<TModel, TViewModel>>();
        return services;
    }
}

// Verwendung:
services.AddViewModelFactory<Customer, CustomerViewModel>();
services.AddViewModelFactory<Order, OrderViewModel>();
services.AddViewModelFactory<Product, ProductViewModel>();
```

### Verwendung in CollectionViewModel

```csharp
public class MainViewModel
{
    private readonly CollectionViewModel<Customer, CustomerViewModel> _customers;
    
    public MainViewModel(
        IDataStoreProvider provider,
        IRepositoryFactory repositoryFactory,
        IViewModelFactory<Customer, CustomerViewModel> viewModelFactory,  // ? Injiziert
        IEqualityComparer<Customer> comparer)
    {
        var dataStore = provider.GetPersistent<Customer>(
            repositoryFactory,
            autoLoad: true);
        
        _customers = new CollectionViewModel<Customer, CustomerViewModel>(
            dataStore,
            viewModelFactory,  // ? Factory übergeben
            comparer);
    }
    
    public ReadOnlyObservableCollection<CustomerViewModel> Customers 
        => _customers.Items;
}
```

### Manuelle Verwendung

```csharp
public class CustomerService
{
    private readonly IViewModelFactory<Customer, CustomerViewModel> _factory;
    
    public CustomerService(IViewModelFactory<Customer, CustomerViewModel> factory)
    {
        _factory = factory;
    }
    
    public CustomerViewModel CreateViewModel(int customerId)
    {
        var customer = LoadCustomerById(customerId);
        return _factory.Create(customer);  // ? Services automatisch injiziert
    }
    
    public IEnumerable<CustomerViewModel> CreateViewModels(IEnumerable<Customer> customers)
    {
        return customers.Select(c => _factory.Create(c));
    }
}
```

## DI-Integration

### Vollständige Registrierung

```csharp
using Common.Bootstrap;
using Common.Bootstrap.Defaults;
using CustomWPFControls.Factories;
using DataToolKit.Abstractions.DI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// 1. DataToolKit Services
new DataToolKitServiceModule().Register(builder.Services);

// 2. EqualityComparer
builder.Services.AddSingleton<IEqualityComparer<Customer>>(
    new FallbackEqualsComparer<Customer>());

// 3. Repository
builder.Services.AddLiteDbRepository<Customer>(
    appSubFolder: "MyApp",
    fileNameBase: "customers",
    subFolder: "Databases");

// 4. Application Services
builder.Services.AddSingleton<IMessageService, MessageService>();
builder.Services.AddSingleton<IValidationService, ValidationService>();

// 5. ViewModelFactory
builder.Services.AddViewModelFactory<Customer, CustomerViewModel>();

// 6. ViewModels
builder.Services.AddTransient<MainViewModel>();

var app = builder.Build();
await app.RunAsync();
```

### Service-Module Pattern

```csharp
public class ViewModelModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // Factories
        services.AddViewModelFactory<Customer, CustomerViewModel>();
        services.AddViewModelFactory<Order, OrderViewModel>();
        services.AddViewModelFactory<Product, ProductViewModel>();
        
        // ViewModels
        services.AddTransient<CustomerListViewModel>();
        services.AddTransient<OrderListViewModel>();
        services.AddTransient<ProductListViewModel>();
        services.AddTransient<MainViewModel>();
    }
}

// Verwendung:
new ViewModelModule().Register(builder.Services);
```

## ActivatorUtilities

### Wie ActivatorUtilities funktioniert

```csharp
// ViewModel-Constructor:
public CustomerViewModel(
    Customer model,                      // Parameter 1: Explizit übergeben
    IMessageService messageService,      // Parameter 2: Aus DI
    IValidationService validationService) // Parameter 3: Aus DI
    : base(model)
{
    // ...
}

// Factory.Create():
ActivatorUtilities.CreateInstance<CustomerViewModel>(_serviceProvider, customer);

// Intern:
// 1. Findet Constructor: CustomerViewModel(Customer, IMessageService, IValidationService)
// 2. Parameter 1 (Customer): Verwendet customer (explizit übergeben)
// 3. Parameter 2 (IMessageService): Ruft _serviceProvider.GetService<IMessageService>() auf
// 4. Parameter 3 (IValidationService): Ruft _serviceProvider.GetService<IValidationService>() auf
// 5. Erstellt Instanz: new CustomerViewModel(customer, messageService, validationService)
```

### Constructor-Auswahl

**ActivatorUtilities wählt Constructor:**
1. Findet Constructor mit **meisten** auflösbaren Parametern
2. Bevorzugt Constructor mit **wenigsten** Parametern bei Gleichstand

**Beispiel:**
```csharp
public class MyViewModel : ViewModelBase<Model>
{
    // Constructor 1: 2 Parameter
    public MyViewModel(Model model, IServiceA serviceA) 
        : base(model) { }
    
    // Constructor 2: 3 Parameter (wird bevorzugt, falls alle Services registriert)
    public MyViewModel(Model model, IServiceA serviceA, IServiceB serviceB) 
        : base(model) { }
}

// Falls IServiceA und IServiceB registriert:
// ? Constructor 2 wird verwendet

// Falls nur IServiceA registriert:
// ? Constructor 1 wird verwendet
```

### Optionale Parameter

```csharp
public class CustomerViewModel : ViewModelBase<Customer>
{
    // Optionale Parameter mit Default-Werten
    public CustomerViewModel(
        Customer model,
        IMessageService messageService,
        ILogger<CustomerViewModel>? logger = null)  // Optional
        : base(model)
    {
        _messageService = messageService;
        _logger = logger ?? NullLogger<CustomerViewModel>.Instance;
    }
}

// ? Funktioniert auch ohne ILogger-Registrierung
```

## Best Practices

### ? Do's

**1. Constructor-First-Parameter ist Model:**
```csharp
// ? Gut: Model ist erster Parameter
public CustomerViewModel(
    Customer model,              // ? Erster Parameter
    IMessageService service)
    : base(model) { }

// ? Schlecht: Model nicht an erster Stelle
public CustomerViewModel(
    IMessageService service,
    Customer model)              // ? Zweiter Parameter
    : base(model) { }
```

**2. Factory-Lifetime: Singleton:**
```csharp
// ? Gut: Factory ist Singleton (Lightweight Object)
services.AddSingleton<IViewModelFactory<Customer, CustomerViewModel>, 
                      ViewModelFactory<Customer, CustomerViewModel>>();

// ? Schlecht: Factory als Transient (unnötiger Overhead)
services.AddTransient<IViewModelFactory<Customer, CustomerViewModel>, ...>();
```

**3. ViewModel-Lifetime: Transient oder per Factory:**
```csharp
// ? Gut: ViewModels via Factory erstellen (nicht direkt registrieren)
services.AddViewModelFactory<Customer, CustomerViewModel>();

// ? Schlecht: ViewModels direkt registrieren (DI kann Model nicht injizieren)
services.AddTransient<CustomerViewModel>();  // Fehler: Kein Customer im DI!
```

**4. Services explizit registrieren:**
```csharp
// ? Gut: Alle benötigten Services registrieren
services.AddSingleton<IMessageService, MessageService>();
services.AddSingleton<IValidationService, ValidationService>();
services.AddViewModelFactory<Customer, CustomerViewModel>();

// ? Schlecht: Service fehlt
// (IValidationService nicht registriert, aber in Constructor benötigt)
```

### ? Don'ts

**1. Keine ServiceLocator-Pattern:**
```csharp
// ? Schlecht: ServiceLocator im ViewModel
public class CustomerViewModel : ViewModelBase<Customer>
{
    public CustomerViewModel(Customer model, IServiceProvider serviceProvider)
        : base(model)
    {
        var messageService = serviceProvider.GetService<IMessageService>();  // Anti-Pattern!
    }
}

// ? Gut: Constructor Injection
public class CustomerViewModel : ViewModelBase<Customer>
{
    public CustomerViewModel(Customer model, IMessageService messageService)
        : base(model)
    {
        _messageService = messageService;
    }
}
```

**2. Keine Logik in Factory:**
```csharp
// ? Schlecht: Factory mit Zusatzlogik
public class CustomFactory : IViewModelFactory<Customer, CustomerViewModel>
{
    public CustomerViewModel Create(Customer model)
    {
        var vm = ActivatorUtilities.CreateInstance<CustomerViewModel>(_serviceProvider, model);
        vm.Initialize();  // ? Logik in Factory!
        vm.LoadData();    // ? Logik in Factory!
        return vm;
    }
}

// ? Gut: Logik im ViewModel-Constructor
public CustomerViewModel(Customer model, IDataService dataService)
    : base(model)
{
    Initialize();
    LoadData();
}
```

**3. Keine Factory-Vererbung für Anpassungen:**
```csharp
// ? Schlecht: Factory überschreiben
public class CustomViewModelFactory : ViewModelFactory<Customer, CustomerViewModel>
{
    public override CustomerViewModel Create(Customer model)
    {
        var vm = base.Create(model);
        // Custom Logic
        return vm;
    }
}

// ? Gut: Custom ViewModel-Basisklasse
public class EnhancedCustomerViewModel : CustomerViewModel
{
    public EnhancedCustomerViewModel(Customer model, /* services */)
        : base(model, /* services */)
    {
        // Custom Logic
    }
}
```

## Beispiele

### Beispiel 1: Einfache Factory-Verwendung

```csharp
// 1. Model
public class Task : EntityBase
{
    public string Title { get; set; } = "";
    public bool IsCompleted { get; set; }
}

// 2. ViewModel mit Services
public class TaskViewModel : ViewModelBase<Task>
{
    private readonly INotificationService _notificationService;
    
    public TaskViewModel(
        Task model,
        INotificationService notificationService)
        : base(model)
    {
        _notificationService = notificationService;
    }
    
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
                
                if (value)
                    _notificationService.ShowSuccess("Task completed!");
            }
        }
    }
}

// 3. Registrierung
services.AddSingleton<INotificationService, NotificationService>();
services.AddViewModelFactory<Task, TaskViewModel>();

// 4. Verwendung
var factory = serviceProvider.GetRequiredService<IViewModelFactory<Task, TaskViewModel>>();
var task = new Task { Title = "Buy milk" };
var viewModel = factory.Create(task);
// ? INotificationService automatisch injiziert
```

### Beispiel 2: Multiple ViewModels mit Shared Services

```csharp
// Shared Services
services.AddSingleton<IMessageService, MessageService>();
services.AddSingleton<IDialogService, DialogService>();
services.AddSingleton<ILogger<object>, Logger<object>>();

// Multiple Factories
services.AddViewModelFactory<Customer, CustomerViewModel>();
services.AddViewModelFactory<Order, OrderViewModel>();
services.AddViewModelFactory<Product, ProductViewModel>();

// MainViewModel nutzt alle Factories
public class MainViewModel
{
    private readonly IViewModelFactory<Customer, CustomerViewModel> _customerFactory;
    private readonly IViewModelFactory<Order, OrderViewModel> _orderFactory;
    private readonly IViewModelFactory<Product, ProductViewModel> _productFactory;
    
    public MainViewModel(
        IViewModelFactory<Customer, CustomerViewModel> customerFactory,
        IViewModelFactory<Order, OrderViewModel> orderFactory,
        IViewModelFactory<Product, ProductViewModel> productFactory)
    {
        _customerFactory = customerFactory;
        _orderFactory = orderFactory;
        _productFactory = productFactory;
    }
    
    public void LoadCustomer(int id)
    {
        var customer = LoadCustomerFromDb(id);
        var viewModel = _customerFactory.Create(customer);
        // ? Alle Services automatisch injiziert
    }
}
```

### Beispiel 3: Factory mit CollectionViewModel

```csharp
public class OrderManagementViewModel : IDisposable
{
    private readonly EditableCollectionViewModel<Order, OrderViewModel> _orders;
    
    public OrderManagementViewModel(
        IDataStoreProvider provider,
        IRepositoryFactory repositoryFactory,
        IViewModelFactory<Order, OrderViewModel> viewModelFactory,  // ? Factory injiziert
        IEqualityComparer<Order> comparer)
    {
        var dataStore = provider.GetPersistent<Order>(
            repositoryFactory,
            autoLoad: true);
        
        _orders = new EditableCollectionViewModel<Order, OrderViewModel>(
            dataStore,
            viewModelFactory,  // ? Factory wird weitergegeben
            comparer);
        
        // Commands
        _orders.CreateModel = () => new Order { Id = 0 };
        _orders.EditModel = (order) => OpenEditDialog(order);
    }
    
    public ReadOnlyObservableCollection<OrderViewModel> Orders => _orders.Items;
    public ICommand AddCommand => _orders.AddCommand;
    public ICommand EditCommand => _orders.EditCommand;
    public ICommand DeleteCommand => _orders.DeleteCommand;
    
    public void Dispose() => _orders.Dispose();
}

// Jedes neue OrderViewModel wird via Factory erstellt:
// - dataStore.Add(order) wird aufgerufen
// - CollectionViewModel erkennt Add-Event
// - viewModelFactory.Create(order) wird aufgerufen
// - Neues OrderViewModel mit allen Services wird erstellt
// ? Vollständig automatisch!
```

### Beispiel 4: Factory mit optionalen Services

```csharp
public class EnhancedViewModel : ViewModelBase<Model>
{
    private readonly ILogger<EnhancedViewModel> _logger;
    
    // Logger ist optional (Default = NullLogger)
    public EnhancedViewModel(
        Model model,
        IMessageService messageService,
        ILogger<EnhancedViewModel>? logger = null)
        : base(model)
    {
        _messageService = messageService;
        _logger = logger ?? NullLogger<EnhancedViewModel>.Instance;
    }
}

// Registrierung ohne Logger:
services.AddSingleton<IMessageService, MessageService>();
services.AddViewModelFactory<Model, EnhancedViewModel>();
// ? Funktioniert! logger = null wird verwendet

// Registrierung mit Logger:
services.AddSingleton<IMessageService, MessageService>();
services.AddLogging();
services.AddViewModelFactory<Model, EnhancedViewModel>();
// ? Funktioniert! logger wird injiziert
```

## Siehe auch

- ?? [ViewModelBase](ViewModelBase.md) - Basisklasse für ViewModels
- ?? [CollectionViewModel](CollectionViewModel.md) - Nutzt ViewModelFactory
- ?? [EditableCollectionViewModel](EditableCollectionViewModel.md) - CRUD mit Factory
- ?? [Getting Started](Getting-Started.md) - Schnellstart-Guide
- ?? [Architecture](Architecture.md) - Architektur-Übersicht
