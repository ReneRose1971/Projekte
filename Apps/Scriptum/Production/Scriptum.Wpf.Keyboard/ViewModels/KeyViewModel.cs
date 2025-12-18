using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Scriptum.Wpf.Keyboard.ViewModels;

/// <summary>
/// Repräsentiert eine einzelne Taste auf der visuellen Tastatur.
/// </summary>
public sealed class KeyViewModel : INotifyPropertyChanged
{
    public int Row { get; init; }
    public int Column { get; init; }
    public int RowSpan { get; init; } = 1;
    public int ColSpan { get; init; } = 1;

    public string LabelPrimary { get; init; } = "";
    public string? LabelShift { get; init; }
    public string? LabelAltGr { get; init; }

    private bool _isPressed;
    public bool IsPressed 
    { 
        get => _isPressed; 
        set 
        { 
            if (_isPressed != value)
            {
                _isPressed = value; 
                OnPropertyChanged(); 
            }
        } 
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
