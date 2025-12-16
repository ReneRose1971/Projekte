# CollectionViewModel<TModel, TViewModel>

Bidirektionale Synchronisation zwischen DataStore (Models) und ObservableCollection (ViewModels) mit automatischem Lifecycle-Management.

## ?? Inhaltsverzeichnis

- [Übersicht](#übersicht)
- [Features](#features)
- [Verwendung](#verwendung)
- [Bidirektionale Synchronisation](#bidirektionale-synchronisation)
- [ViewModel-Lifecycle](#viewmodel-lifecycle)
- [Best Practices](#best-practices)
- [Beispiele](#beispiele)

## Übersicht

`CollectionViewModel<TModel, TViewModel>` ist die **zentrale Klasse** für Collection-Verwaltung in CustomWPFControls. Sie:

- ? **Synchronisiert** DataStore ? ViewModels bidirektional
- ? **Erstellt** ViewModels automatisch via Factory
- ? **Disposed** ViewModels automatisch bei Remove
- ? **Tracked** SelectedItem für UI-Binding
- ? **Bietet** Thread-Safe Operations via DataStore

### Definition

```csharp
namespace CustomWPFControls.ViewModels;

public class CollectionViewModel<TModel, TViewModel> : INotifyPropertyChanged, IDisposable
    where TModel : class
    where TViewModel : class, IViewModelWrapper<TModel>
{
    public ReadOnlyObservableCollection<TViewModel> Items { get; }
    public TViewModel? SelectedItem { get; set; }
    public int Count { get; }
    
    public CollectionViewModel(
        IDataStore<TModel> dataStore,
        IViewModelFactory<TModel, TViewModel> viewModelFactory,
        IEqualityComparer<TModel> modelComparer);
    
    public bool AddModel(TModel model);
    public bool RemoveModel(TModel model);
    public bool RemoveViewModel(TViewModel viewModel);
    public void Clear();
}
```

## Features

### 1. Bidirektionale Synchronisation

**DataStore ? ViewModels:**
```csharp
// DataStore ändert sich
dataStore.Add(customer);
// ? Automatisch: ViewModel wird erstellt und zu Items hinzugefügt

dataStore.Remove(customer);
// ? Automatisch: ViewModel wird entfernt und disposed
```

**ViewModels ? DataStore:**
```csharp
// ViewModel-Methode aufrufen
collectionViewModel.AddModel(newCustomer);
// ? Automatisch: Model wird zu DataStore hinzugefügt
// ? Automatisch: ViewModel wird erstellt (via CollectionChanged)

collectionViewModel.RemoveViewModel(viewModel);
// ? Automatisch: Model wird aus DataStore entfernt
// ? Automatisch: ViewModel wird disposed
```

### 2. Automatisches ViewModel-Lifecycle

**Erstellen:**
```csharp
// DataStore erhält neues Model
dataStore.Add(customer);

// Intern:
// 1. ViewModelFactory.Create(customer) wird aufgerufen
// 2. ViewModel wird in _modelToViewModelMap registriert
// 3. ViewModel wird zu Items hinzugefügt
// ? UI zeigt automatisch neues Item
```

**Entfernen:**
```csharp
// Model wird aus DataStore entfernt
dataStore.Remove(customer);

// Intern:
// 1. ViewModel wird aus Items entfernt
// 2. ViewModel wird aus _modelToViewModelMap entfernt
// 3. OnViewModelRemoving(viewModel) wird aufgerufen (Hook)
// 4. viewModel.Dispose() wird aufgerufen (falls IDisposable)
// ? UI-Update und Cleanup automatisch
```

### 3. SelectedItem-Tracking

**Automatisches SelectedItem-Management:**
```csharp
// Normaler Fall: SelectedItem bleibt
collectionViewModel.SelectedItem = viewModel1;
dataStore.Add(newCustomer);  // SelectedItem bleibt viewModel1

// Entfernen des ausgewählten Items
collectionViewModel.SelectedItem = viewModel1;
dataStore.Remove(viewModel1.Model);
// ? SelectedItem wird automatisch auf null gesetzt

// Clear der Collection
collectionViewModel.SelectedItem = viewModel1;
dataStore.Clear();
// ? SelectedItem wird automatisch auf null gesetzt
```

### 4. Model-to-ViewModel-Mapping

Interne Dictionary für effizientes Lookup:

```csharp
// Intern: Dictionary<TModel, TViewModel> mit Custom Comparer
private readonly Dictionary<TModel, TViewModel> _modelToViewModelMap;

// Verwendet IEqualityComparer<TModel> für Vergleiche
// ? Kein teures LINQ-Searching
// ? O(1) Lookup-Performance
// ? Duplikat-Prevention
```

## Verwendung

### Grundlegende Verwendung

```csharp
using CustomWPFControls.ViewModels;
using DataToolKit.Abstractions.DataStores;

public class MainViewModel
{
    private readonly CollectionViewModel<Customer, CustomerViewModel> _customers;
    
    public MainViewModel(
        IDataStoreProvider provider,
        IRepositoryFactory repositoryFactory,
        IViewModelFactory<Customer, CustomerViewModel> viewModelFactory,
        IEqualityComparer<Customer> comparer)
    {
        // 1. DataStore abrufen
        var dataStore = provider.GetPersistent<Customer>(
            repositoryFactory,
            autoLoad: true);
        
        // 2. CollectionViewModel erstellen
        _customers = new CollectionViewModel<Customer, CustomerViewModel>(
            dataStore,
            viewModelFactory,
            comparer);
    }
    
    // UI bindet an diese Collection
    public ReadOnlyObservableCollection<CustomerViewModel> Customers 
        => _customers.Items;
    
    // UI bindet an SelectedItem
    public CustomerViewModel? SelectedCustomer
    {
        get => _customers.SelectedItem;
        set => _customers.SelectedItem = value;
    }
}
```

### XAML-Binding

```xml
<Window>
    <Grid>
        <!-- ListBox bindet an Customers -->
        <ListBox ItemsSource="{Binding Customers}"
                 SelectedItem="{Binding SelectedCustomer}">
            <ListBox.ItemTemplate>
                <DataTemplate>
                    <StackPanel>
                        <TextBlock Text="{Binding Name}" FontWeight="Bold"/>
                        <TextBlock Text="{Binding Email}" Foreground="Gray"/>
                    </StackPanel>
                </DataTemplate>
            </ListBox.ItemTemplate>
        </ListBox>
        
        <!-- Details für SelectedCustomer -->
        <StackPanel DataContext="{Binding SelectedCustomer}">
            <TextBox Text="{Binding Name, UpdateSourceTrigger=PropertyChanged}"/>
            <TextBox Text="{Binding Email, UpdateSourceTrigger=PropertyChanged}"/>
            <CheckBox IsChecked="{Binding IsActive}"/>
        </StackPanel>
    </Grid>
</Window>
```

### Mit DI-Registrierung

```csharp
using CustomWPFControls.Factories;
using Microsoft.Extensions.DependencyInjection;

// Startup.cs oder ServiceModule
public void ConfigureServices(IServiceCollection services)
{
    // 1. DataStore Provider
    services.AddSingleton<IDataStoreProvider, DataStoreProvider>();
    
    // 2. Repository Factory
    services.AddSingleton<IRepositoryFactory, RepositoryFactory>();
    
    // 3. ViewModelFactory
    services.AddViewModelFactory<Customer, CustomerViewModel>();
    
    // 4. EqualityComparer
    services.AddSingleton<IEqualityComparer<Customer>>(
        new FallbackEqualsComparer<Customer>());
    
    // 5. ViewModel erstellt CollectionViewModel im Constructor
    services.AddTransient<MainViewModel>();
}
```

## Bidirektionale Synchronisation

### Synchronisation im Detail

```
                     ????????????????
                     ?  DataStore   ?
                     ?  (Models)    ?
                     ????????????????
                            ?
                  ?????????????????????
                  ?                   ?
        Add/Remove/Clear    CollectionChanged Event
                  ?                   ?
                  ?                   ?
         ??????????????????????????????????
         ?   CollectionViewModel          ?
         ?   - ViewModelFactory           ?
         ?   - ModelToViewModelMap        ?
         ??????????????????????????????????
                      ?
            Creates/Disposes ViewModels
                      ?
                      ?
         ??????????????????????????????
         ?   Items (ReadOnly)         ?
         ?   ObservableCollection     ?
         ?   (ViewModels)             ?
         ??????????????????????????????
                      ?
                  UI Binding
                      ?
                      ?
              ?????????????????
              ?   ListView    ?
              ?????????????????
```

### CollectionChanged-Events

**Add-Event:**
```csharp
dataStore.Add(customer);

// OnDataStoreChanged wird aufgerufen:
// 1. Prüfe: Ist Model bereits in Map? (Duplikat-Check)
// 2. ViewModelFactory.Create(model)
// 3. _modelToViewModelMap[model] = viewModel
// 4. _viewModels.Add(viewModel)
// 5. OnPropertyChanged(nameof(Count))
// ? UI zeigt neues Item
```

**Remove-Event:**
```csharp
dataStore.Remove(customer);

// OnDataStoreChanged wird aufgerufen:
// 1. Lookup in _modelToViewModelMap
// 2. Ist ViewModel == SelectedItem? ? SelectedItem = null
// 3. _viewModels.Remove(viewModel)
// 4. _modelToViewModelMap.Remove(model)
// 5. OnViewModelRemoving(viewModel) (Hook für Subclasses)
// 6. viewModel.Dispose() (falls IDisposable)
// 7. OnPropertyChanged(nameof(Count))
// ? UI-Update und Cleanup
```

**Reset-Event (Clear):**
```csharp
dataStore.Clear();

// OnDataStoreChanged wird aufgerufen:
// 1. SelectedItem = null (immer!)
// 2. Für jedes ViewModel in Map:
//    - _viewModels.Remove(viewModel)
//    - viewModel.Dispose()
// 3. _modelToViewModelMap.Clear()
// 4. OnPropertyChanged(nameof(Count))
// ? Alle ViewModels entfernt und disposed
```

**Replace-Event:**
```csharp
// Nicht direkt via API, aber bei ObservableCollection[index] = newItem
// 1. Remove old ViewModel (siehe Remove-Event)
// 2. Add new ViewModel (siehe Add-Event)
```

## ViewModel-Lifecycle

### Lifecycle-Phasen

```
???????????????????????????????????????????????????????????
? 1. CREATION                                             ?
?    DataStore.Add(model)                                 ?
?    ?                                                    ?
?    ViewModelFactory.Create(model)                       ?
?    ?                                                    ?
?    _modelToViewModelMap[model] = viewModel              ?
?    ?                                                    ?
?    _viewModels.Add(viewModel)                           ?
???????????????????????????????????????????????????????????
                         ?
                         ?
???????????????????????????????????????????????????????????
? 2. ACTIVE                                               ?
?    ViewModel ist in Items                               ?
?    UI kann binden an Properties                         ?
?    PropertyChanged-Events werden gefeuert               ?
???????????????????????????????????????????????????????????
                         ?
                         ?
???????????????????????????????????????????????????????????
? 3. REMOVAL                                              ?
?    DataStore.Remove(model)                              ?
?    ?                                                    ?
?    _viewModels.Remove(viewModel)                        ?
?    ?                                                    ?
?    _modelToViewModelMap.Remove(model)                   ?
?    ?                                                    ?
?    OnViewModelRemoving(viewModel) [Hook]                ?
?    ?                                                    ?
?    viewModel.Dispose() [falls IDisposable]              ?
???????????????????????????????????????????????????????????
```

### Hook: OnViewModelRemoving

Für benutzerdefinierte Cleanup-Logik:

```csharp
public class MyCollectionViewModel : CollectionViewModel<Customer, CustomerViewModel>
{
    public MyCollectionViewModel(/* ... */) : base(/* ... */) { }
    
    protected override void OnViewModelRemoving(CustomerViewModel viewModel)
    {
        // Custom Cleanup vor Dispose
        viewModel.CancelPendingOperations();
        viewModel.UnsubscribeFromEvents();
        
        // Base aufrufen ist optional (base macht nichts)
        base.OnViewModelRemoving(viewModel);
    }
}
```

### Dispose-Pattern

```csharp
public void Dispose()
{
    if (_disposed) return;
    _disposed = true;

    // 1. DataStore-Event abmelden
    if (_dataStore != null)
    {
        ((INotifyCollectionChanged)_dataStore.Items).CollectionChanged -= OnDataStoreChanged;
    }

    // 2. Alle ViewModels disposen
    if (_modelToViewModelMap != null)
    {
        foreach (var viewModel in _modelToViewModelMap.Values)
        {
            DisposeViewModel(viewModel);
        }
        _modelToViewModelMap.Clear();
    }
    
    // 3. ViewModels-Collection leeren
    _viewModels.Clear();
}
```

## Best Practices

### ? Do's

**1. IEqualityComparer registrieren:**
```csharp
// Wichtig für ModelToViewModelMap!
services.AddSingleton<IEqualityComparer<Customer>>(
    new FallbackEqualsComparer<Customer>());

var collectionVM = new CollectionViewModel<Customer, CustomerViewModel>(
    dataStore,
    factory,
    comparer);  // ? Comparer übergeben
```

**2. ViewModelFactory verwenden:**
```csharp
// ? Gut: DI-basierte Factory
services.AddViewModelFactory<Customer, CustomerViewModel>();
var factory = sp.GetRequiredService<IViewModelFactory<Customer, CustomerViewModel>>();

// ? Schlecht: Manuelle Erstellung
var collectionVM = new CollectionViewModel<Customer, CustomerViewModel>(
    dataStore,
    new ManualFactory(),  // Keine DI!
    comparer);
```

**3. Dispose aufrufen:**
```csharp
public class MainViewModel : IDisposable
{
    private readonly CollectionViewModel<Customer, CustomerViewModel> _customers;
    
    public void Dispose()
    {
        _customers.Dispose();  // ? Wichtig!
    }
}
```

**4. SelectedItem für UI-Binding:**
```csharp
// XAML
<ListBox ItemsSource="{Binding Customers}"
         SelectedItem="{Binding SelectedCustomer}"/>

// ViewModel
public CustomerViewModel? SelectedCustomer
{
    get => _customers.SelectedItem;
    set => _customers.SelectedItem = value;
}
```

### ? Don'ts

**1. Keine direkte Items-Mutation:**
```csharp
// ? Schlecht: ReadOnlyObservableCollection ist read-only!
collectionVM.Items.Add(viewModel);  // Compile-Error!

// ? Gut: Via AddModel
collectionVM.AddModel(customer);
```

**2. Kein manuelles ViewModel-Disposal:**
```csharp
// ? Schlecht: CollectionViewModel managt Lifecycle!
var viewModel = collectionVM.Items.First();
viewModel.Dispose();  // Kann zu Problemen führen!

// ? Gut: Via RemoveViewModel
collectionVM.RemoveViewModel(viewModel);  // Entfernt und disposed automatisch
```

**3. Keine konkurrierenden Änderungen:**
```csharp
// ? Schlecht: DataStore und CollectionViewModel parallel ändern
Task.Run(() => dataStore.Add(customer1));
collectionVM.AddModel(customer2);  // Race Condition!

// ? Gut: Alle Änderungen über eine Schnittstelle
collectionVM.AddModel(customer1);
collectionVM.AddModel(customer2);
```

## Beispiele

### Beispiel 1: Einfache Customer-Liste

```csharp
public class CustomerListViewModel : IDisposable
{
    private readonly CollectionViewModel<Customer, CustomerViewModel> _customers;
    
    public CustomerListViewModel(
        IDataStoreProvider provider,
        IRepositoryFactory repositoryFactory,
        IViewModelFactory<Customer, CustomerViewModel> factory,
        IEqualityComparer<Customer> comparer)
    {
        var dataStore = provider.GetPersistent<Customer>(
            repositoryFactory,
            autoLoad: true);
        
        _customers = new CollectionViewModel<Customer, CustomerViewModel>(
            dataStore,
            factory,
            comparer);
    }
    
    public ReadOnlyObservableCollection<CustomerViewModel> Customers 
        => _customers.Items;
    
    public CustomerViewModel? SelectedCustomer
    {
        get => _customers.SelectedItem;
        set => _customers.SelectedItem = value;
    }
    
    public void AddCustomer(string name, string email)
    {
        var customer = new Customer 
        { 
            Id = 0, 
            Name = name, 
            Email = email 
        };
        _customers.AddModel(customer);
        // ? Automatisch persistiert via DataStore
        // ? Automatisch ViewModel erstellt
        // ? Automatisch UI aktualisiert
    }
    
    public void DeleteSelectedCustomer()
    {
        if (SelectedCustomer != null)
        {
            _customers.RemoveViewModel(SelectedCustomer);
            // ? Automatisch aus DataStore entfernt
            // ? Automatisch ViewModel disposed
            // ? SelectedItem = null
        }
    }
    
    public void Dispose()
    {
        _customers.Dispose();
    }
}
```

### Beispiel 2: Mit Filterung

```csharp
public class FilteredCustomerViewModel : INotifyPropertyChanged
{
    private readonly CollectionViewModel<Customer, CustomerViewModel> _allCustomers;
    private readonly ObservableCollection<CustomerViewModel> _filteredCustomers;
    private string _searchText = "";
    
    public FilteredCustomerViewModel(/* DI parameters */)
    {
        _allCustomers = new CollectionViewModel<Customer, CustomerViewModel>(
            dataStore,
            factory,
            comparer);
        
        _filteredCustomers = new ObservableCollection<CustomerViewModel>();
        FilteredCustomers = new ReadOnlyObservableCollection<CustomerViewModel>(_filteredCustomers);
        
        // Initiales Filtern
        ApplyFilter();
        
        // Bei Änderungen in _allCustomers neu filtern
        ((INotifyCollectionChanged)_allCustomers.Items).CollectionChanged += 
            (s, e) => ApplyFilter();
    }
    
    public ReadOnlyObservableCollection<CustomerViewModel> FilteredCustomers { get; }
    
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }
    }
    
    private void ApplyFilter()
    {
        _filteredCustomers.Clear();
        
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allCustomers.Items
            : _allCustomers.Items.Where(vm => 
                vm.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        
        foreach (var item in filtered)
        {
            _filteredCustomers.Add(item);
        }
    }
}
```

### Beispiel 3: Mit Custom Lifecycle-Hook

```csharp
public class CustomCollectionViewModel : CollectionViewModel<Order, OrderViewModel>
{
    private readonly INotificationService _notificationService;
    
    public CustomCollectionViewModel(
        IDataStore<Order> dataStore,
        IViewModelFactory<Order, OrderViewModel> factory,
        IEqualityComparer<Order> comparer,
        INotificationService notificationService)
        : base(dataStore, factory, comparer)
    {
        _notificationService = notificationService;
    }
    
    protected override void OnViewModelRemoving(OrderViewModel viewModel)
    {
        // Benachrichtigung vor Removal
        _notificationService.Notify(
            $"Order {viewModel.Model.Id} wird entfernt");
        
        // Custom Cleanup
        viewModel.CancelPendingOperations();
        viewModel.UnsubscribeFromEvents();
        
        base.OnViewModelRemoving(viewModel);
    }
}
```

## Siehe auch

- ?? [ViewModelBase](ViewModelBase.md) - Basisklasse für ViewModels
- ?? [EditableCollectionViewModel](EditableCollectionViewModel.md) - Mit Commands
- ?? [ViewModelFactory](ViewModelFactory.md) - DI-basierte ViewModel-Erstellung
- ?? [Getting Started](Getting-Started.md) - Schnellstart-Guide
- ?? [DataStore Provider](../../DataToolKit/Docs/DataStore-Provider.md) - DataStore-Management
