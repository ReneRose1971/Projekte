using System.Windows;
using System.Windows.Controls;
using Scriptum.Wpf.ViewModels;

namespace Scriptum.Wpf.Views;

public partial class LessonDetailsView : UserControl
{
    public LessonDetailsView()
    {
        InitializeComponent();
    }

    private LessonDetailsViewModel? ViewModel => DataContext as LessonDetailsViewModel;

    private void GuideButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.ShowGuide();
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.StartTraining();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.NavigateBack();
    }
}
