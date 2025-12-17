using DataToolKit.Abstractions.DataStores;
using DataToolKit.Storage.DataStores;
using DataToolKit.Storage.Repositories;

namespace TypeTutor.Logic.Data;

/// <summary>
/// Extension-Methoden für <see cref="IDataStoreProvider"/> zur vereinfachten Verwendung
/// der TypeTutor-spezifischen DataStores.
/// </summary>
public static class DataStoreProviderExtensions
{
    /// <summary>
    /// Holt den konfigurierten PersistentDataStore für LessonData als Singleton.
    /// </summary>
    /// <param name="provider">Der IDataStoreProvider.</param>
    /// <param name="repositoryFactory">Die IRepositoryFactory für die Repository-Erstellung.</param>
    /// <returns>Der konfigurierte PersistentDataStore für LessonData.</returns>
    /// <remarks>
    /// Konfiguration:
    /// - Singleton: true
    /// - PropertyChanged-Tracking: false (da LessonData ein immutable record ist)
    /// - AutoLoad: true
    /// </remarks>
    public static PersistentDataStore<LessonData> GetLessonDataStore(
        this IDataStoreProvider provider,
        IRepositoryFactory repositoryFactory)
    {
        return provider.GetPersistent<LessonData>(
            repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: false,
            autoLoad: true);
    }

    /// <summary>
    /// Holt den konfigurierten PersistentDataStore für LessonGuideData als Singleton.
    /// </summary>
    /// <param name="provider">Der IDataStoreProvider.</param>
    /// <param name="repositoryFactory">Die IRepositoryFactory für die Repository-Erstellung.</param>
    /// <returns>Der konfigurierte PersistentDataStore für LessonGuideData.</returns>
    /// <remarks>
    /// Konfiguration:
    /// - Singleton: true
    /// - PropertyChanged-Tracking: false (da LessonGuideData ein immutable record ist)
    /// - AutoLoad: true
    /// </remarks>
    public static PersistentDataStore<LessonGuideData> GetLessonGuideDataStore(
        this IDataStoreProvider provider,
        IRepositoryFactory repositoryFactory)
    {
        return provider.GetPersistent<LessonGuideData>(
            repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: false,
            autoLoad: true);
    }

    /// <summary>
    /// Holt den konfigurierten PersistentDataStore für ModuleData als Singleton.
    /// </summary>
    /// <param name="provider">Der IDataStoreProvider.</param>
    /// <param name="repositoryFactory">Die IRepositoryFactory für die Repository-Erstellung.</param>
    /// <returns>Der konfigurierte PersistentDataStore für ModuleData.</returns>
    /// <remarks>
    /// Konfiguration:
    /// - Singleton: true
    /// - PropertyChanged-Tracking: false (da ModuleData ein immutable record ist)
    /// - AutoLoad: true
    /// </remarks>
    public static PersistentDataStore<ModuleData> GetModuleDataStore(
        this IDataStoreProvider provider,
        IRepositoryFactory repositoryFactory)
    {
        return provider.GetPersistent<ModuleData>(
            repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: false,
            autoLoad: true);
    }
}
