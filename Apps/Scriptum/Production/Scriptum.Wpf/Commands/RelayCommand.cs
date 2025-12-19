using System;
using System.Windows.Input;

namespace Scriptum.Wpf.Commands;

/// <summary>
/// Eine einfache ICommand-Implementierung für synchrone Operationen.
/// </summary>
public sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    /// <summary>
    /// Erstellt ein neues RelayCommand.
    /// </summary>
    /// <param name="execute">Die auszuführende Aktion.</param>
    /// <param name="canExecute">Optionale Funktion zur Prüfung, ob das Command ausgeführt werden kann.</param>
    /// <exception cref="ArgumentNullException">Wird ausgelöst, wenn execute null ist.</exception>
    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Bestimmt, ob das Command im aktuellen Zustand ausgeführt werden kann.
    /// </summary>
    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke(parameter) ?? true;
    }

    /// <summary>
    /// Führt das Command aus.
    /// </summary>
    public void Execute(object? parameter)
    {
        _execute(parameter);
    }

    /// <summary>
    /// Tritt auf, wenn sich Änderungen ergeben, die sich darauf auswirken, ob das Command ausgeführt werden kann.
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
