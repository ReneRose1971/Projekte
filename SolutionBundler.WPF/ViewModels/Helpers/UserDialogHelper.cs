using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SolutionBundler.WPF.ViewModels;
using SolutionBundler.WPF.Windows;
using WpfMessageBox = System.Windows.MessageBox;

namespace SolutionBundler.WPF.ViewModels.Helpers;

/// <summary>
/// Helper class for displaying user dialogs and messages.
/// </summary>
internal static class UserDialogHelper
{
    /// <summary>
    /// Shows an error message box.
    /// </summary>
    public static void ShowError(string message, string title = "Fehler")
    {
        WpfMessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    /// Shows an information message box.
    /// </summary>
    public static void ShowInformation(string message, string title = "Information")
    {
        WpfMessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// Shows a confirmation dialog and returns true if user confirms.
    /// </summary>
    public static bool ShowConfirmation(string message, string title = "Bestätigung")
    {
        var result = WpfMessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }

    /// <summary>
    /// Shows project details in a dialog window with option to edit group.
    /// </summary>
    /// <param name="projectName">Name des Projekts.</param>
    /// <param name="projectPath">Pfad zur .csproj-Datei.</param>
    /// <param name="fileExists">Gibt an, ob die Datei existiert.</param>
    /// <param name="currentGroup">Aktuelle Gruppe (kann null oder leer sein).</param>
    /// <param name="availableGroups">Liste der bereits vorhandenen Gruppen (optional).</param>
    /// <returns>Neue Gruppe oder null wenn Abbruch.</returns>
    public static string? ShowProjectDetails(
        string projectName,
        string projectPath,
        bool fileExists,
        string? currentGroup = null,
        IEnumerable<string>? availableGroups = null)
    {
        var projectInfo = new Core.Models.Persistence.ProjectInfo
        {
            Path = projectPath,
            Group = currentGroup
        };

        var viewModel = new ProjectInfoViewModel(projectInfo);
        var window = new ProjectDetailsWindow(viewModel, availableGroups);

        if (Application.Current?.MainWindow != null)
        {
            window.Owner = Application.Current.MainWindow;
        }

        if (window.ShowDialog() == true)
        {
            return string.IsNullOrWhiteSpace(viewModel.Group) ? null : viewModel.Group;
        }

        return currentGroup;
    }
}
