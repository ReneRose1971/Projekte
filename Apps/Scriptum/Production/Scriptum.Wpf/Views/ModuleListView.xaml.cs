using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Scriptum.Wpf.Projections;
using Scriptum.Wpf.ViewModels;

namespace Scriptum.Wpf.Views;

public partial class ModuleListView : UserControl
{
    public ModuleListView()
    {
        InitializeComponent();
    }

    private ModuleListViewModel? ViewModel => DataContext as ModuleListViewModel;

    private void ModuleList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is ModuleListItem module)
        {
            ViewModel?.SelectModule(module);
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.NavigateBack();
    }
}
