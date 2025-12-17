using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.Repositories;
using SolutionBundler.Core.Models;
using SolutionBundler.Core.Models.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SolutionBundler.Core.Storage;

/// <summary>
/// Wrapper für IDataStore&lt;ProjectInfo&gt; mit vereinfachter API.
/// Verwaltet Projekte persistent via DataToolKit.
/// </summary>
public sealed class ProjectStore
{
    private readonly IDataStore<ProjectInfo> _dataStore;
    private readonly IRepositoryBase<ProjectInfo> _repository;

    /// <summary>
    /// Erstellt eine neue Instanz von ProjectStore.
    /// </summary>
    /// <param name="provider">DataStoreProvider für Zugriff auf DataStore.</param>
    /// <param name="repository">Repository für manuelle Persistierung.</param>
    /// <exception cref="ArgumentNullException">Wenn provider oder repository null ist.</exception>
    public ProjectStore(IDataStoreProvider provider, IRepositoryBase<ProjectInfo> repository)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(repository);
        
        _dataStore = provider.GetDataStore<ProjectInfo>();
        _repository = repository;
    }

    /// <summary>
    /// Schreibgeschützte Auflistung aller gespeicherten Projekte.
    /// </summary>
    public System.Collections.ObjectModel.ReadOnlyObservableCollection<ProjectInfo> Projects 
        => _dataStore.Items;

    /// <summary>
    /// Fügt ein Projekt basierend auf dem Pfad zur .csproj-Datei hinzu.
    /// </summary>
    /// <param name="projectPath">Vollständiger Pfad zur .csproj-Datei.</param>
    /// <returns>True, wenn das Projekt hinzugefügt wurde; False, wenn es bereits existiert.</returns>
    /// <exception cref="ArgumentException">Wenn projectPath leer oder ungültig ist.</exception>
    /// <exception cref="FileNotFoundException">Wenn die .csproj-Datei nicht existiert.</exception>
    public bool AddProject(string projectPath)
    {
        if (string.IsNullOrWhiteSpace(projectPath))
            throw new ArgumentException("Project path cannot be null or empty.", nameof(projectPath));

        if (!File.Exists(projectPath))
            throw new FileNotFoundException($"Project file not found: {projectPath}", projectPath);

        if (!projectPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Project path must end with .csproj", nameof(projectPath));

        var projectInfo = new ProjectInfo { Path = System.IO.Path.GetFullPath(projectPath) };
        return _dataStore.Add(projectInfo);
    }

    /// <summary>
    /// Entfernt ein Projekt basierend auf dem Projektnamen.
    /// </summary>
    /// <param name="projectName">Name des zu entfernenden Projekts (ohne Erweiterung).</param>
    /// <returns>True, wenn das Projekt entfernt wurde; False, wenn es nicht gefunden wurde.</returns>
    public bool RemoveProject(string projectName)
    {
        if (string.IsNullOrWhiteSpace(projectName))
            return false;

        var project = _dataStore.Items.FirstOrDefault(p => 
            string.Equals(p.Name, projectName, StringComparison.OrdinalIgnoreCase));

        return project is not null && _dataStore.Remove(project);
    }

    /// <summary>
    /// Prüft, ob ein Projekt mit dem angegebenen Namen existiert.
    /// </summary>
    /// <param name="projectName">Name des Projekts.</param>
    /// <returns>True, wenn das Projekt existiert.</returns>
    public bool ContainsProject(string projectName)
    {
        if (string.IsNullOrWhiteSpace(projectName))
            return false;

        return _dataStore.Items.Any(p => 
            string.Equals(p.Name, projectName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Entfernt alle Projekte.
    /// </summary>
    public void Clear() => _dataStore.Clear();

    /// <summary>
    /// Setzt die Gruppenzuordnung für ein Projekt.
    /// </summary>
    /// <param name="projectName">Name des Projekts (ohne Erweiterung).</param>
    /// <param name="group">Gruppenname (null oder leer zum Entfernen der Gruppe).</param>
    /// <returns>True, wenn das Projekt gefunden und aktualisiert wurde; False sonst.</returns>
    /// <exception cref="ArgumentException">Wenn projectName leer ist.</exception>
    /// <remarks>
    /// Die Änderung wird automatisch persistiert, wenn der DataStore mit 
    /// <c>trackPropertyChanges: true</c> erstellt wurde.
    /// </remarks>
    public bool SetGroup(string projectName, string? group)
    {
        if (string.IsNullOrWhiteSpace(projectName))
            throw new ArgumentException("Project name cannot be null or empty.", nameof(projectName));

        var project = _dataStore.Items.FirstOrDefault(p => 
            string.Equals(p.Name, projectName, StringComparison.OrdinalIgnoreCase));

        if (project is null)
            return false;

        project.Group = string.IsNullOrWhiteSpace(group) ? null : group;
        return true;
    }

    /// <summary>
    /// Gibt eine sortierte Liste aller verwendeten Gruppennamen zurück.
    /// </summary>
    /// <returns>
    /// Distinct sortierte Liste aller Gruppen (ohne null/leer).
    /// Leere Liste, wenn keine Gruppen verwendet werden.
    /// </returns>
    public IReadOnlyList<string> GetGroups()
    {
        return _dataStore.Items
            .Select(p => p.Group)
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g, StringComparer.OrdinalIgnoreCase)
            .ToList()!;
    }

    /// <summary>
    /// Persistiert alle Änderungen am DataStore in das Repository.
    /// </summary>
    /// <remarks>
    /// Mit trackPropertyChanges=true werden Änderungen automatisch persistiert.
    /// Diese Methode kann für manuelles Speichern verwendet werden, z.B. nach Batch-Updates.
    /// </remarks>
    public void SaveChanges()
    {
        _repository.Write(_dataStore.Items);
    }
}
