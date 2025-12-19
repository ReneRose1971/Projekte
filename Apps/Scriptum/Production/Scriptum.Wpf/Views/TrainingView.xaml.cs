using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Scriptum.Wpf.ViewModels;

namespace Scriptum.Wpf.Views;

public partial class TrainingView : UserControl
{
    public TrainingView()
    {
        InitializeComponent();
    }

    private TrainingViewModel? ViewModel => DataContext as TrainingViewModel;

    private void TrainingView_Loaded(object sender, RoutedEventArgs e)
    {
        Focusable = true;
        Focus();
        PreviewKeyDown += TrainingView_PreviewKeyDown;
        PreviewKeyUp += TrainingView_PreviewKeyUp;
    }

    private void TrainingView_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        ViewModel?.OnKeyDown(e);
        e.Handled = true;
    }

    private void TrainingView_PreviewKeyUp(object sender, KeyEventArgs e)
    {
        ViewModel?.OnKeyUp(e);
        e.Handled = true;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.NavigateBack();
    }

    private void ToggleGuide_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.ToggleGuide();
    }
}
