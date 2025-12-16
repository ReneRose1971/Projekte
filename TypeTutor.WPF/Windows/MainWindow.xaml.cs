// src/TypeTutor.WPF/MainWindow.xaml.cs
using System.Windows;
using System.Windows.Input;
using TypeTutor.Logic.Core;
using System.Threading.Tasks;

namespace TypeTutor.WPF
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;
        private readonly KeyboardAdapter _adapter;

        public MainWindow(MainViewModel vm, KeyboardAdapter adapter)
        {
            InitializeComponent();
            DataContext = _vm = vm;
            _adapter = adapter;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Visual highlight: show key as pressed but do NOT mark handled here for printable keys
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            var code = KeyboardAdapter.MapKey(key);
            if (code != KeyCode.None)
            {
                _vm.VisualKeyboardVM?.SetPressed(code, true);
            }

            // Ask adapter whether this key produces a non-printable stroke (Enter/Backspace/etc.).
            var stroke = _adapter.FromKeyDown(e);
            if (stroke.HasValue)
            {
                // Non-printable key: process immediately and mark handled to avoid default control behavior
                _vm.Process(stroke.Value);
                e.Handled = true;
            }

            // Printable keys (letters, digits, space, oem) will raise TextInput; do not swallow them here.
        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            var code = KeyboardAdapter.MapKey(key);
            if (code != KeyCode.None)
            {
                _vm.VisualKeyboardVM?.SetPressed(code, false);
            }

            // Do not set e.Handled here for printable keys; TextInput already handled in OnTextInput.
        }

        private async void OnTextInput(object sender, TextCompositionEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Text))
                return;

            var stroke = _adapter.FromTextInput(e);

            // Highlight the visual key by the produced character (fallback for OEM mismatches)
            try
            {
                _vm.VisualKeyboardVM?.SetPressedByLabel(e.Text, true);
                _vm.Process(stroke);

                // short visual press then release
                await Task.Delay(120);
                _vm.VisualKeyboardVM?.SetPressedByLabel(e.Text, false);
            }
            finally
            {
                // Prevent default control behavior for text input (e.g. Space activating buttons)
                e.Handled = true;
            }
        }

        private void OnExitClick(object sender, RoutedEventArgs e) => Close();

        // Titlebar actions
        private void OnMinimize(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        private void OnToggleMaximize(object sender, RoutedEventArgs e) =>
            WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;

        private void OnClose(object sender, RoutedEventArgs e) => Close();

        // Titelzeile: Drag + Doppelklick (Maximize/Restore) zuverlässig über ClickCount
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            if (e.ClickCount == 2)
            {
                OnToggleMaximize(sender, new RoutedEventArgs());
                e.Handled = true;
                return;
            }

            try
            {
                DragMove();
            }
            catch
            {
                // Kann in bestimmten Fenstereinstellungen werfen (z.B. während State-Transition) – bewusst ignorieren
            }
        }
    }
}
