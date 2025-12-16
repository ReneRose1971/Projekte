# CustomWPFControls

MVVM-Framework für WPF mit DataStore-Integration, automatischer Synchronisation und PropertyChanged-Support via Fody.

## ?? Inhaltsverzeichnis

- [Überblick](#überblick)
- [Features](#features)
- [Installation](#installation)
- [Schnellstart](#schnellstart)
- [Dokumentation](#dokumentation)
- [Beispiele](#beispiele)
- [Tests](#tests)

---

## ?? Überblick

**CustomWPFControls** ist ein leistungsstarkes MVVM-Framework für WPF-Anwendungen, das:
- ? **Bidirektionale Synchronisation** zwischen DataStore und ViewModels bietet
- ? **Automatische PropertyChanged-Events** via Fody.PropertyChanged implementiert
- ? **ViewModelFactory** für DI-basierte ViewModel-Erstellung bereitstellt
- ? **CollectionViewModel** für einfache Collection-Verwaltung anbietet
- ? **EditableCollectionViewModel** mit Commands (Add, Delete, Edit, Clear) erweitert

---

## ? Features

### **1. ViewModelBase<TModel>**
Basisklasse für alle ViewModels mit automatischem PropertyChanged-Support.

```csharp
public class CustomerViewModel : ViewModelBase<Customer>
{
    public CustomerViewModel(Customer model) : base(model) { }
    
    // Domain-Properties (delegiert an Model)
    public string Name => Model.Name;
    
    // UI-Properties (mit PropertyChanged)
    public bool IsSelected { get; set; }
}
```

### **2. CollectionViewModel<TModel, TViewModel>**
Bidirektionale Synchronisation zwischen DataStore und ViewModels.

```csharp
var viewModel = new CollectionViewModel<Customer, CustomerViewModel>(
    dataStore,
    viewModelFactory,
    comparer);

// Items sind automatisch synchronisiert!
viewModel.AddModel(new Customer { Name = "Alice" });
// ? Automatisch ViewModel erstellt
// ? Automatisch in Items sichtbar
```

### **3. EditableCollectionViewModel<TModel, TViewModel>**
Erweitert CollectionViewModel um Commands (Add, Delete, Edit, Clear).

```csharp
var viewModel = new EditableCollectionViewModel<Customer, CustomerViewModel>(
    dataStore, factory, comparer);

viewModel.CreateModel = () => new Customer();
viewModel.EditModel = customer => OpenEditDialog(customer);

// Commands sind bereit:
viewModel.AddCommand.Execute(null);
viewModel.DeleteCommand.Execute(null);
```

### **4. ViewModelFactory<TModel, TViewModel>**
DI-basierte Factory für ViewModel-Erstellung.

```csharp
services.AddViewModelFactory<Customer, CustomerViewModel>();

var factory = serviceProvider.GetRequiredService<IViewModelFactory<Customer, CustomerViewModel>>();
var viewModel = factory.Create(customer);
```

---

## ?? Installation

### **Voraussetzungen:**
- .NET 8.0 oder höher
- WPF-Projekt

### **NuGet-Pakete:**
```bash
dotnet add package PropertyChanged.Fody
dotnet add package Fody
dotnet add package Microsoft.Extensions.DependencyInjection
```

### **Projekt-Referenzen:**
```xml
<ProjectReference Include="..\DataToolKit\DataToolKit.csproj" />
<ProjectReference Include="..\Common.BootStrap\Common.BootStrap.csproj" />
```

---

## ?? Schnellstart

[Siehe vollständigen Schnellstart-Guide](Docs/Getting-Started.md)

### **1. Model definieren**

```csharp
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}
```

### **2. ViewModel erstellen**

```csharp
public class CustomerViewModel : ViewModelBase<Customer>
{
    public CustomerViewModel(Customer model) : base(model) { }
    
    public string Name => Model.Name;
    public bool IsSelected { get; set; }
}
```

### **3. DI registrieren & verwenden**

[Siehe vollständiges Beispiel](Docs/Getting-Started.md#di-registrierung)

---

## ?? Dokumentation

### **Vollständige Dokumentation:**

- ?? [Getting Started](Docs/Getting-Started.md) - Detaillierter Einstieg
- ??? [Architecture](Docs/Architecture.md) - Architektur-Übersicht
- ?? [ViewModelBase](Docs/ViewModelBase.md) - Basisklasse
- ?? [CollectionViewModel](Docs/CollectionViewModel.md) - Collection-Sync
- ?? [EditableCollectionViewModel](Docs/EditableCollectionViewModel.md) - Commands
- ?? [ViewModelFactory](Docs/ViewModelFactory.md) - Factory-Pattern
- ? [Best Practices](Docs/Best-Practices.md) - Tipps & Tricks
- ?? [API Reference](Docs/API-Reference.md) - Vollständige API

---

## ?? Tests

```
Unit-Tests:          19 Tests ?
Integration-Tests:   25 Tests ?
Behavior-Tests:      26 Tests ?
???????????????????????????????
GESAMT:              70 Tests ?
Coverage:            ~87%
```

---

## ?? Lizenz

Siehe LICENSE-Datei im Repository.

---

## ?? Support

- ?? [Issues erstellen](https://github.com/ReneRose1971/Libraries/issues)
- ?? [Dokumentation](Docs/Getting-Started.md)
