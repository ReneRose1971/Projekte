using DataToolKit.Abstractions.DataStores;
using DataToolKit.Storage.DataStores;
using DataToolKit.Storage.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace TypeTutor.Logic.Data;

/// <summary>
/// Wrapper-Klasse für den Zugriff auf die persistenten DataStores für LessonData, LessonGuideData und ModuleData.
/// Stellt die Items-Collections der DataStores als ReadOnlyObservableCollection zur Verfügung.
/// </summary>
public sealed class DataStoreWrapper
{
    private readonly PersistentDataStore<LessonData> _lessonDataStore;
    private readonly PersistentDataStore<LessonGuideData> _lessonGuideDataStore;
    private readonly PersistentDataStore<ModuleData> _moduleDataStore;

    /// <summary>
    /// Erstellt eine neue Instanz des DataStoreWrapper.
    /// </summary>
    /// <param name="provider">Der IDataStoreProvider zur Auflösung der DataStores.</param>
    /// <param name="repositoryFactory">Die IRepositoryFactory für die Repository-Erstellung.</param>
    /// <exception cref="ArgumentNullException">Wenn provider oder repositoryFactory null ist.</exception>
    public DataStoreWrapper(IDataStoreProvider provider, IRepositoryFactory repositoryFactory)
    {
        if (provider is null)
            throw new ArgumentNullException(nameof(provider));
        if (repositoryFactory is null)
            throw new ArgumentNullException(nameof(repositoryFactory));

        // Hole PersistentDataStores über Extension-Methoden
        _lessonDataStore = provider.GetLessonDataStore(repositoryFactory);
        _lessonGuideDataStore = provider.GetLessonGuideDataStore(repositoryFactory);
        _moduleDataStore = provider.GetModuleDataStore(repositoryFactory);
    }

    /// <summary>
    /// Liefert die Collection aller geladenen Lessons als ReadOnlyObservableCollection.
    /// Die Collection wird automatisch aktualisiert, wenn sich der DataStore ändert.
    /// </summary>
    public ReadOnlyObservableCollection<LessonData> Lessons => _lessonDataStore.Items;

    /// <summary>
    /// Liefert die Collection aller geladenen LessonGuides als ReadOnlyObservableCollection.
    /// Die Collection wird automatisch aktualisiert, wenn sich der DataStore ändert.
    /// </summary>
    public ReadOnlyObservableCollection<LessonGuideData> LessonGuides => _lessonGuideDataStore.Items;

    /// <summary>
    /// Liefert die Collection aller geladenen Module als ReadOnlyObservableCollection.
    /// Die Collection wird automatisch aktualisiert, wenn sich der DataStore ändert.
    /// </summary>
    public ReadOnlyObservableCollection<ModuleData> Modules => _moduleDataStore.Items;

    /// <summary>
    /// Lädt die Daten aus den JSON-Dateien neu.
    /// </summary>
    public void Reload()
    {
        _lessonDataStore.Load();
        _lessonGuideDataStore.Load();
        _moduleDataStore.Load();
    }

    /// <summary>
    /// Sucht einen LessonGuide anhand der LessonId.
    /// </summary>
    /// <param name="lessonId">Die LessonId des LessonGuides.</param>
    /// <returns>Der gefundene LessonGuide oder null, wenn nicht gefunden.</returns>
    public LessonGuideData? GetLessonGuide(string lessonId)
    {
        return _lessonGuideDataStore.Items.FirstOrDefault(g => 
            string.Equals(g.LessonId, lessonId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Sucht ein Modul anhand der ModuleId.
    /// </summary>
    /// <param name="moduleId">Die ModuleId des Moduls.</param>
    /// <returns>Das gefundene Modul oder null, wenn nicht gefunden.</returns>
    public ModuleData? GetModule(string moduleId)
    {
        return _moduleDataStore.Items.FirstOrDefault(m => 
            string.Equals(m.ModuleId, moduleId, StringComparison.OrdinalIgnoreCase));
    }
}
