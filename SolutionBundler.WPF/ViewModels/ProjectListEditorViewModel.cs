using CustomWPFControls.Commands;
using CustomWPFControls.Factories;
using CustomWPFControls.ViewModels;
using DataToolKit.Abstractions.DataStores;
using PropertyChanged;
using SolutionBundler.Core.Abstractions;
using SolutionBundler.Core.Models;
using SolutionBundler.Core.Storage;
using SolutionBundler.WPF.ViewModels.Helpers;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.IO;

namespace SolutionBundler.WPF.ViewModels;

/// <summary>
/// ViewModel für die Projekt-Liste mit Bearbeitungsfunktionen.
/// Basiert auf EditableCollectionViewModel aus CustomWPFControls.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class ProjectListEditorViewModel : EditableCollectionViewModel<ProjectInfo, ProjectInfoViewModel>
{
    private readonly ProjectStore _projectStore;
    private readonly IBundleOrchestrator _bundleOrchestrator;
    private string _statusMessage = "Bereit zum Scannen...";
    private int _progressPercentage;
    private string _logText = "Bereit zum Scannen...\nWählen Sie Projekte aus und starten Sie den Scan-Prozess.";
    private bool _isScanning;

    /// <summary>
    /// Erstellt ein neues ViewModel für die Projekt-Verwaltung.
    /// </summary>
    /// <param name="projectStore">Der ProjectStore für Persistierung.</param>
    /// <param name="dataStoreProvider">DataStore-Provider für Collection-Management.</param>
    /// <param name="viewModelFactory">Factory zum Erstellen von ProjectInfoViewModels.</param>
    /// <param name="comparer">Equality-Comparer für ProjectInfo.</param>
    /// <param name="bundleOrchestrator">Orchestrator für den Scan- und Bundle-Prozess.</param>
    public ProjectListEditorViewModel(
        ProjectStore projectStore,
        IDataStoreProvider dataStoreProvider,
        IViewModelFactory<ProjectInfo, ProjectInfoViewModel> viewModelFactory,
        IEqualityComparer<ProjectInfo> comparer,
        IBundleOrchestrator bundleOrchestrator)
        : base(
            dataStoreProvider.GetDataStore<ProjectInfo>(),
            viewModelFactory,
            comparer)
    {
        _projectStore = projectStore ?? throw new ArgumentNullException(nameof(projectStore));
        _bundleOrchestrator = bundleOrchestrator ?? throw new ArgumentNullException(nameof(bundleOrchestrator));

        ConfigureAddProjectFunction();
        ConfigureEditProjectFunction();

        ScanAllProjectsCommand = new AsyncRelayCommand(
            ExecuteScanAllProjectsAsync, 
            () => !IsScanning && Projects.Count > 0);
    }

    #region Properties

    /// <summary>
    /// Schreibgeschützte Collection der Projekt-ViewModels für UI-Binding.
    /// </summary>
    [DoNotNotify]
    public ReadOnlyObservableCollection<ProjectInfoViewModel> Projects => Items;

    /// <summary>
    /// Aktuell ausgewähltes Projekt.
    /// </summary>
    [DoNotNotify]
    public ProjectInfoViewModel? SelectedProject
    {
        get => SelectedItem;
        set => SelectedItem = value;
    }

    /// <summary>
    /// Status-Nachricht für UI-Anzeige.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => _statusMessage = value;
    }

    /// <summary>
    /// Fortschritt in Prozent (0-100).
    /// </summary>
    public int ProgressPercentage
    {
        get => _progressPercentage;
        set => _progressPercentage = value;
    }

    /// <summary>
    /// Log-Text für Ausgabe-Fenster.
    /// </summary>
    public string LogText
    {
        get => _logText;
        set => _logText = value;
    }

    /// <summary>
    /// Gibt an, ob gerade ein Scan läuft.
    /// </summary>
    public bool IsScanning
    {
        get => _isScanning;
        set
        {
            _isScanning = value;
            CommandManager.InvalidateRequerySuggested();
        }
    }

    #endregion

    #region Commands

    /// <summary>
    /// Command: Projekt aus Datei-Dialog hinzufügen.
    /// </summary>
    [DoNotNotify]
    public ICommand AddProjectCommand => AddCommand;

    /// <summary>
    /// Command: Ausgewähltes Projekt entfernen.
    /// Überschreibt das Standard-DeleteCommand und operiert auf ProjectStore.
    /// </summary>
    [DoNotNotify]
    public ICommand RemoveProjectCommand => new RelayCommand(
        execute: _ =>
        {
            if (SelectedProject != null)
            {
                _projectStore.RemoveProject(SelectedProject.Name);
            }
        },
        canExecute: _ => SelectedProject != null);

    /// <summary>
    /// Command: Alle Projekte entfernen.
    /// Überschreibt das Standard-ClearCommand und operiert auf ProjectStore.
    /// </summary>
    [DoNotNotify]
    public ICommand ClearProjectsCommand => new RelayCommand(
        execute: _ =>
        {
            if (UserDialogHelper.ShowConfirmation("Möchten Sie wirklich alle Projekte entfernen?"))
            {
                _projectStore.Clear();
            }
        },
        canExecute: _ => Projects.Count > 0);

    /// <summary>
    /// Command: Projekt-Details anzeigen.
    /// </summary>
    [DoNotNotify]
    public ICommand ShowProjectDetailsCommand => EditCommand;

    /// <summary>
    /// Command: Alle Projekte scannen und Markdown-Dateien generieren.
    /// </summary>
    [DoNotNotify]
    public ICommand ScanAllProjectsCommand { get; }

    #endregion

    #region Configuration Methods

    private void ConfigureAddProjectFunction()
    {
        CreateModel = () =>
        {
            var dialog = ProjectDialogHelper.CreateProjectFileDialog();

            if (dialog.ShowDialog() == true)
            {
                return TryAddProjectFromDialog(dialog.FileName);
            }

            return null;
        };
    }

    private void ConfigureEditProjectFunction()
    {
        EditModel = (project) =>
        {
            UserDialogHelper.ShowProjectDetails(
                project.Name,
                project.Path,
                File.Exists(project.Path));
        };
    }

    private ProjectInfo? TryAddProjectFromDialog(string projectPath)
    {
        try
        {
            var fullPath = Path.GetFullPath(projectPath);

            if (_projectStore.AddProject(fullPath))
            {
                return null;
            }
            else
            {
                UserDialogHelper.ShowInformation("Das Projekt existiert bereits in der Liste.");
            }
        }
        catch (FileNotFoundException ex)
        {
            UserDialogHelper.ShowError($"Projekt-Datei nicht gefunden:\n{ex.Message}");
        }
        catch (ArgumentException ex)
        {
            UserDialogHelper.ShowError($"Ungültiger Projekt-Pfad:\n{ex.Message}");
        }
        catch (Exception ex)
        {
            UserDialogHelper.ShowError($"Fehler beim Laden des Projekts:\n{ex.Message}");
        }

        return null;
    }

    #endregion

    #region Scan Methods

    private async Task ExecuteScanAllProjectsAsync()
    {
        try
        {
            InitializeScan();
            
            var projects = LoadProjects();
            if (projects.Count == 0)
            {
                HandleNoProjectsFound();
                return;
            }

            LogProjectsLoaded(projects.Count);
            
            var outputDirectory = ProjectDialogHelper.GetBundleOutputDirectory();
            AppendLog($"Ausgabe-Verzeichnis: {outputDirectory}\n\n");

            await ProcessProjectsAsync(projects);

            FinalizeScan(projects.Count, outputDirectory);
        }
        catch (Exception ex)
        {
            HandleScanError(ex);
        }
        finally
        {
            IsScanning = false;
        }
    }

    private void InitializeScan()
    {
        IsScanning = true;
        LogText = string.Empty;
        StatusMessage = "Lade Projekte...";
        AppendLog("=== Scan gestartet ===\n");
    }

    private List<ProjectInfo> LoadProjects()
    {
        return _projectStore.Projects.ToList();
    }

    private void HandleNoProjectsFound()
    {
        StatusMessage = "Keine Projekte gefunden.";
        AppendLog("Keine Projekte zum Scannen vorhanden.\n");
    }

    private void LogProjectsLoaded(int projectCount)
    {
        AppendLog($"> {projectCount} Projekt(e) geladen.\n");
        StatusMessage = $"{projectCount} Projekt(e) geladen. Starte Scan...";
    }

    private async Task ProcessProjectsAsync(List<ProjectInfo> projects)
    {
        for (int i = 0; i < projects.Count; i++)
        {
            var project = projects[i];
            var current = i + 1;

            UpdateProgress(current, projects.Count, project.Name);
            
            await ProcessSingleProjectAsync(project, current, projects.Count);
        }
    }

    private void UpdateProgress(int current, int total, string projectName)
    {
        StatusMessage = $"[{current}/{total}] Verarbeite: {projectName}";
        ProgressPercentage = (int)((current / (double)total) * 100);
        AppendLog($"[{current}/{total}] Verarbeite: {projectName}\n");
    }

    private async Task ProcessSingleProjectAsync(ProjectInfo project, int current, int total)
    {
        try
        {
            var projectDir = GetProjectDirectory(project);
            if (projectDir == null)
            {
                AppendLog($"  ! Projektverzeichnis nicht gefunden: {Path.GetDirectoryName(project.Path)}\n");
                return;
            }

            var scanSettings = CreateScanSettings(project);
            var outputFile = await Task.Run(() => _bundleOrchestrator.Run(projectDir, scanSettings));

            AppendLog($"  > Markdown generiert: {outputFile}\n");
        }
        catch (Exception ex)
        {
            AppendLog($"  X Fehler bei {project.Name}: {ex.Message}\n");
        }
    }

    private string? GetProjectDirectory(ProjectInfo project)
    {
        var projectDir = Path.GetDirectoryName(project.Path);
        
        if (string.IsNullOrEmpty(projectDir) || !Directory.Exists(projectDir))
        {
            return null;
        }

        return projectDir;
    }

    private ScanSettings CreateScanSettings(ProjectInfo project)
    {
        return new ScanSettings
        {
            MaskSecrets = true,
            OutputFileName = $"{project.Name}.md"
        };
    }

    private void FinalizeScan(int projectCount, string outputDirectory)
    {
        StatusMessage = $"Fertig! {projectCount} Projekt(e) verarbeitet.";
        AppendLog($"\n=== Scan abgeschlossen ===\n");
        AppendLog($"> {projectCount} Projekt(e) verarbeitet.\n");
        AppendLog($"Speicherort: {outputDirectory}\n");
        ProgressPercentage = 100;
    }

    private void HandleScanError(Exception ex)
    {
        StatusMessage = $"Fehler: {ex.Message}";
        AppendLog($"\nX Fehler: {ex.Message}\n");
        ProgressPercentage = 0;
    }

    private void AppendLog(string message)
    {
        LogText += message;
    }

    #endregion
}
