using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TypeTutor.Logic.Core;

namespace TypeTutor.WPF
{
    public sealed class TypingTextViewModel : INotifyPropertyChanged
    {
        private readonly ITypingEngine _engine;
        public event PropertyChangedEventHandler? PropertyChanged;

        public TypingTextViewModel(ITypingEngine engine)
        {
            _engine = engine;
        }

        public string TargetText => _engine.State.TargetText;
        public string InputText => _engine.State.InputText;
        public string Summary => $"Länge Soll: {TargetText.Length} | Länge Ist: {InputText.Length}";

        // Trigger flag used by the view to show a short highlight animation
        public bool JustLoaded { get; private set; } = false;

        public void Refresh()
        {
            OnPropertyChanged(nameof(TargetText));
            OnPropertyChanged(nameof(InputText));
            OnPropertyChanged(nameof(Summary));
        }

        // Call this to flash the view briefly when a new lesson is loaded
        public async void PulseOnLoad()
        {
            JustLoaded = true;
            OnPropertyChanged(nameof(JustLoaded));

            // keep the highlight for a short time, then clear
            await Task.Delay(700);

            JustLoaded = false;
            OnPropertyChanged(nameof(JustLoaded));
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
