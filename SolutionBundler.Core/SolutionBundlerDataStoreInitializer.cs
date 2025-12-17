using System;
using Common.Bootstrap;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Storage.Repositories;
using Microsoft.Extensions.DependencyInjection;
using SolutionBundler.Core.Models;
using SolutionBundler.Core.Models.Persistence;

namespace SolutionBundler.Core;

/// <summary>
/// Initialisiert die DataStores für SolutionBundler.Core.
/// </summary>
/// <remarks>
/// <para>
/// <b>Initialisierte DataStores:</b>
/// </para>
/// <list type="bullet">
/// <item><see cref="ProjectInfo"/> - Persistent (JSON) mit AutoLoad</item>
/// </list>
/// <para>
/// <b>Ausführungszeitpunkt:</b> Wird automatisch nach <c>BuildServiceProvider()</c> 
/// durch <c>InitializeDataStores()</c> aufgerufen.
/// </para>
/// </remarks>
public sealed class SolutionBundlerDataStoreInitializer : IDataStoreInitializer
{
    /// <summary>
    /// Initialisiert die DataStores für SolutionBundler.Core.
    /// </summary>
    /// <param name="serviceProvider">Der Service Provider mit registrierten Dependencies.</param>
    /// <exception cref="InvalidOperationException">
    /// Wenn erforderliche Services (IDataStoreProvider, IRepositoryFactory) nicht registriert sind.
    /// </exception>
    public void Initialize(IServiceProvider serviceProvider)
    {
        var provider = serviceProvider.GetRequiredService<IDataStoreProvider>();
        var repositoryFactory = serviceProvider.GetRequiredService<IRepositoryFactory>();

        // ProjectInfo DataStore initialisieren
        // - Persistent (JSON-Repository)
        // - AutoLoad aktiviert (lädt bestehende Projekte beim Start)
        // - Singleton (eine Instanz anwendungsweit)
        // - trackPropertyChanges: true (da ProjectInfo jetzt INotifyPropertyChanged via Fody implementiert)
        provider.GetPersistent<ProjectInfo>(
            repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: true,
            autoLoad: true);
    }
}
