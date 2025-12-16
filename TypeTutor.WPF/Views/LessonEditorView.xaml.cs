using System;
using System.Windows;
using System.Windows.Controls;

namespace TypeTutor.WPF
{
    /// <summary>
    /// Interaktionslogik für LessonEditorView.xaml
    /// </summary>
    public partial class LessonEditorView : UserControl
    {
        public LessonEditorView()
        {
            InitializeComponent();
        }

        private void OnOpenModuleGuides(object sender, RoutedEventArgs e)
        {
            // Resolve window factory via DI
            var winFactory = App.Services.GetService(typeof(Func<ModuleGuideWindow>)) as Func<ModuleGuideWindow>;
            if (winFactory == null) return;
            var win = winFactory();
            win.Owner = Application.Current?.MainWindow;
            win.Show();
        }
    }
}
