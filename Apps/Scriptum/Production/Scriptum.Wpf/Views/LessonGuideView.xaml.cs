using System.Windows;
using System.Windows.Controls;
using Scriptum.Wpf.ViewModels;

namespace Scriptum.Wpf.Views;

public partial class LessonGuideView : UserControl
{
    public LessonGuideView()
    {
        InitializeComponent();
    }

    private LessonGuideViewModel? ViewModel => DataContext as LessonGuideViewModel;

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.NavigateBack();
    }
}
