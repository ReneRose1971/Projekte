using System.Windows;
using System.Windows.Input;
using Scriptum.Wpf.Keyboard.ViewModels;

namespace Scriptum.Wpf;

public partial class MainWindow : Window
{
    private readonly VisualKeyboardViewModel _keyboardViewModel;

    public MainWindow()
    {
        InitializeComponent();
        
        _keyboardViewModel = new VisualKeyboardViewModel();
        KeyboardControl.DataContext = _keyboardViewModel;
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var label = MapKeyToLabel(e.Key, e.KeyboardDevice);
        if (!string.IsNullOrEmpty(label))
        {
            _keyboardViewModel.SetPressed(label, true);
            
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                _keyboardViewModel.IsShiftActive = true;
            
            if (e.Key == Key.RightAlt)
                _keyboardViewModel.IsAltGrActive = true;
        }
    }

    private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
    {
        var label = MapKeyToLabel(e.Key, e.KeyboardDevice);
        if (!string.IsNullOrEmpty(label))
        {
            _keyboardViewModel.SetPressed(label, false);
            
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                _keyboardViewModel.IsShiftActive = false;
            
            if (e.Key == Key.RightAlt)
                _keyboardViewModel.IsAltGrActive = false;
        }
    }

    private static string? MapKeyToLabel(Key key, KeyboardDevice keyboard)
    {
        if (key >= Key.A && key <= Key.Z)
            return ((char)('A' + (key - Key.A))).ToString();

        if (key >= Key.D0 && key <= Key.D9)
            return ((char)('0' + (key - Key.D0))).ToString();

        if (key >= Key.NumPad0 && key <= Key.NumPad9)
            return ((char)('0' + (key - Key.NumPad0))).ToString();

        return key switch
        {
            Key.Space => "Space",
            Key.Enter or Key.Return => "Enter",
            Key.Back => "Backspace",
            Key.Tab => "Tab",
            Key.Escape => "Esc",
            Key.OemComma => ",",
            Key.OemPeriod => ".",
            Key.OemMinus => "-",
            Key.OemPlus => "+",
            Key.Oem102 => "< > |",
            Key.OemOpenBrackets => "Ü",
            Key.OemCloseBrackets => "+",
            Key.Oem1 => "Ü",
            Key.Oem3 => "Ö",
            Key.Oem5 => "ß",
            Key.Oem7 => "Ä",
            Key.Oem2 => "#",
            Key.CapsLock => "?",
            Key.LeftShift or Key.RightShift => "Shift",
            Key.LeftCtrl or Key.RightCtrl => "Ctrl",
            Key.LeftAlt => "Alt",
            Key.RightAlt => "AltGr",
            Key.LWin or Key.RWin => "Win",
            Key.Apps => "Menu",
            Key.Left => "?",
            Key.Right => "?",
            Key.Up => "?",
            Key.Down => "?",
            Key.Insert => "Ins",
            Key.Delete => "Del",
            Key.Home => "Home",
            Key.End => "End",
            Key.PageUp => "PgUp",
            Key.PageDown => "PgDn",
            Key.PrintScreen => "PrtSc",
            Key.Scroll => "ScrLk",
            Key.Pause => "Pause",
            Key.Decimal => ".",
            Key.Add => "+",
            Key.Subtract => "-",
            Key.Multiply => "*",
            Key.Divide => "/",
            Key.F1 => "F1",
            Key.F2 => "F2",
            Key.F3 => "F3",
            Key.F4 => "F4",
            Key.F5 => "F5",
            Key.F6 => "F6",
            Key.F7 => "F7",
            Key.F8 => "F8",
            Key.F9 => "F9",
            Key.F10 => "F10",
            Key.F11 => "F11",
            Key.F12 => "F12",
            _ => null
        };
    }
}
