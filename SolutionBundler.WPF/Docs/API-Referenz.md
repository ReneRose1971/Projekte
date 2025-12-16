# API-Referenz - SolutionBundler.WPF

Vollständige API-Dokumentation aller ViewModels, Custom Controls und UI-Komponenten.

---

## Namespaces

- [ViewModels](#viewmodels) - MVVM ViewModels
- [Controls](#controls) - Custom WPF Controls
- [Converters](#converters) - Value Converters
- [Storage](#storage) - Persistierung

---

## ViewModels

### ProjectInfoViewModel

Wrapper-ViewModel für `ProjectInfo`-Model.

```csharp
namespace SolutionBundler.WPF.ViewModels;

public class ProjectInfoViewModel : ViewModelBase<ProjectInfo>, 
                                     IViewModelWrapper<ProjectInfo>
{
    public ProjectInfoViewModel(ProjectInfo model);
    
    public string Name { get; }
    public string Path { get; }
    public bool FileExists { get; }
    public string StatusIcon { get; }
    public string StatusText { get; }
    public ProjectInfo Model { get; }
}
```

**Properties:**

| Property | Typ | Beschreibung |
|----------|-----|--------------|
| `Name` | `string` | Projektname (aus Path extrahiert) |
| `Path` | `string` | Vollständiger Pfad zur `.csproj` |
| `FileExists` | `bool` | Prüft, ob Datei existiert |
| `StatusIcon` | `string` | `"?"` wenn existiert, `"?"` sonst |
| `StatusText` | `string` | Tooltip-Text |

**Beispiel:**

```csharp
var projectInfo = new ProjectInfo { Path = @"C:\Projects\MyApp\MyApp.csproj" };
var viewModel = new ProjectInfoViewModel(projectInfo);

Console.WriteLine(viewModel.Name);        // "MyApp"
Console.WriteLine(viewModel.StatusIcon);  // "?" oder "?"
```

---

### ProjectListEditorViewModel

Collection-ViewModel mit CRUD-Operationen für Projekte.

```csharp
public class ProjectListEditorViewModel 
    : EditableCollectionViewModel<ProjectInfo, ProjectInfoViewModel>
{
    public ProjectListEditorViewModel(
        IDataStoreProvider provider,
        IRepositoryFactory repositoryFactory,
        IViewModelFactory<ProjectInfo, ProjectInfoViewModel> viewModelFactory,
        IEqualityComparer<ProjectInfo> comparer);
    
    // Properties
    public ReadOnlyObservableCollection<ProjectInfoViewModel> Projects { get; }
    public ProjectInfoViewModel? SelectedProject { get; set; }
    
    // Commands
    public ICommand AddProjectCommand { get; }
    public ICommand RemoveProjectCommand { get; }
    public ICommand ClearProjectsCommand { get; }
    public ICommand ShowProjectDetailsCommand { get; }
}
```

**Commands:**

| Command | Beschreibung | CanExecute |
|---------|--------------|-----------|
| `AddProjectCommand` | Öffnet FileDialog für `.csproj`-Auswahl | Immer |
| `RemoveProjectCommand` | Entfernt selektiertes Projekt | `SelectedProject != null` |
| `ClearProjectsCommand` | Löscht alle Projekte | `Projects.Count > 0` |
| `ShowProjectDetailsCommand` | Zeigt MessageBox mit Details | `SelectedProject != null` |

**Verwendung:**

```csharp
public class MainViewModel
{
    private readonly ProjectListEditorViewModel _projectEditor;
    
    public MainViewModel(ProjectListEditorViewModel projectEditor)
    {
        _projectEditor = projectEditor;
    }
    
    public ProjectListEditorViewModel ProjectEditor => _projectEditor;
}
```

**XAML-Binding:**

```xaml
<Window DataContext="{Binding MainViewModel}">
    <controls:ProjectListEditorView 
        ItemsSource="{Binding ProjectEditor.Projects}"
        SelectedItem="{Binding ProjectEditor.SelectedProject}"
        AddCommand="{Binding ProjectEditor.AddProjectCommand}"
        DeleteCommand="{Binding ProjectEditor.RemoveProjectCommand}"/>
</Window>
```

---

### SimpleViewModelFactory

Einfache Factory-Implementierung für ViewModels.

```csharp
namespace SolutionBundler.WPF.ViewModels;

public class SimpleViewModelFactory<TModel, TViewModel> 
    : IViewModelFactory<TModel, TViewModel>
    where TModel : class
    where TViewModel : class, IViewModelWrapper<TModel>
{
    public SimpleViewModelFactory(Func<TModel, TViewModel> factoryFunc);
    
    public TViewModel Create(TModel model);
}
```

**DI-Registrierung:**

```csharp
builder.Services.AddSingleton<IViewModelFactory<ProjectInfo, ProjectInfoViewModel>>(
    new SimpleViewModelFactory<ProjectInfo, ProjectInfoViewModel>(
        model => new ProjectInfoViewModel(model)));
```

---

## Controls

### ProjectListEditorView

Custom WPF Control für Projekt-Verwaltung.

```csharp
namespace SolutionBundler.WPF.Controls;

public class ProjectListEditorView : ListEditorView
{
    // DependencyProperties
    public static readonly DependencyProperty TitleProperty;
    public static readonly DependencyProperty EmptyMessageProperty;
    public static readonly DependencyProperty ItemsSourceProperty;
    public static readonly DependencyProperty SelectedItemProperty;
    public static readonly DependencyProperty AddCommandProperty;
    public static readonly DependencyProperty EditCommandProperty;
    public static readonly DependencyProperty DeleteCommandProperty;
    public static readonly DependencyProperty ClearCommandProperty;
    
    // Properties
    public string Title { get; set; }
    public string EmptyMessage { get; set; }
    public IEnumerable ItemsSource { get; set; }
    public object SelectedItem { get; set; }
    public ICommand AddCommand { get; set; }
    public ICommand EditCommand { get; set; }
    public ICommand DeleteCommand { get; set; }
    public ICommand ClearCommand { get; set; }
}
```

**DependencyProperties:**

| Property | Typ | Standard | Beschreibung |
|----------|-----|----------|--------------|
| `Title` | `string` | `"Projekte"` | Titel der Liste |
| `EmptyMessage` | `string` | `""` | Platzhalter bei leerer Liste |
| `ItemsSource` | `IEnumerable` | `null` | Collection für Binding |
| `SelectedItem` | `object` | `null` | Selektiertes Element |
| `AddCommand` | `ICommand` | `null` | Command für "Add"-Button |
| `EditCommand` | `ICommand` | `null` | Command für "Details"-Button |
| `DeleteCommand` | `ICommand` | `null` | Command für "Remove"-Button |
| `ClearCommand` | `ICommand` | `null` | Command für "Clear All"-Button |

**XAML:**

```xaml
<controls:ProjectListEditorView 
    Title="Meine Projekte"
    EmptyMessage="Keine Projekte vorhanden. Klicken Sie auf 'Add'."
    ItemsSource="{Binding Projects}"
    SelectedItem="{Binding SelectedProject, Mode=TwoWay}"
    AddCommand="{Binding AddCommand}"
    EditCommand="{Binding EditCommand}"
    DeleteCommand="{Binding DeleteCommand}"
    ClearCommand="{Binding ClearCommand}"/>
```

**Style-Datei:** `Themes/Generic.xaml` oder `Themes/Generic2.xaml`

---

### LogOutputView

Custom Control für Log-Ausgabe.

```csharp
public class LogOutputView : Control
{
    // DependencyProperties
    public static readonly DependencyProperty TitleProperty;
    public static readonly DependencyProperty LogTextProperty;
    public static readonly DependencyProperty ProgressProperty;
    public static readonly DependencyProperty StatusTextProperty;
    public static readonly DependencyProperty IsProgressVisibleProperty;
    
    // Properties
    public string Title { get; set; }
    public string LogText { get; set; }
    public double Progress { get; set; }
    public string StatusText { get; set; }
    public bool IsProgressVisible { get; set; }
}
```

**DependencyProperties:**

| Property | Typ | Standard | Beschreibung |
|----------|-----|----------|--------------|
| `Title` | `string` | `"Ausgabe"` | Titel der Log-Ausgabe |
| `LogText` | `string` | `""` | Mehrzeiliger Log-Text |
| `Progress` | `double` | `0.0` | Fortschritt (0-100) |
| `StatusText` | `string` | `"Bereit"` | Status-Text |
| `IsProgressVisible` | `bool` | `true` | ProgressBar sichtbar? |

**XAML:**

```xaml
<controls:LogOutputView 
    Title="Build-Log"
    LogText="{Binding BuildLog}"
    StatusText="{Binding Status}"
    Progress="{Binding ProgressValue}"
    IsProgressVisible="True"/>
```

**Features:**
- Monospace-Font (Consolas)
- Auto-Scroll (via Behavior)
- Read-only TextBox
- ProgressBar (0-100%)

---

## Converters

### CountToVisibilityConverter

Konvertiert Collection-Count in Visibility.

```csharp
namespace SolutionBundler.WPF.Converters;

public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, 
                          CultureInfo culture);
}
```

**Logik:**
- `Count > 0` ? `Visibility.Visible`
- `Count == 0` ? `Visibility.Collapsed`

**XAML:**

```xaml
<TextBlock 
    Text="Projekte vorhanden"
    Visibility="{Binding Projects.Count, 
                 Converter={StaticResource CountToVisibilityConverter}}"/>
```

---

### InverseCountToVisibilityConverter

Inverse Variante von `CountToVisibilityConverter`.

```csharp
public class InverseCountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, 
                          CultureInfo culture);
}
```

**Logik:**
- `Count == 0` ? `Visibility.Visible`
- `Count > 0` ? `Visibility.Collapsed`

**XAML:**

```xaml
<Border 
    Visibility="{Binding Projects.Count, 
                 Converter={StaticResource InverseCountToVisibilityConverter}}">
    <TextBlock Text="Keine Projekte vorhanden"/>
</Border>
```

---

## Storage

### ProjectStore

Wrapper für DataToolKit-Persistierung.

```csharp
namespace SolutionBundler.Core.Storage;

public class ProjectStore
{
    public ProjectStore(
        IDataStoreProvider dataStoreProvider,
        IRepositoryFactory repositoryFactory);
    
    public ReadOnlyObservableCollection<ProjectInfo> Projects { get; }
    
    public bool AddProject(string projectPath);
    public bool RemoveProject(string projectName);
    public bool ContainsProject(string projectName);
    public void Clear();
}
```

**Verwendung:**

```csharp
var store = new ProjectStore(dataStoreProvider, repositoryFactory);

// Projekt hinzufügen
store.AddProject(@"C:\Projects\MyApp\MyApp.csproj");

// Projekt entfernen
store.RemoveProject("MyApp");

// Alle Projekte
foreach (var project in store.Projects)
{
    Console.WriteLine(project.Path);
}

// Alle löschen
store.Clear();
```

**Persistierung:** Automatisch in `%AppData%/SolutionBundler/`

---

## Windows

### MainWindowWithSplitView

Haupt-Window mit Split-Layout.

```csharp
public partial class MainWindowWithSplitView : Window
{
    public MainWindowWithSplitView();
}
```

**Layout:**
- **Links (33%):** `ProjectListEditorView`
- **Rechts (66%):** `LogOutputView`
- **GridSplitter:** Anpassbare Größe

**XAML:**

```xaml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="1*"/>   <!-- 33% -->
        <ColumnDefinition Width="5"/>    <!-- Splitter -->
        <ColumnDefinition Width="2*"/>   <!-- 66% -->
    </Grid.ColumnDefinitions>
    
    <controls:ProjectListEditorView Grid.Column="0"/>
    <GridSplitter Grid.Column="1"/>
    <controls:LogOutputView Grid.Column="2"/>
</Grid>
```

---

### ProjectManagementWindow

Demo-Window für Projekt-Verwaltung.

```csharp
public partial class ProjectManagementWindow : Window
{
    public ProjectManagementWindow(ProjectListEditorViewModel viewModel);
}
```

**Verwendung:**

```csharp
// In DI registrieren
builder.Services.AddTransient<ProjectManagementWindow>();

// Öffnen
var window = serviceProvider.GetRequiredService<ProjectManagementWindow>();
window.Show();
```

---

## Dependency Injection

### Vollständiges Setup

```csharp
// In App.xaml.cs
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);
    
    var builder = Host.CreateApplicationBuilder();
    
    // 1. DataToolKit-Module
    builder.Services.AddModulesFromAssemblies(
        typeof(DataToolKitServiceModule).Assembly);
    
    // 2. ProjectStore
    builder.Services.AddSingleton<ProjectStore>();
    
    // 3. Comparer
    builder.Services.AddSingleton<IEqualityComparer<ProjectInfo>>(
        new FallbackEqualsComparer<ProjectInfo>());
    
    // 4. ViewModelFactory
    builder.Services.AddSingleton<IViewModelFactory<ProjectInfo, ProjectInfoViewModel>>(
        new SimpleViewModelFactory<ProjectInfo, ProjectInfoViewModel>(
            model => new ProjectInfoViewModel(model)));
    
    // 5. ViewModels
    builder.Services.AddTransient<ProjectListEditorViewModel>();
    
    // 6. Windows
    builder.Services.AddTransient<MainWindowWithSplitView>();
    builder.Services.AddTransient<ProjectManagementWindow>();
    
    _host = builder.Build();
    
    // Starten
    var mainWindow = _host.Services.GetRequiredService<MainWindowWithSplitView>();
    mainWindow.Show();
}
```

---

## Siehe auch

- [SolutionBundler.WPF README](../README.md)
- [CustomWPFControls API](../../../Libraries/CustomWPFControls/Docs/API-Reference.md)
- [DataToolKit Storage](../../../Libraries/DataToolKit/Docs/Storage-Options.md)
