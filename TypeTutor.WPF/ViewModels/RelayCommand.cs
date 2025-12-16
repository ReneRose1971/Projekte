// src/TypeTutor.WPF/RelayCommand.cs
using System;
using System.Windows.Input;

namespace TypeTutor.WPF
{
    public sealed class RelayCommand : ICommand
    {
        private readonly Action _exec;
        private readonly Func<bool>? _can;

        public RelayCommand(Action exec, Func<bool>? can = null)
        {
            _exec = exec ?? throw new ArgumentNullException(nameof(exec));
            _can = can;
        }

        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => _can?.Invoke() ?? true;
        public void Execute(object? parameter) => _exec();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
