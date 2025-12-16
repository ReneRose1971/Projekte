using DataToolKit.Abstractions.DataStores;
using DataToolKit.Storage.DataStores;
using DataToolKit.Storage.Repositories;
using System;
using System.Collections.ObjectModel;

namespace TypeTutor.Logic.Data;

/// <summary>
/// Wrapper-Klasse für den Zugriff auf die persistenten DataStores für LessonData und LessonGuideData.
/// Stellt die Items-Collections der DataStores als ReadOnlyObservableCollection zur Verfügung.
/// </summary>
public sealed class DataStoreWrapper
{
    private readonly PersistentDataStore<LessonData> _lessonDataStore;
    private readonly PersistentDataStore<LessonGuideData> _lessonGuideDataStore;

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

        // Hole PersistentDataStores für beide Datentypen
        // Diese wurden vom TypeTutorServiceModule registriert
        _lessonDataStore = provider.GetPersistent<LessonData>(
            repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: false,
            autoLoad: true);

        _lessonGuideDataStore = provider.GetPersistent<LessonGuideData>(
            repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: false,
            autoLoad: true);
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
    /// Direkter Zugriff auf den LessonData DataStore für erweiterte Operationen.
    /// </summary>
    public PersistentDataStore<LessonData> LessonDataStore => _lessonDataStore;

    /// <summary>
    /// Direkter Zugriff auf den LessonGuideData DataStore für erweiterte Operationen.
    /// </summary>
    public PersistentDataStore<LessonGuideData> LessonGuideDataStore => _lessonGuideDataStore;

    /// <summary>
    /// Lädt die Daten aus den JSON-Dateien neu.
    /// </summary>
    public void Reload()
    {
        _lessonDataStore.Load();
        _lessonGuideDataStore.Load();
    }
}
