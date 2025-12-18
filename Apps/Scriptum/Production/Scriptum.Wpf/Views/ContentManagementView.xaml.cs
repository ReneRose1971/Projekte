using System.Windows;
using System.Windows.Controls;
using Scriptum.Wpf.ViewModels;

namespace Scriptum.Wpf.Views;

public partial class ContentManagementView : UserControl
{
    public ContentManagementView()
    {
        InitializeComponent();
    }

    private ContentManagementViewModel? ViewModel => DataContext as ContentManagementViewModel;

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.NavigateBack();
    }
}
