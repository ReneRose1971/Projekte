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
    /// Shows project details in an information dialog with option to edit group.
    /// </summary>
    /// <param name="projectName">Name des Projekts.</param>
    /// <param name="projectPath">Pfad zur .csproj-Datei.</param>
    /// <param name="fileExists">Gibt an, ob die Datei existiert.</param>
    /// <param name="currentGroup">Aktuelle Gruppe (kann null oder leer sein).</param>
    /// <returns>Neue Gruppe oder null wenn Abbruch.</returns>
    public static string? ShowProjectDetails(string projectName, string projectPath, bool fileExists, string? currentGroup = null)
    {
        var message = $"Projekt: {projectName}\n\n" +
                     $"Pfad: {projectPath}\n" +
                     $"Existiert: {fileExists}\n\n" +
                     $"Aktuelle Gruppe: {currentGroup ?? "(keine)"}\n\n" +
                     $"Neue Gruppe eingeben (leer lassen zum Entfernen):";

        var inputDialog = new GroupInputDialog(message, currentGroup ?? string.Empty);
        inputDialog.Owner = Application.Current?.MainWindow;
        
        if (inputDialog.ShowDialog() == true)
        {
            return string.IsNullOrWhiteSpace(inputDialog.GroupName) ? null : inputDialog.GroupName;
        }

        return currentGroup; // Keine Änderung
    }

    /// <summary>
    /// Zeigt einen einfachen Input-Dialog für die Gruppen-Eingabe.
    /// </summary>
    /// <param name="prompt">Aufforderungstext.</param>
    /// <param name="defaultValue">Vorbelegter Wert.</param>
    /// <returns>Eingegebener Wert oder null bei Abbruch.</returns>
    public static string? ShowGroupInputDialog(string prompt, string defaultValue = "")
    {
        var inputDialog = new GroupInputDialog(prompt, defaultValue);
        inputDialog.Owner = Application.Current?.MainWindow;
        
        if (inputDialog.ShowDialog() == true)
        {
            return string.IsNullOrWhiteSpace(inputDialog.GroupName) ? null : inputDialog.GroupName;
        }

        return null;
    }
}

/// <summary>
/// Einfacher Input-Dialog für die Gruppen-Eingabe.
/// </summary>
internal class GroupInputDialog : Window
{
    private readonly System.Windows.Controls.TextBox _textBox;

    public string GroupName => _textBox.Text;

    public GroupInputDialog(string prompt, string defaultValue)
    {
        Title = "Gruppe bearbeiten";
        Width = 400;
        Height = 200;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;

        var stackPanel = new System.Windows.Controls.StackPanel
        {
            Margin = new Thickness(10)
        };

        var promptLabel = new System.Windows.Controls.TextBlock
        {
            Text = prompt,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 10)
        };

        _textBox = new System.Windows.Controls.TextBox
        {
            Text = defaultValue,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var buttonPanel = new System.Windows.Controls.StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var okButton = new System.Windows.Controls.Button
        {
            Content = "OK",
            Width = 75,
            Height = 25,
            Margin = new Thickness(0, 0, 5, 0),
            IsDefault = true
        };
        okButton.Click += (s, e) => { DialogResult = true; Close(); };

        var cancelButton = new System.Windows.Controls.Button
        {
            Content = "Abbrechen",
            Width = 75,
            Height = 25,
            IsCancel = true
        };
        cancelButton.Click += (s, e) => { DialogResult = false; Close(); };

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);

        stackPanel.Children.Add(promptLabel);
        stackPanel.Children.Add(_textBox);
        stackPanel.Children.Add(buttonPanel);

        Content = stackPanel;

        Loaded += (s, e) => _textBox.Focus();
    }
}
