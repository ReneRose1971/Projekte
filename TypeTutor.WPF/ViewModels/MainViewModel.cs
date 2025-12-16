using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TypeTutor.Logic.Core;
using TTVisualKeyboard.ViewModels;
using System;
using System.Linq;

namespace TypeTutor.WPF
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private readonly ITypingEngine _engine;
        private readonly ILessonRepository _repo;

        public event PropertyChangedEventHandler? PropertyChanged;

        public LessonMenuViewModel LessonMenuVM { get; }
        public TypingTextViewModel TypingTextVM { get; }
        public KeyboardViewModel KeyboardVM { get; }
        public TypingEngineStateViewModel EngineStateVM { get; }

        public VisualKeyboardViewModel VisualKeyboardVM { get; }

        public ObservableCollection<Lesson> Lessons { get; } = new();

        private Lesson? _selected;
        public Lesson? Selected
        {
            get => _selected;
            set
            {
                if (_selected == value) return;
                _selected = value;
                OnPropertyChanged();
                LoadSelectedLessonIntoEngine();
            }
        }

        public string CompletionMessage { get; private set; } = string.Empty;

        public RelayCommand CmdReloadLessons { get; }
        public RelayCommand CmdOpenLessonsFolder { get; }

        public MainViewModel(ITypingEngine engine, ILessonRepository repo, LessonMenuViewModel lessonMenuVM, TypingTextViewModel typingTextVM, KeyboardViewModel keyboardVM, TypingEngineStateViewModel engineStateVM, LessonListViewModel sharedListVm, VisualKeyboardViewModel visualKeyboardVM)
        {
            _engine = engine;
            _repo = repo;
            LessonMenuVM = lessonMenuVM;
            TypingTextVM = typingTextVM;
            KeyboardVM = keyboardVM;
            EngineStateVM = engineStateVM;
            VisualKeyboardVM = visualKeyboardVM;

            // Note: VisualKeyboardViewModel currently does not expose KeyClicked/SetPressed
            // so we do not subscribe to events here to keep compilation independent from the visual control implementation.

            // Keep the main Lessons list in sync with the shared LessonListViewModel
            if (sharedListVm != null)
            {
                // initialize from shared list
                foreach (var l in sharedListVm.Items) Lessons.Add(l);

                // subscribe to changes
                sharedListVm.Items.CollectionChanged += OnSharedLessonsChanged;
            }

            // Wenn das Menü eine Lesson auswählt (z.B. über den Browser/Window),
            // laden wir diese sofort in die Engine (brechen ggf. laufende Lesson ab).
            LessonMenuVM.LessonPicked += (_, selected) =>
            {
                if (selected != null)
                {
                    Selected = selected; // führt zu Reset der Engine via LoadSelectedLessonIntoEngine
                }
            };

            _engine.LessonCompleted += success => { CompletionMessage = success ? "Lesson erfolgreich abgeschlossen." : "Lesson mit Fehlern abgeschlossen."; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CompletionMessage))); };

            CmdReloadLessons = new RelayCommand(async () => await ReloadLessonsAsync());
            CmdOpenLessonsFolder = new RelayCommand(() => { });

            _ = ReloadLessonsAsync();
        }

        private void OnSharedLessonsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Determine whether the currently selected lesson was removed (or no longer present)
            bool selectedRemoved = false;

            if (Selected is not null)
            {
                if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
                {
                    foreach (var old in e.OldItems ?? Array.Empty<object>())
                    {
                        if (old is Lesson removed && string.Equals(removed.Meta.Title, Selected.Meta.Title, StringComparison.OrdinalIgnoreCase))
                        {
                            selectedRemoved = true;
                            break;
                        }
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    // After reset the shared collection may be empty – check presence
                    bool found = false;
                    if (sender is System.Collections.IEnumerable coll)
                    {
                        foreach (var o in coll)
                        {
                            if (o is Lesson l && string.Equals(l.Meta.Title, Selected.Meta.Title, StringComparison.OrdinalIgnoreCase))
                            {
                                found = true; break;
                            }
                        }
                    }
                    if (!found) selectedRemoved = true;
                }
            }

            // Resync entire collection to keep ordering consistent
            Lessons.Clear();
            if (sender is System.Collections.IEnumerable list)
            {
                foreach (var item in list)
                {
                    if (item is Lesson l) Lessons.Add(l);
                }
            }

            // If the previously selected lesson was removed, choose a sensible fallback.
            if (selectedRemoved)
            {
                if (Lessons.Count > 0)
                {
                    // Select first available lesson (loads it into engine)
                    Selected = Lessons[0];
                }
                else
                {
                    // No lessons left: clear selection and reset engine to welcome state
                    Selected = null;
                    _engine.Reset("Willkommen bei TypeTutor — kein Lesson-Content vorhanden!");
                    TypingTextVM.Refresh();
                    EngineStateVM.Refresh();
                }

                return;
            }

            // Try to keep the same logical selection if possible: match by title and reassign Selected
            if (Selected is not null)
            {
                var matched = Lessons.FirstOrDefault(l => string.Equals(l.Meta.Title, Selected.Meta.Title, StringComparison.OrdinalIgnoreCase));
                if (matched is not null && !ReferenceEquals(matched, Selected))
                {
                    // Re-assign to the instance from the refreshed collection (this will call LoadSelectedLessonIntoEngine)
                    Selected = matched;
                    return;
                }
            }

            // ensure selection remains meaningful
            if (Lessons.Count > 0 && Selected is null)
                Selected = Lessons[0];
        }

        public void Process(TypeTutor.Logic.Core.KeyStroke stroke)
        {
            _engine.Process(stroke);
            UpdateFromState();
            TypingTextVM.Refresh();
            EngineStateVM.Refresh();

            // Do not pulse visual keys here. Visual highlighting is controlled by KeyDown/KeyUp handlers
            // in the MainWindow so that pressed/held keys are shown accurately.
        }

        private void UpdateFromState()
        {
            OnPropertyChanged(nameof(CompletionMessage));
        }

        private async Task ReloadLessonsAsync()
        {
            Lessons.Clear();
            var list = await _repo.LoadAllAsync();
            foreach (var lesson in list) Lessons.Add(lesson);

            if (Lessons.Count > 0)
            {
                Selected = Lessons[0];
            }
            else
            {
                // Fallback text if no lessons exist
                _engine.Reset("Willkommen bei TypeTutor ? kein Lesson-Content vorhanden!");
                TypingTextVM.Refresh();
                EngineStateVM.Refresh();
            }
        }

        private void LoadSelectedLessonIntoEngine()
        {
            if (Selected is null) return;

            // Reset the engine to abort any running lesson and load the new target
            _engine.Reset(Selected.TargetText);
            TypingTextVM.Refresh();
            EngineStateVM.Refresh();

            // Trigger a short UI pulse to indicate loading
            TypingTextVM.PulseOnLoad();

            // clear completion message
            CompletionMessage = string.Empty;
            OnPropertyChanged(nameof(CompletionMessage));
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
