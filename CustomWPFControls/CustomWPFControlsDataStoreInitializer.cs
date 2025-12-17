using System;
using Common.Bootstrap;
using CustomWPFControls.Services;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Storage.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CustomWPFControls;

/// <summary>
/// Initialisiert die DataStores für CustomWPFControls.
/// </summary>
/// <remarks>
/// <para>
/// <b>Initialisierte DataStores:</b>
/// </para>
/// <list type="bullet">
/// <item><see cref="WindowLayoutData"/> - Persistent (JSON) mit AutoLoad und PropertyChanged-Tracking</item>
/// </list>
/// <para>
/// <b>Verwendung:</b> Dieser Initializer wird vom <see cref="WindowLayoutService"/> 
/// benötigt, der Fenster-Positionen und -Größen persistiert.
/// </para>
/// </remarks>
public sealed class CustomWPFControlsDataStoreInitializer : IDataStoreInitializer
{
    /// <summary>
    /// Initialisiert die DataStores für CustomWPFControls.
    /// </summary>
    /// <param name="serviceProvider">Der Service Provider mit registrierten Dependencies.</param>
    /// <exception cref="InvalidOperationException">
    /// Wenn erforderliche Services (IDataStoreProvider, IRepositoryFactory) nicht registriert sind.
    /// </exception>
    public void Initialize(IServiceProvider serviceProvider)
    {
        var provider = serviceProvider.GetRequiredService<IDataStoreProvider>();
        var repositoryFactory = serviceProvider.GetRequiredService<IRepositoryFactory>();

        // WindowLayoutData DataStore initialisieren
        // - Persistent (JSON-Repository)
        // - AutoLoad aktiviert (lädt gespeicherte Window-Layouts beim Start)
        // - PropertyChanged-Tracking aktiviert (automatisches Speichern bei Änderungen)
        // - Singleton (eine Instanz anwendungsweit)
        provider.GetPersistent<WindowLayoutData>(
            repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: true,
            autoLoad: true);
    }
}
