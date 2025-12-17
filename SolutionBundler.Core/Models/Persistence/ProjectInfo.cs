using DataToolKit.Abstractions.Repositories;
using PropertyChanged;
using System;

namespace SolutionBundler.Core.Models.Persistence;

/// <summary>
/// Repräsentiert ein C#-Projekt mit Pfad, Name und optionaler Gruppenzugehörigkeit.
/// Wird via JSON serialisiert und persistiert.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class ProjectInfo 
{
    /// <summary>
    /// Vollständiger Pfad zur .csproj-Datei.
    /// </summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// Optionale Gruppenzuordnung für das Projekt (z.B. "Apps", "Libraries").
    /// Standardmäßig leer. Wird nicht für Gleichheitsvergleiche verwendet.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// Projektname, extrahiert aus dem Pfad (ohne Erweiterung).
    /// Wird als Identitätskriterium verwendet.
    /// </summary>
    [DoNotNotify]
    public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

    /// <summary>
    /// Überschreibt Equals basierend auf dem Name-Property.
    /// Group wird NICHT für Gleichheit berücksichtigt.
    /// </summary>
    public override bool Equals(object? obj)
        => obj is ProjectInfo other && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Überschreibt GetHashCode basierend auf dem Name-Property.
    /// Group wird NICHT für GetHashCode berücksichtigt.
    /// </summary>
    public override int GetHashCode()
        => StringComparer.OrdinalIgnoreCase.GetHashCode(Name);

    /// <summary>
    /// ToString gibt den Projektnamen zurück.
    /// </summary>
    public override string ToString() => Name;
}
