using System;
using System.Linq;
using System.Reflection;

namespace Common.Bootstrap;

/// <summary>
/// Erweiterungen für die automatische Initialisierung von DataStores.
/// </summary>
public static class DataStoreInitializerExtensions
{
    /// <summary>
    /// Sucht in den angegebenen (oder allen geladenen) Assemblies nach Klassen,
    /// die <see cref="IDataStoreInitializer"/> implementieren, erzeugt Instanzen
    /// und ruft <see cref="IDataStoreInitializer.Initialize"/> auf.
    /// </summary>
    /// <param name="serviceProvider">Der Service Provider, aus dem Dependencies aufgelöst werden.</param>
    /// <param name="assemblies">
    /// Liste der zu scannenden Assemblies. Wenn leer oder null, werden alle
    /// aktuell geladenen Assemblies (<see cref="System.AppDomain.GetAssemblies"/>) gescannt.
    /// </param>
    /// <returns>
    /// Der gleiche <see cref="IServiceProvider"/> für Fluent-API-Verkettung.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Wenn <paramref name="serviceProvider"/> null ist.
    /// </exception>
    /// <exception cref="MissingMethodException">
    /// Wenn ein gefundener Initialisierer keinen öffentlichen parameterlosen Konstruktor hat.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Wenn während der Initialisierung ein Fehler auftritt. Die Exception enthält
    /// den Namen des fehlgeschlagenen Initialisierers und die ursprüngliche Exception als InnerException.
    /// </exception>
    /// <remarks>
    /// <para>
    /// <b>?? WICHTIG - Ausführungszeitpunkt:</b>
    /// Diese Methode muss <b>NACH</b> <c>BuildServiceProvider()</c> aufgerufen werden,
    /// niemals während der Service-Registrierung in <see cref="IServiceModule.Register"/>!
    /// </para>
    /// <para>
    /// <b>Gefunden werden:</b>
    /// </para>
    /// <list type="bullet">
    /// <item>Konkrete, nicht-abstrakte Klassen, die <see cref="IDataStoreInitializer"/> implementieren</item>
    /// <item>Öffentliche und interne Klassen</item>
    /// <item>Nested Classes (auch private, wenn zugänglich)</item>
    /// </list>
    /// <para>
    /// <b>NICHT gefunden werden:</b>
    /// </para>
    /// <list type="bullet">
    /// <item>Abstrakte Klassen und Interfaces</item>
    /// <item>Generische Klassen mit ungebundenen Typparametern</item>
    /// <item>Klassen ohne öffentlichen parameterlosen Konstruktor</item>
    /// </list>
    /// <para>
    /// <b>Ausführungsreihenfolge:</b> Die Initialisierer werden in der Reihenfolge ausgeführt,
    /// in der sie gefunden werden. Wenn eine bestimmte Reihenfolge erforderlich ist, sollten
    /// die Assemblies in der gewünschten Reihenfolge übergeben werden.
    /// </para>
    /// <para>
    /// <b>Fehlerbehandlung:</b> 
    /// </para>
    /// <list type="bullet">
    /// <item><see cref="ReflectionTypeLoadException"/> wird automatisch behandelt - 
    /// nur erfolgreich geladene Typen werden verarbeitet</item>
    /// <item>Bei Fehlern während der Initialisierung wird eine aussagekräftige 
    /// <see cref="InvalidOperationException"/> mit dem Namen des fehlgeschlagenen 
    /// Initialisierers geworfen</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <b>? KORREKT:</b> Verwendung in Program.cs mit Microsoft.Extensions.Hosting
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
    /// <b>? KORREKT:</b> Verwendung mit plain ServiceCollection
    /// <code>
    /// var services = new ServiceCollection();
    /// 
    /// // Phase 1: Service-Registrierung
    /// services.AddModulesFromAssemblies(
    ///     typeof(Program).Assembly,
    ///     typeof(DataToolKitServiceModule).Assembly);
    /// 
    /// // Phase 2: Container bauen
    /// var provider = services.BuildServiceProvider();
    /// 
    /// // Phase 3: DataStores initialisieren
    /// provider.InitializeDataStores(
    ///     typeof(Program).Assembly,
    ///     typeof(MyLibraryDataStoreInitializer).Assembly);
    /// 
    /// // Phase 4: Services verwenden
    /// var myService = provider.GetRequiredService&lt;IMyService&gt;();
    /// </code>
    /// 
    /// <b>? KORREKT:</b> Alle Assemblies scannen
    /// <code>
    /// var provider = services.BuildServiceProvider();
    /// 
    /// // Scannt alle aktuell geladenen Assemblies
    /// provider.InitializeDataStores();
    /// </code>
    /// 
    /// <b>? FALSCH:</b> Während Service-Registrierung aufrufen
    /// <code>
    /// public class MyModule : IServiceModule
    /// {
    ///     public void Register(IServiceCollection services)
    ///     {
    ///         services.AddSingleton&lt;IDataStoreProvider, DataStoreProvider&gt;();
    ///         
    ///         // ? FALSCH: Container ist noch nicht fertig!
    ///         var provider = services.BuildServiceProvider();
    ///         provider.InitializeDataStores(); // Zu früh!
    ///     }
    /// }
    /// </code>
    /// </example>
    public static IServiceProvider InitializeDataStores(
        this IServiceProvider serviceProvider,
        params Assembly[] assemblies)
    {
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));

        if (assemblies == null || assemblies.Length == 0)
            assemblies = AppDomain.CurrentDomain.GetAssemblies();

        var initializerType = typeof(IDataStoreInitializer);

        var initializers = assemblies
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null)!; }
            })
            .Where(t => initializerType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
            .Select(t => (IDataStoreInitializer)Activator.CreateInstance(t)!)
            .ToList();

        foreach (var initializer in initializers)
        {
            try
            {
                initializer.Initialize(serviceProvider);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Fehler beim Initialisieren von DataStores durch '{initializer.GetType().Name}': {ex.Message}",
                    ex);
            }
        }

        return serviceProvider;
    }
}
