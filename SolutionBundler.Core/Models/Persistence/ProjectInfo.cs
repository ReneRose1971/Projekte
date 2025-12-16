using DataToolKit.Abstractions.Repositories;

namespace SolutionBundler.Core.Models;

/// <summary>
/// Repräsentiert ein C#-Projekt mit Pfad und Name.
/// Wird via JSON serialisiert und persistiert.
/// </summary>
public sealed class ProjectInfo 
{
    /// <summary>
    /// Vollständiger Pfad zur .csproj-Datei.
    /// </summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// Projektname, extrahiert aus dem Pfad (ohne Erweiterung).
    /// Wird als Identitätskriterium verwendet.
    /// </summary>
    public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

    /// <summary>
    /// Überschreibt Equals basierend auf dem Name-Property.
    /// </summary>
    public override bool Equals(object? obj)
        => obj is ProjectInfo other && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Überschreibt GetHashCode basierend auf dem Name-Property.
    /// </summary>
    public override int GetHashCode()
        => StringComparer.OrdinalIgnoreCase.GetHashCode(Name);

    /// <summary>
    /// ToString gibt den Projektnamen zurück.
    /// </summary>
    public override string ToString() => Name;
}
