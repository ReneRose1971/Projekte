// src/TypeTutor.WPF/LessonEditorWindow.xaml.cs
using System.Windows;
using System.Windows.Input;

namespace TypeTutor.WPF
{
    public partial class LessonEditorWindow : Window
    {
        public LessonEditorWindow(LessonEditorViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            this.Loaded += (s, e) =>
            {
                if (this.Content is FrameworkElement root)
                {
                    var child = FindChild<LessonListView>(root);
                    if (child != null)
                    {
                        child.ItemDoubleClick += OnListDoubleClick;
                    }
                }
            };
        }

        private void OnListDoubleClick(object? sender, MouseButtonEventArgs e)
        {
            if (DataContext is LessonEditorViewModel vm)
            {
                // execute the command to load the selected lesson into the editor
                vm.CmdLoadFromSelection.Execute(null);
            }
        }

        private void OnMinimize(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void OnToggleMaximize(object sender, RoutedEventArgs e) => WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        private void OnClose(object sender, RoutedEventArgs e) => Close();

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var found = FindChild<T>(child);
                if (found != null) return found;
            }
            return null;
        }
    }
}
