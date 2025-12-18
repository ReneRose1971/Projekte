using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using PropertyChanged;
using Scriptum.Application.Import;
using Scriptum.Wpf.Navigation;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für den Content-Import.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class ContentImportViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IContentImportService _importService;

    public ContentImportViewModel(
        INavigationService navigationService,
        IContentImportService importService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _importService = importService ?? throw new ArgumentNullException(nameof(importService));
    }

    public string ModulesPath { get; set; } = string.Empty;
    public string LessonsPath { get; set; } = string.Empty;
    public string GuidesPath { get; set; } = string.Empty;
    public bool OverwriteExisting { get; set; }
    public bool IsBusy { get; private set; }
    public string StatusText { get; private set; } = "Bereit für Import";
    public ObservableCollection<string> Warnings { get; } = new();
    public ContentImportResult? LastResult { get; private set; }
    public string OutputFolderPath { get; private set; } = string.Empty;

    public bool CanImport => !IsBusy 
        && !string.IsNullOrWhiteSpace(ModulesPath) 
        && !string.IsNullOrWhiteSpace(LessonsPath) 
        && !string.IsNullOrWhiteSpace(GuidesPath);

    public void BrowseModules()
    {
        var path = OpenFileDialog("JSON-Dateien (*.json)|*.json|Alle Dateien (*.*)|*.*");
        if (path != null)
        {
            ModulesPath = path;
        }
    }

    public void BrowseLessons()
    {
        var path = OpenFileDialog("JSON-Dateien (*.json)|*.json|Alle Dateien (*.*)|*.*");
        if (path != null)
        {
            LessonsPath = path;
        }
    }

    public void BrowseGuides()
    {
        var path = OpenFileDialog("JSON-Dateien (*.json)|*.json|Alle Dateien (*.*)|*.*");
        if (path != null)
        {
            GuidesPath = path;
        }
    }

    public async Task ImportAsync()
    {
        if (!CanImport)
            return;

        IsBusy = true;
        StatusText = "Import läuft...";
        Warnings.Clear();
        LastResult = null;
        OutputFolderPath = string.Empty;

        try
        {
            var request = new ContentImportRequest(
                ModulesPath,
                LessonsPath,
                GuidesPath,
                OverwriteExisting);

            var result = await _importService.ImportAsync(request);
            LastResult = result;

            if (result.Success)
            {
                StatusText = $"Import erfolgreich: {result.ModulesImported} Module, {result.LessonsImported} Lektionen, {result.GuidesImported} Guides";
                OutputFolderPath = result.OutputFolderPath;

                foreach (var warning in result.Warnings)
                {
                    Warnings.Add(warning);
                }

                if (result.Warnings.Count > 0)
                {
                    StatusText += $" ({result.Warnings.Count} Warnungen)";
                }

                MessageBox.Show(
                    $"Import erfolgreich abgeschlossen!\n\n" +
                    $"Module: {result.ModulesImported}\n" +
                    $"Lektionen: {result.LessonsImported}\n" +
                    $"Anleitungen: {result.GuidesImported}\n\n" +
                    $"Bitte starten Sie die Anwendung neu, um die importierten Inhalte zu laden.",
                    "Import erfolgreich",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                StatusText = $"Import fehlgeschlagen: {result.ErrorMessage}";
                Warnings.Add(result.ErrorMessage ?? "Unbekannter Fehler");

                MessageBox.Show(
                    $"Import fehlgeschlagen:\n\n{result.ErrorMessage}",
                    "Import fehlgeschlagen",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Fehler: {ex.Message}";
            Warnings.Add(ex.Message);

            MessageBox.Show(
                $"Fehler beim Import:\n\n{ex.Message}",
                "Fehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void GoBack()
    {
        _navigationService.NavigateToContentManagement();
    }

    private static string? OpenFileDialog(string filter)
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter,
            Title = "Datei auswählen"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
