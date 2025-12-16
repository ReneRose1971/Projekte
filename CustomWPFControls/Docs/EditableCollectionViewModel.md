# EditableCollectionViewModel<TModel, TViewModel>

Erweitert CollectionViewModel um Commands (Add, Delete, Clear, Edit) für vollständige CRUD-Operationen in der UI.

## ?? Inhaltsverzeichnis

- [Übersicht](#übersicht)
- [Features](#features)
- [Commands](#commands)
- [Verwendung](#verwendung)
- [XAML-Binding](#xaml-binding)
- [Best Practices](#best-practices)
- [Beispiele](#beispiele)

## Übersicht

`EditableCollectionViewModel<TModel, TViewModel>` erweitert [CollectionViewModel](CollectionViewModel.md) um **ICommand-Properties** für CRUD-Operationen. Sie bietet:

- ? **AddCommand** - Fügt neue Elemente hinzu
- ? **DeleteCommand** - Löscht ausgewähltes Element
- ? **ClearCommand** - Löscht alle Elemente
- ? **EditCommand** - Bearbeitet ausgewähltes Element
- ? **CanExecute-Logic** - Automatische Command-Validation
- ? **Delegate-Pattern** - CreateModel & EditModel für Flexibilität

### Definition

```csharp
namespace CustomWPFControls.ViewModels;

public class EditableCollectionViewModel<TModel, TViewModel> 
    : CollectionViewModel<TModel, TViewModel>
    where TModel : class
    where TViewModel : class, IViewModelWrapper<TModel>
{
    public Func<TModel>? CreateModel { get; set; }
    public Action<TModel>? EditModel { get; set; }
    
    public ICommand AddCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand EditCommand { get; }
    
    public EditableCollectionViewModel(
        IDataStore<TModel> dataStore,
        IViewModelFactory<TModel, TViewModel> viewModelFactory,
        IEqualityComparer<TModel> modelComparer);
}
```

## Features

### 1. Commands für CRUD-Operationen

**AddCommand:**
- **Execute:** Ruft `CreateModel()` auf ? fügt Model zu DataStore hinzu
- **CanExecute:** `CreateModel != null`

**DeleteCommand:**
- **Execute:** Entfernt `SelectedItem` aus DataStore
- **CanExecute:** `SelectedItem != null`

**ClearCommand:**
- **Execute:** Leert DataStore (alle Items)
- **CanExecute:** `Count > 0`

**EditCommand:**
- **Execute:** Ruft `EditModel(SelectedItem.Model)` auf
- **CanExecute:** `SelectedItem != null && EditModel != null`

### 2. Delegate-Pattern

**CreateModel - Factory für neue Models:**
```csharp
editableVM.CreateModel = () => new Customer
{
    Id = 0,
    Name = "Neuer Kunde",
    Email = "",
    CreatedAt = DateTime.Now
};

// Oder mit Dialog:
editableVM.CreateModel = () =>
{
    var dialog = new NewCustomerDialog();
    return dialog.ShowDialog() == true 
        ? dialog.Customer 
        : null;
};
```

**EditModel - Callback für Bearbeitung:**
```csharp
editableVM.EditModel = (customer) =>
{
    var dialog = new EditCustomerDialog(customer);
    if (dialog.ShowDialog() == true)
    {
        // Dialog hat Model bereits geändert
        // PropertyChanged-Tracking persistiert automatisch
    }
};

// Oder ohne Dialog:
editableVM.EditModel = (customer) =>
{
    NavigateToEditPage(customer);
};
```

### 3. Automatische UI-Updates

**WPF CommandManager:**
- Commands implementieren `CommandManager.RequerySuggested`
- CanExecute wird automatisch neu evaluiert bei:
  - SelectedItem-Änderung
  - Count-Änderung
  - Focus-Änderung
- Buttons werden automatisch enabled/disabled

**Beispiel:**
```xml
<Button Command="{Binding DeleteCommand}" Content="Löschen"/>
<!-- Button ist disabled, wenn SelectedItem == null -->
<!-- Button ist enabled, sobald ein Item ausgewählt wird -->
```

## Commands

### AddCommand

```csharp
public ICommand AddCommand { get; }

// Implementation:
_addCommand = new RelayCommand(_ =>
{
    if (CreateModel == null)
        throw new InvalidOperationException("CreateModel muss gesetzt sein.");

    var model = CreateModel();
    AddModel(model);  // Aus CollectionViewModel
}, _ => CreateModel != null);
```

**Verwendung:**
```csharp
// Setup
editableVM.CreateModel = () => new Customer { Id = 0, Name = "Neu" };

// UI-Binding
<Button Command="{Binding AddCommand}" Content="Hinzufügen"/>

// Oder programmatisch
if (editableVM.AddCommand.CanExecute(null))
{
    editableVM.AddCommand.Execute(null);
}
```

**CanExecute-Logik:**
- ? `CreateModel != null` ? Command ist enabled
- ? `CreateModel == null` ? Command ist disabled

### DeleteCommand

```csharp
public ICommand DeleteCommand { get; }

// Implementation:
_deleteCommand = new RelayCommand(_ =>
{
    if (SelectedItem != null)
    {
        RemoveViewModel(SelectedItem);  // Aus CollectionViewModel
    }
}, _ => SelectedItem != null);
```

**Verwendung:**
```csharp
<Button Command="{Binding DeleteCommand}" Content="Löschen"/>

// Oder mit Bestätigung
editableVM.DeleteCommand = new RelayCommand(_ =>
{
    if (SelectedItem != null && ConfirmDelete())
    {
        RemoveViewModel(SelectedItem);
    }
}, _ => SelectedItem != null);
```

**CanExecute-Logik:**
- ? `SelectedItem != null` ? Command ist enabled
- ? `SelectedItem == null` ? Command ist disabled

### ClearCommand

```csharp
public ICommand ClearCommand { get; }

// Implementation:
_clearCommand = new RelayCommand(_ =>
{
    Clear();  // Aus CollectionViewModel
}, _ => Count > 0);
```

**Verwendung:**
```csharp
<Button Command="{Binding ClearCommand}" Content="Alle löschen"/>

// Oder mit Bestätigung
editableVM.ClearCommand = new RelayCommand(_ =>
{
    if (MessageBox.Show("Wirklich alle löschen?", "Bestätigung", 
        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
    {
        Clear();
    }
}, _ => Count > 0);
```

**CanExecute-Logik:**
- ? `Count > 0` ? Command ist enabled
- ? `Count == 0` ? Command ist disabled (keine Items vorhanden)

### EditCommand

```csharp
public ICommand EditCommand { get; }

// Implementation:
_editCommand = new RelayCommand(_ =>
{
    if (SelectedItem != null && EditModel != null)
    {
        EditModel(SelectedItem.Model);
    }
}, _ => SelectedItem != null && EditModel != null);
```

**Verwendung:**
```csharp
// Setup
editableVM.EditModel = (customer) =>
{
    var dialog = new EditCustomerDialog(customer);
    dialog.ShowDialog();
};

// UI-Binding
<Button Command="{Binding EditCommand}" Content="Bearbeiten"/>

// Oder mit MouseDoubleClick
<ListBox ItemsSource="{Binding Customers}"
         SelectedItem="{Binding SelectedCustomer}">
    <ListBox.InputBindings>
        <MouseBinding Gesture="LeftDoubleClick" 
                      Command="{Binding EditCommand}"/>
    </ListBox.InputBindings>
</ListBox>
```

**CanExecute-Logik:**
- ? `SelectedItem != null && EditModel != null` ? Command ist enabled
- ? Andernfalls ? Command ist disabled

## Verwendung

### Grundlegende Verwendung

```csharp
using CustomWPFControls.ViewModels;
using DataToolKit.Abstractions.DataStores;

public class CustomerListViewModel : IDisposable
{
    private readonly EditableCollectionViewModel<Customer, CustomerViewModel> _customers;
    
    public CustomerListViewModel(
        IDataStoreProvider provider,
        IRepositoryFactory repositoryFactory,
        IViewModelFactory<Customer, CustomerViewModel> factory,
        IEqualityComparer<Customer> comparer)
    {
        var dataStore = provider.GetPersistent<Customer>(
            repositoryFactory,
            autoLoad: true);
        
        _customers = new EditableCollectionViewModel<Customer, CustomerViewModel>(
            dataStore,
            factory,
            comparer);
        
        // Commands konfigurieren
        SetupCommands();
    }
    
    private void SetupCommands()
    {
        // CreateModel: Factory für neue Kunden
        _customers.CreateModel = () => new Customer
        {
            Id = 0,
            Name = "Neuer Kunde",
            Email = "",
            CreatedAt = DateTime.Now
        };
        
        // EditModel: Dialog für Bearbeitung
        _customers.EditModel = (customer) =>
        {
            var dialog = new EditCustomerDialog(customer);
            dialog.ShowDialog();
        };
    }
    
    // Properties für UI-Binding
    public ReadOnlyObservableCollection<CustomerViewModel> Customers 
        => _customers.Items;
    
    public CustomerViewModel? SelectedCustomer
    {
        get => _customers.SelectedItem;
        set => _customers.SelectedItem = value;
    }
    
    // Commands für UI-Binding
    public ICommand AddCommand => _customers.AddCommand;
    public ICommand DeleteCommand => _customers.DeleteCommand;
    public ICommand ClearCommand => _customers.ClearCommand;
    public ICommand EditCommand => _customers.EditCommand;
    
    public void Dispose()
    {
        _customers.Dispose();
    }
}
```

### Mit Dialog-Integration

```csharp
public class ProductListViewModel
{
    private readonly EditableCollectionViewModel<Product, ProductViewModel> _products;
    private readonly IDialogService _dialogService;
    
    public ProductListViewModel(
        IDataStore<Product> dataStore,
        IViewModelFactory<Product, ProductViewModel> factory,
        IEqualityComparer<Product> comparer,
        IDialogService dialogService)
    {
        _products = new EditableCollectionViewModel<Product, ProductViewModel>(
            dataStore,
            factory,
            comparer);
        
        _dialogService = dialogService;
        
        SetupCommands();
    }
    
    private void SetupCommands()
    {
        // CreateModel: Dialog für neues Produkt
        _products.CreateModel = () =>
        {
            var viewModel = new ProductDialogViewModel();
            var result = _dialogService.ShowDialog(viewModel);
            
            return result == true ? viewModel.ToModel() : null;
        };
        
        // EditModel: Dialog für Bearbeitung
        _products.EditModel = (product) =>
        {
            var viewModel = new ProductDialogViewModel(product);
            var result = _dialogService.ShowDialog(viewModel);
            
            if (result == true)
            {
                viewModel.UpdateModel(product);
                // PropertyChanged-Tracking persistiert automatisch
            }
        };
    }
    
    public ReadOnlyObservableCollection<ProductViewModel> Products => _products.Items;
    public ICommand AddCommand => _products.AddCommand;
    public ICommand DeleteCommand => _products.DeleteCommand;
    public ICommand EditCommand => _products.EditCommand;
}
```

## XAML-Binding

### ListBox mit Commands

```xml
<Window x:Class="MyApp.Views.CustomerListView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- Liste mit Doppelklick zum Bearbeiten -->
        <ListBox Grid.Row="0"
                 ItemsSource="{Binding Customers}"
                 SelectedItem="{Binding SelectedCustomer}">
            <ListBox.InputBindings>
                <MouseBinding Gesture="LeftDoubleClick" 
                              Command="{Binding EditCommand}"/>
            </ListBox.InputBindings>
            <ListBox.ItemTemplate>
                <DataTemplate>
                    <StackPanel>
                        <TextBlock Text="{Binding Name}" FontWeight="Bold"/>
                        <TextBlock Text="{Binding Email}" Foreground="Gray"/>
                    </StackPanel>
                </DataTemplate>
            </ListBox.ItemTemplate>
        </ListBox>
        
        <!-- Command-Buttons -->
        <StackPanel Grid.Row="1" Orientation="Horizontal" Margin="5">
            <Button Command="{Binding AddCommand}" 
                    Content="Hinzufügen" 
                    Width="100" 
                    Margin="0,0,5,0"/>
            <Button Command="{Binding EditCommand}" 
                    Content="Bearbeiten" 
                    Width="100" 
                    Margin="0,0,5,0"/>
            <Button Command="{Binding DeleteCommand}" 
                    Content="Löschen" 
                    Width="100" 
                    Margin="0,0,5,0"/>
            <Button Command="{Binding ClearCommand}" 
                    Content="Alle löschen" 
                    Width="100"/>
        </StackPanel>
    </Grid>
</Window>
```

### DataGrid mit ContextMenu

```xml
<DataGrid ItemsSource="{Binding Orders}"
          SelectedItem="{Binding SelectedOrder}"
          AutoGenerateColumns="False">
    
    <!-- Context Menu -->
    <DataGrid.ContextMenu>
        <ContextMenu>
            <MenuItem Header="Hinzufügen" 
                      Command="{Binding AddCommand}"/>
            <MenuItem Header="Bearbeiten" 
                      Command="{Binding EditCommand}"/>
            <Separator/>
            <MenuItem Header="Löschen" 
                      Command="{Binding DeleteCommand}"/>
            <MenuItem Header="Alle löschen" 
                      Command="{Binding ClearCommand}"/>
        </ContextMenu>
    </DataGrid.ContextMenu>
    
    <DataGrid.Columns>
        <DataGridTextColumn Header="Bestellnr." Binding="{Binding OrderNumber}"/>
        <DataGridTextColumn Header="Kunde" Binding="{Binding CustomerName}"/>
        <DataGridTextColumn Header="Betrag" Binding="{Binding Total, StringFormat=C}"/>
    </DataGrid.Columns>
</DataGrid>
```

### Toolbar mit Icons

```xml
<ToolBar>
    <Button Command="{Binding AddCommand}" ToolTip="Hinzufügen">
        <Image Source="/Images/add.png" Width="16" Height="16"/>
    </Button>
    <Button Command="{Binding EditCommand}" ToolTip="Bearbeiten">
        <Image Source="/Images/edit.png" Width="16" Height="16"/>
    </Button>
    <Separator/>
    <Button Command="{Binding DeleteCommand}" ToolTip="Löschen">
        <Image Source="/Images/delete.png" Width="16" Height="16"/>
    </Button>
    <Button Command="{Binding ClearCommand}" ToolTip="Alle löschen">
        <Image Source="/Images/clear.png" Width="16" Height="16"/>
    </Button>
</ToolBar>
```

## Best Practices

### ? Do's

**1. CreateModel und EditModel setzen:**
```csharp
// ? Gut: Commands funktionieren
editableVM.CreateModel = () => new Customer { Id = 0 };
editableVM.EditModel = (c) => OpenEditDialog(c);

// ? Schlecht: Commands sind disabled
// (CreateModel und EditModel sind null)
```

**2. Null-Check in CreateModel:**
```csharp
// ? Gut: Gibt null zurück bei Abbruch
editableVM.CreateModel = () =>
{
    var dialog = new NewCustomerDialog();
    return dialog.ShowDialog() == true ? dialog.Customer : null;
};

// AddModel prüft automatisch auf null
```

**3. PropertyChanged-Tracking nutzen:**
```csharp
// ? Gut: Model-Änderungen werden automatisch persistiert
editableVM.EditModel = (customer) =>
{
    var dialog = new EditCustomerDialog(customer);
    if (dialog.ShowDialog() == true)
    {
        // customer.Name etc. wurde bereits geändert
        // PropertyChanged-Tracking persistiert automatisch
    }
};
```

**4. Bestätigungsdialoge für kritische Operationen:**
```csharp
// ? Gut: Bestätigung vor ClearCommand
var customClearCommand = new RelayCommand(_ =>
{
    var result = MessageBox.Show(
        "Wirklich alle Einträge löschen?",
        "Bestätigung",
        MessageBoxButton.YesNo,
        MessageBoxImage.Warning);
    
    if (result == MessageBoxResult.Yes)
    {
        _editableVM.Clear();
    }
}, _ => _editableVM.Count > 0);
```

### ? Don'ts

**1. Keine Exception in CreateModel:**
```csharp
// ? Schlecht: Exception bei Abbruch
editableVM.CreateModel = () =>
{
    var dialog = new NewCustomerDialog();
    if (dialog.ShowDialog() != true)
        throw new OperationCanceledException();  // ? Schlecht!
    
    return dialog.Customer;
};

// ? Gut: Gibt null zurück
editableVM.CreateModel = () =>
{
    var dialog = new NewCustomerDialog();
    return dialog.ShowDialog() == true ? dialog.Customer : null;
};
```

**2. Keine Geschäftslogik in Delegates:**
```csharp
// ? Schlecht: Geschäftslogik in CreateModel
editableVM.CreateModel = () =>
{
    var customer = new Customer { Id = 0 };
    customer.CalculateCreditLimit();  // ? Geschäftslogik!
    customer.AssignToDefaultSegment();  // ? Geschäftslogik!
    return customer;
};

// ? Gut: Geschäftslogik im Service
editableVM.CreateModel = () => _customerService.CreateNew();
```

**3. Kein Command-Override ohne CanExecute:**
```csharp
// ? Schlecht: Custom Command ohne CanExecute
var customDeleteCommand = new RelayCommand(_ =>
{
    _editableVM.RemoveViewModel(_editableVM.SelectedItem);
});  // Kein CanExecute! Button immer enabled

// ? Gut: Mit CanExecute
var customDeleteCommand = new RelayCommand(
    _ => _editableVM.RemoveViewModel(_editableVM.SelectedItem),
    _ => _editableVM.SelectedItem != null);  // CanExecute!
```

## Beispiele

### Beispiel 1: Vollständige Customer-Verwaltung

```csharp
public class CustomerManagementViewModel : IDisposable
{
    private readonly EditableCollectionViewModel<Customer, CustomerViewModel> _customers;
    private readonly IDialogService _dialogService;
    
    public CustomerManagementViewModel(
        IDataStoreProvider provider,
        IRepositoryFactory repositoryFactory,
        IViewModelFactory<Customer, CustomerViewModel> factory,
        IEqualityComparer<Customer> comparer,
        IDialogService dialogService)
    {
        var dataStore = provider.GetPersistent<Customer>(repositoryFactory, autoLoad: true);
        
        _customers = new EditableCollectionViewModel<Customer, CustomerViewModel>(
            dataStore,
            factory,
            comparer);
        
        _dialogService = dialogService;
        
        SetupCommands();
    }
    
    private void SetupCommands()
    {
        _customers.CreateModel = CreateNewCustomer;
        _customers.EditModel = EditExistingCustomer;
    }
    
    private Customer CreateNewCustomer()
    {
        var viewModel = new CustomerEditViewModel();
        var result = _dialogService.ShowDialog("Neuer Kunde", viewModel);
        
        if (result == true)
        {
            return new Customer
            {
                Id = 0,
                Name = viewModel.Name,
                Email = viewModel.Email,
                CreatedAt = DateTime.Now
            };
        }
        
        return null;  // Abbruch
    }
    
    private void EditExistingCustomer(Customer customer)
    {
        var viewModel = new CustomerEditViewModel(customer);
        var result = _dialogService.ShowDialog("Kunde bearbeiten", viewModel);
        
        if (result == true)
        {
            customer.Name = viewModel.Name;
            customer.Email = viewModel.Email;
            // PropertyChanged-Tracking persistiert automatisch
        }
    }
    
    // Properties für UI
    public ReadOnlyObservableCollection<CustomerViewModel> Customers => _customers.Items;
    public CustomerViewModel? SelectedCustomer
    {
        get => _customers.SelectedItem;
        set => _customers.SelectedItem = value;
    }
    
    // Commands für UI
    public ICommand AddCommand => _customers.AddCommand;
    public ICommand EditCommand => _customers.EditCommand;
    public ICommand DeleteCommand => _customers.DeleteCommand;
    public ICommand ClearCommand => _customers.ClearCommand;
    
    public void Dispose() => _customers.Dispose();
}
```

### Beispiel 2: Mit Validierung

```csharp
public class ValidatingEditableViewModel : IDisposable
{
    private readonly EditableCollectionViewModel<Product, ProductViewModel> _products;
    private readonly IValidationService _validationService;
    
    public ValidatingEditableViewModel(
        IDataStore<Product> dataStore,
        IViewModelFactory<Product, ProductViewModel> factory,
        IEqualityComparer<Product> comparer,
        IValidationService validationService)
    {
        _products = new EditableCollectionViewModel<Product, ProductViewModel>(
            dataStore,
            factory,
            comparer);
        
        _validationService = validationService;
        
        SetupCommandsWithValidation();
    }
    
    private void SetupCommandsWithValidation()
    {
        _products.CreateModel = () =>
        {
            var product = new Product { Id = 0 };
            
            // Validierung vor Rückgabe
            var errors = _validationService.Validate(product);
            if (errors.Any())
            {
                ShowValidationErrors(errors);
                return null;  // Abbruch
            }
            
            return product;
        };
        
        _products.EditModel = (product) =>
        {
            var originalState = product.Clone();  // Backup
            
            var dialog = new EditProductDialog(product);
            if (dialog.ShowDialog() == true)
            {
                // Validierung nach Bearbeitung
                var errors = _validationService.Validate(product);
                if (errors.Any())
                {
                    ShowValidationErrors(errors);
                    product.RestoreFrom(originalState);  // Rollback
                }
            }
        };
    }
    
    public ReadOnlyObservableCollection<ProductViewModel> Products => _products.Items;
    public ICommand AddCommand => _products.AddCommand;
    public ICommand EditCommand => _products.EditCommand;
    public ICommand DeleteCommand => _products.DeleteCommand;
    
    public void Dispose() => _products.Dispose();
}
```

### Beispiel 3: Mit Custom Commands

```csharp
public class CustomCommandsViewModel
{
    private readonly EditableCollectionViewModel<Order, OrderViewModel> _orders;
    
    public CustomCommandsViewModel(/* DI parameters */)
    {
        _orders = new EditableCollectionViewModel<Order, OrderViewModel>(
            dataStore,
            factory,
            comparer);
        
        // Standard Commands verwenden
        _orders.CreateModel = () => new Order { Id = 0 };
        _orders.EditModel = (o) => OpenEditDialog(o);
        
        // Custom Commands erstellen
        DuplicateCommand = new RelayCommand(
            _ => DuplicateSelectedOrder(),
            _ => _orders.SelectedItem != null);
        
        ExportCommand = new RelayCommand(
            _ => ExportOrders(),
            _ => _orders.Count > 0);
    }
    
    // Standard Commands
    public ICommand AddCommand => _orders.AddCommand;
    public ICommand EditCommand => _orders.EditCommand;
    public ICommand DeleteCommand => _orders.DeleteCommand;
    
    // Custom Commands
    public ICommand DuplicateCommand { get; }
    public ICommand ExportCommand { get; }
    
    private void DuplicateSelectedOrder()
    {
        if (_orders.SelectedItem != null)
        {
            var original = _orders.SelectedItem.Model;
            var duplicate = new Order
            {
                Id = 0,  // Neue ID
                CustomerName = original.CustomerName,
                Total = original.Total,
                // ... andere Properties kopieren
            };
            _orders.AddModel(duplicate);
        }
    }
    
    private void ExportOrders()
    {
        // Export-Logik
    }
}
```

## Siehe auch

- ?? [CollectionViewModel](CollectionViewModel.md) - Basisklasse ohne Commands
- ?? [ViewModelBase](ViewModelBase.md) - Basisklasse für ViewModels
- ?? [ViewModelFactory](ViewModelFactory.md) - DI-basierte ViewModel-Erstellung
- ?? [Getting Started](Getting-Started.md) - Schnellstart-Guide
- ?? [Best Practices](Best-Practices.md) - Allgemeine Tipps
