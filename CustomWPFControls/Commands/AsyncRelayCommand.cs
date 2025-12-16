using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CustomWPFControls.Commands
{
    /// <summary>
    /// Eine ICommand-Implementierung für asynchrone Operationen.
    /// Führt Tasks aus und unterstützt CanExecute-Logik.
    /// </summary>
    public sealed class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;

        /// <summary>
        /// Erstellt ein neues AsyncRelayCommand.
        /// </summary>
        /// <param name="execute">Die auszuführende asynchrone Aktion.</param>
        /// <param name="canExecute">Optionale Funktion zur Prüfung, ob das Command ausgeführt werden kann.</param>
        /// <exception cref="ArgumentNullException">Wird ausgelöst, wenn execute null ist.</exception>
        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Tritt auf, wenn sich Änderungen ergeben, die sich darauf auswirken, ob das Command ausgeführt werden kann.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Bestimmt, ob das Command im aktuellen Zustand ausgeführt werden kann.
        /// </summary>
        /// <param name="parameter">Parameter für das Command (wird ignoriert).</param>
        /// <returns>true, wenn das Command ausgeführt werden kann; andernfalls false.</returns>
        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        /// <summary>
        /// Führt das asynchrone Command aus.
        /// </summary>
        /// <param name="parameter">Parameter für das Command (wird ignoriert).</param>
        public async void Execute(object? parameter) => await _execute();
    }
}
