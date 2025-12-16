using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TypeTutor.Logic.Core;

namespace TypeTutor.WPF
{
    public sealed class TypingEngineStateViewModel : INotifyPropertyChanged
    {
        private readonly ITypingEngine _engine;
        public event PropertyChangedEventHandler? PropertyChanged;

        public TypingEngineStateViewModel(ITypingEngine engine)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            CmdRestart = new RelayCommand(() => { _engine.Reset(_engine.State.TargetText); Refresh(); });
            _engine.LessonCompleted += success =>
            {
                CompletionSuccess = success;
                CompletionMessage = success ? "Lesson erfolgreich abgeschlossen." : "Lesson mit Fehlern abgeschlossen.";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CompletionSuccess)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CompletionMessage)));
            };
        }

        public TypingEngineState State => _engine.State;
        public int Prefix => State.CorrectPrefixLength;
        public int Errors => State.ErrorCount;
        public int NextIndex => State.NextIndex;
        public string Expected => State.ExpectedNextChar?.ToString() ?? "—";
        public string LastInput => State.LastInputChar?.ToString() ?? "—";
        public bool IsComplete => State.IsComplete;
        public int TargetLength => State.TargetText.Length;
        public int InputLength => State.InputText.Length;
        public double ProgressPercent => TargetLength > 0 ? Math.Clamp(100.0 * NextIndex / TargetLength, 0.0, 100.0) : 0.0;
        public string CompletionMessage { get; private set; } = string.Empty;
        public bool CompletionSuccess { get; private set; } = false;
        public RelayCommand CmdRestart { get; }

        public void Refresh()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prefix)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Errors)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NextIndex)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Expected)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastInput)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsComplete)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetLength)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InputLength)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProgressPercent)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CompletionMessage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CompletionSuccess)));
        }
    }
}
