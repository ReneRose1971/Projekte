using System;

namespace Common.Bootstrap;

/// <summary>
/// Schnittstelle für DataStore-Initialisierer.
/// Implementierende Klassen werden automatisch beim Startup gefunden und ausgeführt.
/// </summary>
/// <remarks>
/// <para>
/// <b>?? WICHTIG - Ausführungszeitpunkt:</b>
/// Diese Methode darf <b>NICHT</b> während <see cref="IServiceModule.Register"/> 
/// aufgerufen werden! Die Initialisierung muss <b>nach</b> dem vollständigen Aufbau 
/// des DI-Containers erfolgen.
/// </para>
/// <para>
/// <b>Korrekte Ausführungsreihenfolge:</b>
/// </para>
/// <list type="number">
/// <item><b>Phase 1:</b> <c>services.AddModulesFromAssemblies()</c> - Registrierung aller Services</item>
/// <item><b>Phase 2:</b> <c>var provider = services.BuildServiceProvider()</c> - Container bauen</item>
/// <item><b>Phase 3:</b> <c>provider.InitializeDataStores()</c> - DataStores initialisieren ?</item>
/// <item><b>Phase 4:</b> <c>await app.RunAsync()</c> - Anwendung starten</item>
/// </list>
/// <para>
/// <b>Zweck:</b> Zentralisiert die Initialisierung aller DataStores einer Bibliothek.
/// Jede Bibliothek kann einen oder mehrere Initialisierer bereitstellen, die ihre
/// benötigten DataStores beim Application-Startup registrieren.
/// </para>
/// </remarks>
/// <example>
/// <b>? KORREKT:</b> Verwendung in Program.cs
/// <code>
/// var builder = Host.CreateApplicationBuilder(args);
/// 
/// // Phase 1: Service-Registrierung
/// builder.Services.AddModulesFromAssemblies(
///     typeof(Program).Assembly,
///     typeof(DataToolKitServiceModule).Assembly);
/// 
/// // Phase 2: Container bauen
/// var app = builder.Build();
/// 
/// // Phase 3: DataStores initialisieren (NACH Build!)
/// app.Services.InitializeDataStores(
///     typeof(Program).Assembly,
///     typeof(MyLibraryDataStoreInitializer).Assembly);
/// 
/// // Phase 4: App starten
/// await app.RunAsync();
/// </code>
/// 
/// <b>? KORREKT:</b> Implementierung eines Initialisierers
/// <code>
/// public sealed class MyLibraryDataStoreInitializer : IDataStoreInitializer
/// {
///     public void Initialize(IServiceProvider serviceProvider)
///     {
///         // Container ist fertig - Dependencies sind verfügbar
///         var provider = serviceProvider.GetRequiredService&lt;IDataStoreProvider&gt;();
///         var factory = serviceProvider.GetRequiredService&lt;IRepositoryFactory&gt;();
///         
///         // Initialisiere alle DataStores für diese Bibliothek
///         provider.GetPersistent&lt;Customer&gt;(factory, isSingleton: true, autoLoad: true);
///         provider.GetPersistent&lt;Order&gt;(factory, isSingleton: true, autoLoad: true);
///         provider.GetInMemory&lt;AppSettings&gt;(isSingleton: true);
///     }
/// }
/// </code>
/// 
/// <b>? FALSCH:</b> Initialisierung in IServiceModule
/// <code>
/// public class MyModule : IServiceModule
/// {
///     public void Register(IServiceCollection services)
///     {
///         services.AddSingleton&lt;IDataStoreProvider, DataStoreProvider&gt;();
///         
///         // ? FALSCH: Container ist noch nicht fertig!
///         var provider = services.BuildServiceProvider(); // Erstellt inkompletten Provider
///         provider.InitializeDataStores(); // Zu früh - Dependencies fehlen!
///     }
/// }
/// </code>
/// </example>
public interface IDataStoreInitializer
{
    /// <summary>
    /// Initialisiert die DataStores für diese Bibliothek/dieses Modul.
    /// </summary>
    /// <param name="serviceProvider">
    /// Der Service Provider, aus dem Dependencies aufgelöst werden können.
    /// </param>
    /// <remarks>
    /// <para>
    /// Diese Methode wird beim Application-Startup aufgerufen, <b>nachdem</b> alle
    /// Services registriert und der Container gebaut wurde, aber <b>bevor</b> die Anwendung startet.
    /// </para>
    /// <para>
    /// <b>Best Practices:</b>
    /// </para>
    /// <list type="bullet">
    /// <item>Halten Sie diese Methode schlank und fokussiert</item>
    /// <item>Nur DataStores initialisieren, keine komplexe Business-Logik</item>
    /// <item>Verwenden Sie <c>GetRequiredService&lt;T&gt;()</c> um sicherzustellen, dass Dependencies registriert sind</item>
    /// <item>Fehlerbehandlung beachten - Exceptions werden als <see cref="InvalidOperationException"/> geworfen</item>
    /// </list>
    /// </remarks>
    void Initialize(IServiceProvider serviceProvider);
}
