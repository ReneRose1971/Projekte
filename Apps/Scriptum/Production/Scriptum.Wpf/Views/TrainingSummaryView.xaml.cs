using System.Windows;
using System.Windows.Controls;
using Scriptum.Wpf.ViewModels;

namespace Scriptum.Wpf.Views;

public partial class TrainingSummaryView : UserControl
{
    public TrainingSummaryView()
    {
        InitializeComponent();
    }

    private TrainingSummaryViewModel? ViewModel => DataContext as TrainingSummaryViewModel;

    private void ModulesButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.NavigateToModules();
    }

    private void HomeButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.NavigateToHome();
    }
}
