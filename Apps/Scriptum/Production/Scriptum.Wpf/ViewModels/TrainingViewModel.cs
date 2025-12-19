using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using DataToolKit.Abstractions.DataStores;
using PropertyChanged;
using Scriptum.Application;
using Scriptum.Content.Data;
using Scriptum.Wpf.Commands;
using Scriptum.Wpf.Keyboard;
using Scriptum.Wpf.Keyboard.ViewModels;
using Scriptum.Wpf.Navigation;

namespace Scriptum.Wpf.ViewModels;

/// <summary>
/// ViewModel für die Trainingsansicht.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class TrainingViewModel : INotifyPropertyChanged
{
    private readonly INavigationService _navigationService;
    private readonly ITrainingSessionCoordinator _coordinator;
    private readonly IKeyboardInputHandler _keyboardInputHandler;
    private readonly VisualKeyboardViewModel _keyboardViewModel;
    private readonly IDataStore<LessonGuideData> _guideDataStore;

    public event PropertyChangedEventHandler? PropertyChanged;

    public TrainingViewModel(
        INavigationService navigationService,
        ITrainingSessionCoordinator coordinator,
        IKeyChordAdapter adapter,
        IKeyCodeMapper keyCodeMapper,
        VisualKeyboardViewModel keyboardViewModel,
        IDataStoreProvider dataStoreProvider)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        _keyboardViewModel = keyboardViewModel ?? throw new ArgumentNullException(nameof(keyboardViewModel));

        if (adapter == null)
            throw new ArgumentNullException(nameof(adapter));
        
        if (keyCodeMapper == null)
            throw new ArgumentNullException(nameof(keyCodeMapper));

        if (dataStoreProvider == null)
            throw new ArgumentNullException(nameof(dataStoreProvider));

        _guideDataStore = dataStoreProvider.GetDataStore<LessonGuideData>();

        _keyboardInputHandler = new TrainingKeyboardInputHandler(
            coordinator,
            adapter,
            keyCodeMapper,
            keyboardViewModel,
            OnStateChanged);

        NavigateBackCommand = new RelayCommand(_ => ExecuteNavigateBack());
        ToggleGuideCommand = new RelayCommand(_ => ExecuteToggleGuide());
    }

    public VisualKeyboardViewModel Keyboard => _keyboardViewModel;
    public string ModuleId { get; private set; } = string.Empty;
    public string LessonId { get; private set; } = string.Empty;
    public bool IsGuideVisible { get; set; } = false;

    public ICommand NavigateBackCommand { get; }
    public ICommand ToggleGuideCommand { get; }

    /// <summary>
    /// Trigger-Property für State-Updates. Wird inkrementiert, wenn sich der Coordinator-State ändert.
    /// Fody erkennt automatisch, dass alle Properties, die auf StateVersion zugreifen, aktualisiert werden müssen.
    /// </summary>
    [DoNotNotify]
    private int StateVersion { get; set; }
    
    [DependsOn(nameof(StateVersion), nameof(LessonId))
    ]
    public string GuideText
    {
        get
        {
            if (string.IsNullOrEmpty(LessonId))
                return "Keine Hilfe verfügbar";

            var guide = _guideDataStore.Items.FirstOrDefault(g => g.LessonId == LessonId);
            if (guide == null || string.IsNullOrWhiteSpace(guide.GuideTextMarkdown))
                return "Keine Hilfe für diese Lektion vorhanden.";

            return guide.GuideTextMarkdown;
        }
    }
    
    [DependsOn(nameof(StateVersion))]
    public string DisplayTarget
    {
        get
        {
            _ = StateVersion; // Force dependency tracking
            
            if (_coordinator.CurrentState?.Sequence == null)
                return "Keine Lektion geladen";

            var symbols = _coordinator.CurrentState.Sequence.Symbols;
            if (symbols == null || symbols.Count == 0)
                return "Leere Lektion";

            return string.Join("", symbols.Select(s => s.Graphem));
        }
    }

    [DependsOn(nameof(StateVersion))]
    public string DisplayInput
    {
        get
        {
            _ = StateVersion; // Force dependency tracking
            
            if (_coordinator.CurrentSession?.Inputs == null || _coordinator.CurrentSession.Inputs.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var input in _coordinator.CurrentSession.Inputs)
            {
                if (!string.IsNullOrEmpty(input.ErzeugtesGraphem))
                    sb.Append(input.ErzeugtesGraphem);
            }
            return sb.ToString();
        }
    }
    
    [DependsOn(nameof(StateVersion))]
    public int CurrentIndex
    {
        get
        {
            _ = StateVersion; // Force dependency tracking
            return _coordinator.CurrentState?.CurrentTargetIndex ?? 0;
        }
    }
    
    [DependsOn(nameof(StateVersion))]
    public bool IsCompleted
    {
        get
        {
            _ = StateVersion; // Force dependency tracking
            return _coordinator.CurrentSession?.IsCompleted ?? false;
        }
    }
    
    [DependsOn(nameof(StateVersion))]
    public int ErrorCount
    {
        get
        {
            _ = StateVersion; // Force dependency tracking
            return _coordinator.CurrentSession?.Evaluations?.Count ?? 0;
        }
    }
    
    [DependsOn(nameof(StateVersion), nameof(IsCompleted), nameof(CurrentIndex), nameof(ErrorCount))]
    public string StatusText => IsCompleted ? "Lektion abgeschlossen!" : $"Position: {CurrentIndex}, Fehler: {ErrorCount}";

    public void Initialize(string moduleId, string lessonId)
    {
        ModuleId = moduleId;
        LessonId = lessonId;

        try
        {
            _coordinator.StartSession(moduleId, lessonId);
            System.Diagnostics.Debug.WriteLine($"Training Session gestartet: {moduleId}/{lessonId}");
            
            OnStateChanged();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Starten der Session: {ex.Message}");
        }
    }

    public void OnKeyDown(KeyEventArgs e)
    {
        var wasProcessed = _keyboardInputHandler.HandleKeyDown(e);
        
        if (wasProcessed && IsCompleted)
        {
            _navigationService.NavigateToTrainingSummary();
        }
    }

    public void OnKeyUp(KeyEventArgs e)
    {
        _keyboardInputHandler.HandleKeyUp(e);
    }

    [Obsolete("Use NavigateBackCommand instead")]
    public void NavigateBack()
    {
        ExecuteNavigateBack();
    }

    [Obsolete("Use ToggleGuideCommand instead")]
    public void ToggleGuide()
    {
        ExecuteToggleGuide();
    }

    private void ExecuteNavigateBack()
    {
        _navigationService.NavigateToLessonDetails(ModuleId, LessonId);
    }

    private void ExecuteToggleGuide()
    {
        IsGuideVisible = !IsGuideVisible;
    }

    /// <summary>
    /// Wird aufgerufen, wenn sich der Coordinator-State ändert.
    /// Triggert automatisch PropertyChanged für alle abhängigen Properties.
    /// </summary>
    private void OnStateChanged()
    {
        StateVersion++;
    }
}
