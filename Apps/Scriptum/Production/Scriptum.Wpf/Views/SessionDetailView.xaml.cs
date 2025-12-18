using System.Windows;
using System.Windows.Controls;
using Scriptum.Wpf.ViewModels;

namespace Scriptum.Wpf.Views;

public partial class SessionDetailView : UserControl
{
    public SessionDetailView()
    {
        InitializeComponent();
    }

    private SessionDetailViewModel? ViewModel => DataContext as SessionDetailViewModel;

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.NavigateBack();
    }
}
