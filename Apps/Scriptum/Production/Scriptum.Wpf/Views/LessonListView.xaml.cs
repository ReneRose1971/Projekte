using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Scriptum.Wpf.Projections;
using Scriptum.Wpf.ViewModels;

namespace Scriptum.Wpf.Views;

public partial class LessonListView : UserControl
{
    public LessonListView()
    {
        InitializeComponent();
    }

    private LessonListViewModel? ViewModel => DataContext as LessonListViewModel;

    private void LessonList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is LessonListItem lesson)
        {
            ViewModel?.StartTraining(lesson);
        }
    }

    private void DetailsButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is LessonListItem lesson)
        {
            ViewModel?.ShowDetails(lesson);
        }
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is LessonListItem lesson)
        {
            ViewModel?.StartTraining(lesson);
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.NavigateBack();
    }
}
