using System.ComponentModel;
using PropertyChanged;
using DataToolKit.Abstractions.Repositories;

namespace CustomWPFControls.Services;

/// <summary>
/// Speichert Position und Größe eines WPF-Fensters zur JSON-Persistierung.
/// Verwendet Fody.PropertyChanged für automatische INotifyPropertyChanged-Implementierung.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class WindowLayoutData : IEntity, INotifyPropertyChanged
{
    /// <summary>
    /// Eindeutige ID (für IEntity-Kompatibilität, wird automatisch beim Persistieren gesetzt).
    /// </summary>
    [DoNotNotify]
    public int Id { get; set; }

    /// <summary>
    /// Eindeutiger Schlüssel zur Identifikation des Fensters.
    /// </summary>
    public string WindowKey { get; set; } = string.Empty;

    /// <summary>
    /// Linke Position des Fensters.
    /// </summary>
    public double Left { get; set; }

    /// <summary>
    /// Obere Position des Fensters.
    /// </summary>
    public double Top { get; set; }

    /// <summary>
    /// Breite des Fensters.
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Höhe des Fensters.
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// Fensterstatus (0=Normal, 1=Minimized, 2=Maximized).
    /// </summary>
    public int WindowState { get; set; }

    /// <summary>
    /// PropertyChanged-Event (wird von Fody automatisch aufgerufen).
    /// </summary>
#pragma warning disable CS0067 // Event wird nie verwendet (Fody injiziert den Code)
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
}
