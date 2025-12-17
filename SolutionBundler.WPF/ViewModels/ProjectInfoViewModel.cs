using CustomWPFControls.ViewModels;
using PropertyChanged;
using SolutionBundler.Core.Models.Persistence;
using System.IO;

namespace SolutionBundler.WPF.ViewModels;

/// <summary>
/// ViewModel-Wrapper für ProjectInfo.
/// Ermöglicht MVVM-konforme Darstellung und Bearbeitung von Projekten.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class ProjectInfoViewModel : ViewModelBase<ProjectInfo>
{
    /// <summary>
    /// Erstellt ein neues ViewModel für ein ProjectInfo-Objekt.
    /// </summary>
    /// <param name="model">Das zu wrappende ProjectInfo-Model.</param>
    public ProjectInfoViewModel(ProjectInfo model) : base(model)
    {
    }

    /// <summary>
    /// Projektname (read-only, berechnet aus Path).
    /// </summary>
    public string Name => Model.Name;

    /// <summary>
    /// Vollständiger Pfad zur .csproj-Datei.
    /// </summary>
    public string Path => Model.Path;

    /// <summary>
    /// Optionale Gruppenzuordnung für das Projekt.
    /// Änderungen werden direkt am Model vorgenommen und müssen manuell persistiert werden.
    /// </summary>
    public string? Group
    {
        get => Model.Group;
        set => Model.Group = value;
    }

    /// <summary>
    /// Prüft, ob die .csproj-Datei noch existiert.
    /// </summary>
    public bool FileExists => File.Exists(Model.Path);

    /// <summary>
    /// Status-Text für Tooltip und Anzeige.
    /// </summary>
    public string StatusText => FileExists 
        ? $"Projekt existiert: {Path}" 
        : $"Datei nicht gefunden: {Path}";

    /// <summary>
    /// ToString für Debugging und Anzeige.
    /// </summary>
    public override string ToString() => $"{Name} ({Path})";
}
