using System;
using Common.Bootstrap;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Storage.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Scriptum.Content.Data;
using Scriptum.Progress;

namespace Scriptum.Persistence;

/// <summary>
/// Initialisiert die DataStores für <see cref="TrainingSession"/>, 
/// <see cref="ModuleData"/>, <see cref="LessonData"/> und <see cref="LessonGuideData"/>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Ausführungszeitpunkt:</b> Diese Klasse wird nach dem Aufbau des DI-Containers
/// und vor dem Start der Anwendung ausgeführt.
/// </para>
/// <para>
/// <b>Idempotenz:</b> Mehrfache Aufrufe von <see cref="Initialize"/> sind sicher,
/// da <see cref="IDataStoreProvider.GetPersistent{T}"/> bei <c>isSingleton: true</c>
/// immer die gleiche Instanz zurückgibt.
/// </para>
/// </remarks>
public sealed class ScriptumDataStoreInitializer : IDataStoreInitializer
{
    /// <summary>
    /// Initialisiert die PersistentDataStores für Progress und Content.
    /// </summary>
    /// <param name="serviceProvider">Der Service-Provider.</param>
    /// <exception cref="InvalidOperationException">
    /// Wenn <see cref="IDataStoreProvider"/> oder <see cref="IRepositoryFactory"/> nicht registriert sind.
    /// </exception>
    public void Initialize(IServiceProvider serviceProvider)
    {
        var provider = serviceProvider.GetRequiredService<IDataStoreProvider>();
        var repositoryFactory = serviceProvider.GetRequiredService<IRepositoryFactory>();

        InitializeProgressDataStores(provider, repositoryFactory);
        InitializeContentDataStores(provider, repositoryFactory);
    }

    private static void InitializeProgressDataStores(
        IDataStoreProvider provider,
        IRepositoryFactory repositoryFactory)
    {
        provider.GetPersistent<TrainingSession>(
            repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: true,
            autoLoad: true);
    }

    private static void InitializeContentDataStores(
        IDataStoreProvider provider,
        IRepositoryFactory repositoryFactory)
    {
        provider.GetPersistent<ModuleData>(
            repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: true,
            autoLoad: true);

        provider.GetPersistent<LessonData>(
            repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: true,
            autoLoad: true);

        provider.GetPersistent<LessonGuideData>(
            repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: true,
            autoLoad: true);
    }
}
