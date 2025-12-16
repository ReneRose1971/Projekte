using DataToolKit.Abstractions.DataStores;
using SolutionBundler.Core.Models;

namespace SolutionBundler.Core.Storage;

/// <summary>
/// Wrapper für IDataStore&lt;ProjectInfo&gt; mit vereinfachter API.
/// Verwaltet Projekte persistent via DataToolKit.
/// </summary>
public sealed class ProjectStore
{
    private readonly IDataStore<ProjectInfo> _dataStore;

    /// <summary>
    /// Erstellt eine neue Instanz von ProjectStore.
    /// </summary>
    /// <param name="provider">DataStoreProvider für Zugriff auf DataStore.</param>
    /// <exception cref="ArgumentNullException">Wenn provider null ist.</exception>
    public ProjectStore(IDataStoreProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _dataStore = provider.GetDataStore<ProjectInfo>();
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
}
