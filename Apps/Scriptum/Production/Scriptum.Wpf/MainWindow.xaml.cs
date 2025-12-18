using System;
using System.Windows;
using System.Windows.Input;
using Scriptum.Wpf.ViewModels;

namespace Scriptum.Wpf;

public partial class MainWindow : Window
{
    public MainWindow(ShellViewModel shellViewModel)
    {
        InitializeComponent();
        DataContext = shellViewModel;
        shellViewModel.ShowHome();
    }
}
