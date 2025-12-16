using System.Windows;
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
    /// Shows project details in an information dialog.
    /// </summary>
    public static void ShowProjectDetails(string projectName, string projectPath, bool fileExists)
    {
        WpfMessageBox.Show(
            $"Projekt: {projectName}\n\n" +
            $"Pfad: {projectPath}\n" +
            $"Existiert: {fileExists}",
            "Projekt-Details",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
