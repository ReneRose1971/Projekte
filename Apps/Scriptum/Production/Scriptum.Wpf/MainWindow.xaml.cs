using System;
using System.Windows;
using System.Windows.Input;
using Scriptum.Wpf.Navigation;
using Scriptum.Wpf.ViewModels;

namespace Scriptum.Wpf;

public partial class MainWindow : Window
{
    private readonly ShellViewModel _shellViewModel;

    public MainWindow(ShellViewModel shellViewModel)
    {
        InitializeComponent();
        _shellViewModel = shellViewModel;
        DataContext = _shellViewModel;
        _shellViewModel.ShowHome();
    }

    private void NavigationButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: NavigationItem navItem }) return;
        navItem.Action?.Invoke();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        _shellViewModel.GoBack();
    }
}
