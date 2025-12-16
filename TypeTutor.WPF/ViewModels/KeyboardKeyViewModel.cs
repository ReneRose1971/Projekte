// src/TypeTutor.WPF/KeyboardKeyViewModel.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TypeTutor.Logic.Core;

namespace TypeTutor.WPF
{
    /// <summary>
    /// Repräsentiert eine einzelne Taste in der UI.
    /// - UI-Properties: Label, ToolTip, IsPressed, IsWide
    /// - Technische Zuordnung: KeyCode (aus Core)
    /// </summary>
    public sealed class KeyboardKeyViewModel : INotifyPropertyChanged
    {
        private bool _isPressed;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Anzeigename auf der Taste (z. B. "A", "ß", "Alt").</summary>
        public string Label { get; }

        /// <summary>Optionaler Tooltip (z. B. zweites Zeichen mit Shift/AltGr).</summary>
        public string? ToolTip { get; }

        /// <summary>Zuordnung zur Domain-Taste für Highlighting/Mapping.</summary>
        public KeyCode Code { get; }

        /// <summary>Falls wahr, bekommt die Taste eine größere Mindestbreite.</summary>
        public bool IsWide { get; }

        /// <summary>Aktueller Press-Zustand (für Highlighting in der UI).</summary>
        public bool IsPressed
        {
            get => _isPressed;
            set
            {
                if (_isPressed == value) return;
                _isPressed = value;
                OnPropertyChanged();
            }
        }

        public KeyboardKeyViewModel(string label, KeyCode code, bool isWide = false, string? toolTip = null)
        {
            Label = label;
            Code = code;
            IsWide = isWide;
            ToolTip = toolTip;
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
