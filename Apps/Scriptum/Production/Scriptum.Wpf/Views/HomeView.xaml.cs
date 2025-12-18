using System.Windows;
using System.Windows.Controls;
using Scriptum.Wpf.ViewModels;

namespace Scriptum.Wpf.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
    }

    private HomeViewModel? ViewModel => DataContext as HomeViewModel;

    private void ModulesButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.NavigateToModules();
    }

    private void StatisticsButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.NavigateToStatistics();
    }

    private void SessionsButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.NavigateToLastSession();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.NavigateToSettings();
    }
}
