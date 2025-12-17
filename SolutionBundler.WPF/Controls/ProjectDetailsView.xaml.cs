using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SolutionBundler.WPF.Controls;

/// <summary>
/// UserControl zur Anzeige und Bearbeitung von Projekt-Details.
/// </summary>
public partial class ProjectDetailsView : UserControl
{
    public static readonly DependencyProperty AvailableGroupsProperty =
        DependencyProperty.Register(
            nameof(AvailableGroups),
            typeof(IEnumerable<string>),
            typeof(ProjectDetailsView),
            new PropertyMetadata(null, OnAvailableGroupsChanged));

    public IEnumerable<string>? AvailableGroups
    {
        get => (IEnumerable<string>?)GetValue(AvailableGroupsProperty);
        set => SetValue(AvailableGroupsProperty, value);
    }

    public ProjectDetailsView()
    {
        InitializeComponent();
    }

    private static void OnAvailableGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProjectDetailsView view && e.NewValue is IEnumerable<string> groups)
        {
            view.GroupComboBox.ItemsSource = groups;
        }
    }
}
