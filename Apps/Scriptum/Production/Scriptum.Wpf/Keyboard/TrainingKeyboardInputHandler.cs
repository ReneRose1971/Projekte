using System;
using System.Windows.Input;
using Scriptum.Application;
using Scriptum.Core;
using Scriptum.Engine;
using Scriptum.Wpf.Keyboard.ViewModels;

namespace Scriptum.Wpf.Keyboard;

/// <summary>
/// Standard-Implementierung des Keyboard-Input-Handlers.
/// Koordiniert Keyboard-Visualisierung und Training-Input-Verarbeitung.
/// </summary>
public sealed class TrainingKeyboardInputHandler : IKeyboardInputHandler
{
    private readonly ITrainingSessionCoordinator _coordinator;
    private readonly IKeyChordAdapter _adapter;
    private readonly IKeyCodeMapper _keyCodeMapper;
    private readonly VisualKeyboardViewModel _keyboardViewModel;
    private readonly Action _onInputProcessed;

    /// <summary>
    /// Erstellt einen neuen TrainingKeyboardInputHandler.
    /// </summary>
    /// <param name="coordinator">Der Training-Session-Coordinator.</param>
    /// <param name="adapter">Der Key-Chord-Adapter.</param>
    /// <param name="keyCodeMapper">Der Key-Code-Mapper.</param>
    /// <param name="keyboardViewModel">Das Visual-Keyboard-ViewModel.</param>
    /// <param name="onInputProcessed">Callback nach erfolgreicher Input-Verarbeitung.</param>
    public TrainingKeyboardInputHandler(
        ITrainingSessionCoordinator coordinator,
        IKeyChordAdapter adapter,
        IKeyCodeMapper keyCodeMapper,
        VisualKeyboardViewModel keyboardViewModel,
        Action onInputProcessed)
    {
        _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _keyCodeMapper = keyCodeMapper ?? throw new ArgumentNullException(nameof(keyCodeMapper));
        _keyboardViewModel = keyboardViewModel ?? throw new ArgumentNullException(nameof(keyboardViewModel));
        _onInputProcessed = onInputProcessed ?? throw new ArgumentNullException(nameof(onInputProcessed));
    }

    /// <summary>
    /// Behandelt einen KeyDown-Event.
    /// </summary>
    public bool HandleKeyDown(KeyEventArgs e)
    {
        if (e == null)
            throw new ArgumentNullException(nameof(e));

        UpdateKeyboardVisualization(e.Key, isPressed: true);

        if (!_coordinator.IsSessionRunning || _coordinator.CurrentSession?.IsCompleted == true)
            return false;

        if (_adapter.TryCreateChord(e, out var chord))
        {
            try
            {
                var evaluation = _coordinator.ProcessInput(chord);
                if (evaluation != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Evaluation: {evaluation.Outcome}");
                }

                _onInputProcessed();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fehler bei ProcessInput: {ex.Message}");
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Behandelt einen KeyUp-Event.
    /// </summary>
    public void HandleKeyUp(KeyEventArgs e)
    {
        if (e == null)
            throw new ArgumentNullException(nameof(e));

        UpdateKeyboardVisualization(e.Key, isPressed: false);
    }

    private void UpdateKeyboardVisualization(Key key, bool isPressed)
    {
        var label = _keyCodeMapper.MapToLabel(key);
        if (!string.IsNullOrEmpty(label))
        {
            _keyboardViewModel.SetPressed(label, isPressed);

            if (key == Key.LeftShift || key == Key.RightShift)
                _keyboardViewModel.IsShiftActive = isPressed;

            if (key == Key.RightAlt)
                _keyboardViewModel.IsAltGrActive = isPressed;
        }
    }
}
