// src/TypeTutor.WPF/LessonsWindow.xaml.cs
using System;
using System.Windows;
using System.Windows.Input;
using System.Linq;

namespace TypeTutor.WPF
{
    public partial class LessonsWindow : Window
    {
        public LessonsWindowViewModel VM { get; }

        public LessonsWindow(LessonsWindowViewModel vm)
        {
            InitializeComponent();
            DataContext = VM = vm;

            this.Loaded += (s, e) =>
            {
                if (this.Content is FrameworkElement root)
                {
                    var child = FindChild<LessonListView>(root);
                    if (child != null) child.ItemDoubleClick += OnListDoubleClick;
                }
            };
        }

        private async void OnListDoubleClick(object? sender, MouseButtonEventArgs e)
        {
            VM.ConfirmSelection();

            // Open a ModuleGuide details popup for the selected lesson's ModuleId (if any)
            var picked = VM.Picked;
            if (picked != null)
            {
                var moduleId = picked.Meta.ModuleId;
                if (!string.IsNullOrWhiteSpace(moduleId))
                {
                    try
                    {
                        // Try to open a lightweight details-only window first
                        var detailsFactory = App.Services.GetService(typeof(Func<ModuleGuideDetailsWindow>)) as Func<ModuleGuideDetailsWindow>;
                        var mgVm = App.Services.GetService(typeof(ModuleGuideViewModel)) as ModuleGuideViewModel;

                        if (mgVm != null)
                        {
                            await mgVm.LoadAsync();
                            var match = mgVm.Items.FirstOrDefault(m => string.Equals(m.Title, moduleId, StringComparison.OrdinalIgnoreCase));
                            if (match == null)
                            {
                                MessageBox.Show($"Kein ModuleGuide für Modul '{moduleId}' gefunden.", "ModuleGuide fehlt", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                if (detailsFactory != null)
                                {
                                    var w = detailsFactory();
                                    // details window uses ModuleGuideDetailsView which reads ModuleGuideViewModel as DataContext
                                    w.DataContext = mgVm;
                                    mgVm.SelectedItem = match;
                                    w.Owner = Application.Current?.MainWindow ?? this;
                                    w.Show();
                                }
                                else
                                {
                                    // fallback to opening full editor window
                                    var factory = App.Services.GetService(typeof(Func<ModuleGuideWindow>)) as Func<ModuleGuideWindow>;
                                    if (factory != null)
                                    {
                                        var win = factory();
                                        try { win.ListVM.SelectedItem = match; } catch { }
                                        win.Owner = Application.Current?.MainWindow ?? this;
                                        win.Show();
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }
            }

            Close();
        }

        private void OnClose(object sender, RoutedEventArgs e) => Close();
        private void OnMinimize(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void OnToggleMaximize(object sender, RoutedEventArgs e) => WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;

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
