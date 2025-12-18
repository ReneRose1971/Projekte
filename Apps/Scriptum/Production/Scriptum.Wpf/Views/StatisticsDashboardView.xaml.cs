using System.Windows;
using System.Windows.Controls;
using Scriptum.Wpf.ViewModels;

namespace Scriptum.Wpf.Views;

public partial class StatisticsDashboardView : UserControl
{
    public StatisticsDashboardView()
    {
        InitializeComponent();
    }

    private StatisticsDashboardViewModel? ViewModel => DataContext as StatisticsDashboardViewModel;

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.NavigateBack();
    }
}
