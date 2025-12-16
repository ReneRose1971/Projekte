# Getting Started - CustomWPFControls

Vollständiger Einstieg in CustomWPFControls mit MVVM, DataStore und Commands.

## ?? Inhaltsverzeichnis

- [Installation](#installation)
- [Projekt-Setup](#projekt-setup)
- [Model erstellen](#1-model-erstellen)
- [ViewModel erstellen](#2-viewmodel-erstellen)
- [DI-Konfiguration](#3-di-konfiguration)
- [View erstellen](#4-view-erstellen)
- [Fertig!](#fertig)

---

## ?? Installation

### **1. NuGet-Pakete installieren**

```bash
dotnet add package PropertyChanged.Fody
dotnet add package Fody
dotnet add package Microsoft.Extensions.DependencyInjection
```

### **2. Projekt-Referenzen hinzufügen**

```xml
<ItemGroup>
  <ProjectReference Include="..\CustomWPFControls\CustomWPFControls.csproj" />
  <ProjectReference Include="..\DataToolKit\DataToolKit.csproj" />
  <ProjectReference Include="..\Common.BootStrap\Common.BootStrap.csproj" />
</ItemGroup>
```

### **3. FodyWeavers.xml erstellen**

```xml
<?xml version="1.0" encoding="utf-8"?>
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <PropertyChanged FilterType="Explicit" InjectOnPropertyNameChanged="false" />
</Weavers>
```

---

## ??? Projekt-Setup

### **Ordner-Struktur**

```
MyWpfApp/
??? Models/
?   ??? Customer.cs
??? ViewModels/
?   ??? CustomerItemViewModel.cs
?   ??? CustomerListViewModel.cs
??? Views/
?   ??? CustomerListView.xaml
??? DI/
?   ??? ViewModelModule.cs
??? App.xaml.cs
```

---

## 1?? Model erstellen

### **Models/Customer.cs**

```csharp
namespace MyWpfApp.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";

        public override bool Equals(object? obj)
        {
            if (obj is not Customer other) return false;
            return Id == other.Id && Name == other.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name);
        }

        public override string ToString()
        {
            return $"Customer{{Id={Id}, Name={Name}}}";
        }
    }
}
```

---

## 2?? ViewModel erstellen

### **ViewModels/CustomerItemViewModel.cs**

```csharp
using CustomWPFControls.ViewModels;
using MyWpfApp.Models;

namespace MyWpfApp.ViewModels
{
    public class CustomerItemViewModel : ViewModelBase<Customer>
    {
        public CustomerItemViewModel(Customer model) : base(model)
        {
        }

        // Domain-Properties (read-only, delegiert an Model)
        public int Id => Model.Id;
        public string Name => Model.Name;
        public string Email => Model.Email;
        public string Phone => Model.Phone;

        // UI-Properties (mit PropertyChanged via Fody)
        public bool IsSelected { get; set; }
        public bool IsExpanded { get; set; }

        // Computed Properties
        public string DisplayName => $"{Name} ({Email})";
        public string ShortInfo => $"{Name.Substring(0, Math.Min(20, Name.Length))}...";
    }
}
```

### **ViewModels/CustomerListViewModel.cs**

```csharp
using System.Collections.Generic;
using CustomWPFControls.Factories;
using CustomWPFControls.ViewModels;
using DataToolKit.Abstractions.DataStores;
using MyWpfApp.Models;

namespace MyWpfApp.ViewModels
{
    public class CustomerListViewModel : EditableCollectionViewModel<Customer, CustomerItemViewModel>
    {
        public CustomerListViewModel(
            IDataStore<Customer> dataStore,
            IViewModelFactory<Customer, CustomerItemViewModel> viewModelFactory,
            IEqualityComparer<Customer> modelComparer)
            : base(dataStore, viewModelFactory, modelComparer)
        {
            // CreateModel-Callback setzen
            CreateModel = () => new Customer
            {
                Name = "New Customer",
                Email = "new@example.com"
            };

            // EditModel-Callback setzen (optional)
            EditModel = customer =>
            {
                // Hier Dialog öffnen oder inline bearbeiten
                // Beispiel: OpenEditDialog(customer);
            };
        }
    }
}
```

---

## 3?? DI-Konfiguration

### **DI/ViewModelModule.cs**

```csharp
using System.Collections.Generic;
using Common.Bootstrap;
using Common.Bootstrap.Defaults;
using CustomWPFControls.Factories;
using DataToolKit.Abstractions.DataStores;
using Microsoft.Extensions.DependencyInjection;
using MyWpfApp.Models;
using MyWpfApp.ViewModels;

namespace MyWpfApp.DI
{
    public class ViewModelModule : IServiceModule
    {
        public void Register(IServiceCollection services)
        {
            // 1. EqualityComparer für Customer registrieren
            services.AddSingleton<IEqualityComparer<Customer>>(
                new FallbackEqualsComparer<Customer>());

            // 2. DataStore registrieren (JSON oder LiteDB)
            services.AddSingleton<IDataStore<Customer>>(provider =>
            {
                var comparer = provider.GetRequiredService<IEqualityComparer<Customer>>();
                
                // Option A: In-Memory (für Development)
                return new InMemoryDataStore<Customer>(comparer);
                
                // Option B: Persistent mit JSON
                // var dataStoreProvider = provider.GetRequiredService<IDataStoreProvider>();
                // var repositoryFactory = provider.GetRequiredService<IRepositoryFactory>();
                // return dataStoreProvider.GetPersistent<Customer>(
                //     repositoryFactory,
                //     autoLoad: true);
            });

            // 3. ViewModelFactory registrieren
            services.AddViewModelFactory<Customer, CustomerItemViewModel>();

            // 4. CustomerListViewModel registrieren
            services.AddSingleton<CustomerListViewModel>();
        }
    }
}
```

### **App.xaml.cs**

```csharp
using System.Windows;
using Common.Bootstrap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyWpfApp.DI;
using MyWpfApp.Views;

namespace MyWpfApp
{
    public partial class App : Application
    {
        private IHost? _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Host mit DI erstellen
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Module registrieren
                    services.AddModulesFromAssemblies(
                        typeof(App).Assembly,
                        typeof(ViewModelModule).Assembly);
                    
                    // MainWindow registrieren
                    services.AddSingleton<MainWindow>();
                })
                .Build();

            // MainWindow anzeigen
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host?.Dispose();
            base.OnExit(e);
        }
    }
}
```

---

## 4?? View erstellen

### **Views/MainWindow.xaml**

```xml
<Window x:Class="MyWpfApp.Views.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:viewModels="clr-namespace:MyWpfApp.ViewModels"
        Title="Customer Management" 
        Height="600" Width="800">
    
    <Grid Margin="10">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Titel -->
        <TextBlock Grid.Row="0" 
                   Text="Customers" 
                   FontSize="24" 
                   FontWeight="Bold" 
                   Margin="0,0,0,10"/>

        <!-- Liste -->
        <ListBox Grid.Row="1" 
                 ItemsSource="{Binding Items}"
                 SelectedItem="{Binding SelectedItem}"
                 DisplayMemberPath="DisplayName"
                 Margin="0,0,0,10">
            <ListBox.ItemTemplate>
                <DataTemplate>
                    <StackPanel>
                        <TextBlock Text="{Binding Name}" 
                                   FontWeight="Bold" 
                                   FontSize="14"/>
                        <TextBlock Text="{Binding Email}" 
                                   FontSize="12" 
                                   Foreground="Gray"/>
                    </StackPanel>
                </DataTemplate>
            </ListBox.ItemTemplate>
        </ListBox>

        <!-- Commands -->
        <StackPanel Grid.Row="2" 
                    Orientation="Horizontal" 
                    HorizontalAlignment="Right">
            <Button Content="Add" 
                    Command="{Binding AddCommand}" 
                    Width="80" 
                    Margin="5"/>
            <Button Content="Edit" 
                    Command="{Binding EditCommand}" 
                    Width="80" 
                    Margin="5"/>
            <Button Content="Delete" 
                    Command="{Binding DeleteCommand}" 
                    Width="80" 
                    Margin="5"/>
            <Button Content="Clear All" 
                    Command="{Binding ClearCommand}" 
                    Width="80" 
                    Margin="5"/>
        </StackPanel>
    </Grid>
</Window>
```

### **Views/MainWindow.xaml.cs**

```csharp
using System.Windows;
using MyWpfApp.ViewModels;

namespace MyWpfApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(CustomerListViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
```

---

## ? Fertig!

Starten Sie die Anwendung:

```bash
dotnet run
```

### **Was Sie jetzt haben:**

- ? **Bidirektionale Synchronisation** - View ? DataStore
- ? **PropertyChanged** - Automatisch via Fody
- ? **Commands** - Add, Edit, Delete, Clear
- ? **DI-Integration** - Alles über DI-Container
- ? **MVVM-Pattern** - Saubere Trennung

---

## ?? Nächste Schritte

- ?? [Architecture](Architecture.md) - Architektur verstehen
- ?? [ViewModelBase](ViewModelBase.md) - Basisklasse erweitern
- ? [Best Practices](Best-Practices.md) - Tipps & Tricks

---

## ?? Troubleshooting

### **PropertyChanged funktioniert nicht**

Stellen Sie sicher, dass:
1. `FodyWeavers.xml` im Projekt-Root liegt
2. PropertyChanged.Fody NuGet installiert ist
3. Build erfolgreich war (Fody weaved beim Build)

### **ViewModel nicht erstellt**

Prüfen Sie:
1. ViewModelFactory registriert: `services.AddViewModelFactory<TModel, TViewModel>()`
2. ViewModel hat Constructor mit TModel-Parameter
3. Dependencies im DI-Container registriert

### **DataStore nicht synchronisiert**

Prüfen Sie:
1. IEqualityComparer registriert
2. DataStore via DI injiziert
3. Model.Equals/GetHashCode implementiert
