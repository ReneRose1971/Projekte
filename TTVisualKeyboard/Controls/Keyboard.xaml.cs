using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; // <— wichtig!
using TTVisualKeyboard.ViewModels;

namespace TTVisualKeyboard.Views
{
    public partial class Keyboard : UserControl
    {
        public Keyboard()
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

            // Template instanziieren
            KeyboardItems.ApplyTemplate();

            // 1) ItemsPresenter im VisualTree suchen
            var presenter = FindDescendant<ItemsPresenter>(KeyboardItems);
            if (presenter == null)
                return;

            presenter.ApplyTemplate();

            // 2) Panel (unser Grid) ist das Kind des ItemsPresenters
            if (VisualTreeHelper.GetChildrenCount(presenter) == 0)
                return;

            if (VisualTreeHelper.GetChild(presenter, 0) is not Grid panel)
                return;

            // 3) Grid-Definitionen setzen
            panel.RowDefinitions.Clear();
            panel.ColumnDefinitions.Clear();

            for (int r = 0; r < vm.RowCount; r++)
                panel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Increase column star weight slightly so keys appear more square
            const double columnWidthFactor = 1.4; // adjusted from 1.2 to increase width
            for (int c = 0; c < vm.ColumnCount; c++)
                panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(columnWidthFactor, GridUnitType.Star) });
        }

        // Hilfsfunktion: generischer Visual-Tree-Search
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
}
