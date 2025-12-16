using System.IO;
using SolutionBundler.Core.Models;

namespace SolutionBundler.WPF.ViewModels.Helpers;

/// <summary>
/// Helper for creating project file dialogs.
/// </summary>
internal static class ProjectDialogHelper
{
    /// <summary>
    /// Creates and configures an OpenFileDialog for .csproj files.
    /// </summary>
    public static Microsoft.Win32.OpenFileDialog CreateProjectFileDialog()
    {
        return new Microsoft.Win32.OpenFileDialog
        {
            Title = "Projekt auswählen",
            Filter = "C# Projekt (*.csproj)|*.csproj",
            CheckFileExists = true,
            Multiselect = false
        };
    }

    /// <summary>
    /// Gets the output directory path for bundle files.
    /// </summary>
    public static string GetBundleOutputDirectory()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "SolutionBundler",
            "Bundles");
    }
}
