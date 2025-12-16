using CustomWPFControls.Controls;
using System.Windows;

namespace SolutionBundler.WPF.Controls;

/// <summary>
/// WPF Custom Control für die Darstellung und Bearbeitung von Projekten.
/// Erbt von ListEditorView aus CustomWPFControls.
/// </summary>
public class ProjectListEditorView : ListEditorView
{
    static ProjectListEditorView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ProjectListEditorView),
            new FrameworkPropertyMetadata(typeof(ProjectListEditorView)));
    }

    /// <summary>
    /// DependencyProperty für den Titel der Projekt-Liste.
    /// </summary>
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(ProjectListEditorView),
            new PropertyMetadata("Projekte"));

    /// <summary>
    /// Titel der Projekt-Liste (Standard: "Projekte").
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// DependencyProperty für Platzhalter-Text bei leerer Liste.
    /// </summary>
    public static readonly DependencyProperty EmptyMessageProperty =
        DependencyProperty.Register(
            nameof(EmptyMessage),
            typeof(string),
            typeof(ProjectListEditorView),
            new PropertyMetadata("Keine Projekte vorhanden. Klicken Sie 'Hinzufügen', um ein Projekt hinzuzufügen."));

    /// <summary>
    /// Nachricht, die angezeigt wird, wenn keine Projekte vorhanden sind.
    /// </summary>
    public string EmptyMessage
    {
        get => (string)GetValue(EmptyMessageProperty);
        set => SetValue(EmptyMessageProperty, value);
    }
}
