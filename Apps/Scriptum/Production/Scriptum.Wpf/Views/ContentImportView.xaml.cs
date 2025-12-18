using System.Windows;
using System.Windows.Controls;
using Scriptum.Wpf.ViewModels;

namespace Scriptum.Wpf.Views;

public partial class ContentImportView : UserControl
{
    public ContentImportView()
    {
        InitializeComponent();
    }

    private void BrowseModules_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ContentImportViewModel vm)
        {
            vm.BrowseModules();
        }
    }

    private void BrowseLessons_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ContentImportViewModel vm)
        {
            vm.BrowseLessons();
        }
    }

    private void BrowseGuides_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ContentImportViewModel vm)
        {
            vm.BrowseGuides();
        }
    }

    private async void Import_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ContentImportViewModel vm)
        {
            await vm.ImportAsync();
        }
    }

    private void GoBack_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ContentImportViewModel vm)
        {
            vm.GoBack();
        }
    }
}
