using System;
using System.Windows.Input;
using PropertyChanged;
using Scriptum.Application;
using Scriptum.Core;
using Scriptum.Wpf.Keyboard.ViewModels;
using Scriptum.Wpf.Navigation;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für die Trainingsansicht.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class TrainingViewModel
{
    private readonly INavigationService _navigationService;
    private readonly ITrainingSessionCoordinator _coordinator;
    private readonly IKeyChordAdapter _adapter;
    private readonly VisualKeyboardViewModel _keyboardViewModel;

    public TrainingViewModel(
        INavigationService navigationService,
        ITrainingSessionCoordinator coordinator,
        IKeyChordAdapter adapter,
        VisualKeyboardViewModel keyboardViewModel)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _keyboardViewModel = keyboardViewModel ?? throw new ArgumentNullException(nameof(keyboardViewModel));
    }

    public VisualKeyboardViewModel Keyboard => _keyboardViewModel;
    public string ModuleId { get; private set; } = string.Empty;
    public string LessonId { get; private set; } = string.Empty;
    public string DisplayTarget => "TODO: TargetSequence anzeigen";
    public int CurrentIndex => _coordinator.CurrentState?.CurrentTargetIndex ?? 0;
    public bool IsCompleted => _coordinator.CurrentSession?.IsCompleted ?? false;
    public int ErrorCount => _coordinator.CurrentSession?.Evaluations?.Count ?? 0;
    public string StatusText => IsCompleted ? "Lektion abgeschlossen!" : $"Position: {CurrentIndex}, Fehler: {ErrorCount}";

    public void Initialize(string moduleId, string lessonId)
    {
        ModuleId = moduleId;
        LessonId = lessonId;

        try
        {
            _coordinator.StartSession(moduleId, lessonId);
            System.Diagnostics.Debug.WriteLine($"Training Session gestartet: {moduleId}/{lessonId}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Starten der Session: {ex.Message}");
        }
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

        if (!_coordinator.IsSessionRunning || IsCompleted)
            return;

        if (_adapter.TryCreateChord(e, out var chord))
        {
            try
            {
                var evaluation = _coordinator.ProcessInput(chord);
                if (evaluation != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Evaluation: {evaluation.Outcome}");
                }

                if (IsCompleted)
                {
                    _navigationService.NavigateToTrainingSummary();
                }
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

    public void NavigateBack()
    {
        _navigationService.NavigateToLessonDetails(ModuleId, LessonId);
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
