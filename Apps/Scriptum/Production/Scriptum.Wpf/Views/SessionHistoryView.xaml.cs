using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Scriptum.Wpf.Projections;
using Scriptum.Wpf.ViewModels;

namespace Scriptum.Wpf.Views;

public partial class SessionHistoryView : UserControl
{
    public SessionHistoryView()
    {
        InitializeComponent();
    }

    private SessionHistoryViewModel? ViewModel => DataContext as SessionHistoryViewModel;

    private void SessionList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is SessionListItem session)
        {
            ViewModel?.SelectSession(session);
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.NavigateBack();
    }
}
