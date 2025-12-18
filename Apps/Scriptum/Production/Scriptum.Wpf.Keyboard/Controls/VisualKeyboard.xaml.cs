using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Scriptum.Wpf.Keyboard.ViewModels;

namespace Scriptum.Wpf.Keyboard.Controls;

/// <summary>
/// UserControl für die visuelle deutsche QWERTZ-Tastatur.
/// </summary>
public partial class VisualKeyboard : UserControl
{
    public VisualKeyboard()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        TrySetupGridDefinitions();
    }

    private void OnDataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        TrySetupGridDefinitions();
    }

    private void TrySetupGridDefinitions()
    {
        if (DataContext is not VisualKeyboardViewModel vm)
            return;

        KeyboardItems.ApplyTemplate();

        var presenter = FindDescendant<ItemsPresenter>(KeyboardItems);
        if (presenter == null)
            return;

        presenter.ApplyTemplate();

        if (VisualTreeHelper.GetChildrenCount(presenter) == 0)
            return;

        if (VisualTreeHelper.GetChild(presenter, 0) is not Grid panel)
            return;

        panel.RowDefinitions.Clear();
        panel.ColumnDefinitions.Clear();

        for (int r = 0; r < vm.RowCount; r++)
            panel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        const double columnWidthFactor = 1.4;
        for (int c = 0; c < vm.ColumnCount; c++)
            panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(columnWidthFactor, GridUnitType.Star) });
    }

    private static T? FindDescendant<T>(DependencyObject root) where T : DependencyObject
    {
        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T typed) return typed;
            var result = FindDescendant<T>(child);
            if (result != null) return result;
        }
        return null;
    }
}
