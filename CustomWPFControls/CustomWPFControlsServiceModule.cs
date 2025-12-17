using Common.Bootstrap;
using DataToolKit.Abstractions.DI;
using Microsoft.Extensions.DependencyInjection;
using CustomWPFControls.Services;

namespace CustomWPFControls;

/// <summary>
/// Service-Modul für CustomWPFControls.
/// Registriert Repositories und Services.
/// </summary>
/// <remarks>
/// <para>
/// <b>DataStore-Initialisierung:</b> DataStores werden NICHT hier initialisiert,
/// sondern durch <see cref="CustomWPFControlsDataStoreInitializer"/> nach dem Build
/// des Containers. Siehe <see cref="IDataStoreInitializer"/> für Details.
/// </para>
/// </remarks>
public sealed class CustomWPFControlsServiceModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // DataToolKit-Module registrieren (für IDataStoreProvider, falls noch nicht geschehen)
        // Dies ist idempotent, falls bereits registriert
        new DataToolKitServiceModule().Register(services);

        // JSON Repository für WindowLayoutData registrieren
        services.AddJsonRepository<WindowLayoutData>(
            appSubFolder: "CustomWPFControls",
            fileNameBase: "windowlayouts");

        // WindowLayoutService als Singleton
        // WICHTIG: Der DataStore für WindowLayoutData wird durch CustomWPFControlsDataStoreInitializer
        // nach BuildServiceProvider() erstellt
        services.AddSingleton<WindowLayoutService>();
    }
}
