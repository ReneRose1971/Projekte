using System;
using System.Windows;
using System.Windows.Input;

namespace Scriptum.Wpf;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        
        InitializeComponent();
        
        DataContext = _viewModel;
        KeyboardControl.DataContext = _viewModel.KeyboardViewModel;
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        _viewModel.OnKeyDown(e);
    }

    private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
    {
        _viewModel.OnKeyUp(e);
    }
}
