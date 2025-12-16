using System.Windows;
using System.Windows.Input;
using WpfControl = System.Windows.Controls.Control;

namespace SolutionBundler.WPF.Controls;

/// <summary>
/// WPF Custom Control für Log-Ausgabe während des Scan- und Build-Prozesses.
/// Zeigt Logs in einem mehrzeiligen TextBox und einen Fortschritt in einer ProgressBar.
/// </summary>
public class LogOutputView : WpfControl
{
    static LogOutputView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(LogOutputView),
            new FrameworkPropertyMetadata(typeof(LogOutputView)));
    }

    /// <summary>
    /// DependencyProperty für den Titel der Log-Ausgabe.
    /// </summary>
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(LogOutputView),
            new PropertyMetadata("Ausgabe"));

    /// <summary>
    /// Titel der Log-Ausgabe (Standard: "Ausgabe").
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// DependencyProperty für den Log-Text.
    /// </summary>
    public static readonly DependencyProperty LogTextProperty =
        DependencyProperty.Register(
            nameof(LogText),
            typeof(string),
            typeof(LogOutputView),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Aktueller Log-Text (mehrzeilig).
    /// </summary>
    public string LogText
    {
        get => (string)GetValue(LogTextProperty);
        set => SetValue(LogTextProperty, value);
    }

    /// <summary>
    /// DependencyProperty für den Fortschritt (0-100).
    /// </summary>
    public static readonly DependencyProperty ProgressProperty =
        DependencyProperty.Register(
            nameof(Progress),
            typeof(double),
            typeof(LogOutputView),
            new PropertyMetadata(0.0));

    /// <summary>
    /// Aktueller Fortschritt (0-100).
    /// </summary>
    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    /// <summary>
    /// DependencyProperty für den Status-Text.
    /// </summary>
    public static readonly DependencyProperty StatusTextProperty =
        DependencyProperty.Register(
            nameof(StatusText),
            typeof(string),
            typeof(LogOutputView),
            new PropertyMetadata("Bereit"));

    /// <summary>
    /// Status-Text in der Statuszeile.
    /// </summary>
    public string StatusText
    {
        get => (string)GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
    }

    /// <summary>
    /// DependencyProperty für Fortschrittsbalken-Sichtbarkeit.
    /// </summary>
    public static readonly DependencyProperty IsProgressVisibleProperty =
        DependencyProperty.Register(
            nameof(IsProgressVisible),
            typeof(bool),
            typeof(LogOutputView),
            new PropertyMetadata(true));

    /// <summary>
    /// Zeigt oder versteckt den Fortschrittsbalken.
    /// </summary>
    public bool IsProgressVisible
    {
        get => (bool)GetValue(IsProgressVisibleProperty);
        set => SetValue(IsProgressVisibleProperty, value);
    }

    /// <summary>
    /// DependencyProperty für Auto-Scroll zum Ende.
    /// </summary>
    public static readonly DependencyProperty AutoScrollToEndProperty =
        DependencyProperty.Register(
            nameof(AutoScrollToEnd),
            typeof(bool),
            typeof(LogOutputView),
            new PropertyMetadata(true));

    /// <summary>
    /// Aktiviert Auto-Scroll zum Ende bei neuen Log-Einträgen.
    /// </summary>
    public bool AutoScrollToEnd
    {
        get => (bool)GetValue(AutoScrollToEndProperty);
        set => SetValue(AutoScrollToEndProperty, value);
    }

    /// <summary>
    /// DependencyProperty für das Scan-Command.
    /// </summary>
    public static readonly DependencyProperty ScanCommandProperty =
        DependencyProperty.Register(
            nameof(ScanCommand),
            typeof(ICommand),
            typeof(LogOutputView),
            new PropertyMetadata(null));

    /// <summary>
    /// Command zum Starten des Scan-Prozesses.
    /// </summary>
    public ICommand ScanCommand
    {
        get => (ICommand)GetValue(ScanCommandProperty);
        set => SetValue(ScanCommandProperty, value);
    }

    /// <summary>
    /// DependencyProperty für die Sichtbarkeit des Scan-Buttons.
    /// </summary>
    public static readonly DependencyProperty IsScanVisibleProperty =
        DependencyProperty.Register(
            nameof(IsScanVisible),
            typeof(bool),
            typeof(LogOutputView),
            new PropertyMetadata(true));

    /// <summary>
    /// Zeigt oder versteckt den Scan-Button.
    /// </summary>
    public bool IsScanVisible
    {
        get => (bool)GetValue(IsScanVisibleProperty);
        set => SetValue(IsScanVisibleProperty, value);
    }
}
