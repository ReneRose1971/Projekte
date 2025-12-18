using System;
using System.Windows.Input;
using PropertyChanged;
using Scriptum.Application;
using Scriptum.Wpf.Keyboard.ViewModels;

namespace Scriptum.Wpf;

/// <summary>
/// ViewModel für das MainWindow.
/// Orchestriert Koordination zwischen Training, Adapter und visueller Tastatur.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class MainViewModel
{
    private readonly ITrainingSessionCoordinator _coordinator;
    private readonly IKeyChordAdapter _adapter;
    private readonly VisualKeyboardViewModel _keyboardViewModel;

    public VisualKeyboardViewModel KeyboardViewModel => _keyboardViewModel;

    public bool IsSessionRunning => _coordinator.IsSessionRunning;
    public bool IsSessionCompleted => _coordinator.CurrentSession?.IsCompleted ?? false;
    public int CurrentIndex => _coordinator.CurrentState?.CurrentTargetIndex ?? 0;
    public int ErrorCount => _coordinator.CurrentSession?.Evaluations?.Count ?? 0;
    public string StatusText => IsSessionRunning 
        ? (IsSessionCompleted ? "Lektion abgeschlossen!" : $"Index: {CurrentIndex}, Fehler: {ErrorCount}")
        : "Keine aktive Sitzung";

    public MainViewModel(
        ITrainingSessionCoordinator coordinator,
        IKeyChordAdapter adapter,
        VisualKeyboardViewModel keyboardViewModel)
    {
        _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _keyboardViewModel = keyboardViewModel ?? throw new ArgumentNullException(nameof(keyboardViewModel));

        StartDefaultSession();
    }

    public void OnKeyDown(KeyEventArgs e)
    {
        var label = MapKeyToLabel(e.Key);
        if (!string.IsNullOrEmpty(label))
        {
            _keyboardViewModel.SetPressed(label, true);

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                _keyboardViewModel.IsShiftActive = true;

            if (e.Key == Key.RightAlt)
                _keyboardViewModel.IsAltGrActive = true;
        }

        if (!IsSessionRunning)
            return;

        if (_adapter.TryCreateChord(e, out var chord))
        {
            try
            {
                var evaluation = _coordinator.ProcessInput(chord);
                
                System.Diagnostics.Debug.WriteLine(
                    evaluation != null 
                        ? $"Evaluation: {evaluation.Outcome}" 
                        : "Input ignored");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fehler bei ProcessInput: {ex.Message}");
            }
        }
    }

    public void OnKeyUp(KeyEventArgs e)
    {
        var label = MapKeyToLabel(e.Key);
        if (!string.IsNullOrEmpty(label))
        {
            _keyboardViewModel.SetPressed(label, false);

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                _keyboardViewModel.IsShiftActive = false;

            if (e.Key == Key.RightAlt)
                _keyboardViewModel.IsAltGrActive = false;
        }
    }

    private void StartDefaultSession()
    {
        try
        {
            _coordinator.StartSession("default-module", "default-lesson");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Starten der Session: {ex.Message}");
        }
    }

    private static string? MapKeyToLabel(Key key)
    {
        if (key >= Key.A && key <= Key.Z)
            return ((char)('A' + (key - Key.A))).ToString();

        if (key >= Key.D0 && key <= Key.D9)
            return ((char)('0' + (key - Key.D0))).ToString();

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
            Key.OemOpenBrackets => "ü",
            Key.OemCloseBrackets => "+",
            Key.Oem1 => "ö",
            Key.Oem3 => "ä",
            Key.Oem5 => "^",
            Key.Oem7 => "ß",
            Key.Oem2 => "#",
            Key.LeftShift or Key.RightShift => "Shift",
            Key.LeftCtrl or Key.RightCtrl => "Ctrl",
            Key.LeftAlt => "Alt",
            Key.RightAlt => "AltGr",
            _ => null
        };
    }
}
