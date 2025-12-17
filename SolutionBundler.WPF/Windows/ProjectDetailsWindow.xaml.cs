using System.Collections.Generic;
using System.Windows;
using SolutionBundler.WPF.ViewModels;

namespace SolutionBundler.WPF.Windows;

/// <summary>
/// Dialog-Fenster zur Anzeige und Bearbeitung von Projekt-Details.
/// </summary>
public partial class ProjectDetailsWindow : Window
{
    /// <summary>
    /// Erstellt ein neues Projekt-Details-Fenster.
    /// </summary>
    /// <param name="viewModel">Das ProjectInfoViewModel mit den Projekt-Daten.</param>
    /// <param name="availableGroups">Liste der bereits vorhandenen Gruppen.</param>
    public ProjectDetailsWindow(ProjectInfoViewModel viewModel, IEnumerable<string>? availableGroups = null)
    {
        InitializeComponent();
        DataContext = viewModel;

        if (availableGroups != null)
        {
            DetailsView.AvailableGroups = availableGroups;
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
