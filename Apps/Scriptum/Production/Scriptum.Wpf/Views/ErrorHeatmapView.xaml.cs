using System.Windows;
using System.Windows.Controls;
using Scriptum.Wpf.ViewModels;

namespace Scriptum.Wpf.Views;

public partial class ErrorHeatmapView : UserControl
{
    public ErrorHeatmapView()
    {
        InitializeComponent();
    }

    private ErrorHeatmapViewModel? ViewModel => DataContext as ErrorHeatmapViewModel;

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.NavigateBack();
    }
}
