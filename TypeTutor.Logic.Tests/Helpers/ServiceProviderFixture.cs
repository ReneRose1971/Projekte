using Common.Bootstrap;
using Microsoft.Extensions.DependencyInjection;
using TypeTutor.Logic.DI;
using DataToolKit.Abstractions.DI;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Storage.Repositories;
using TypeTutor.Logic.Data;

namespace TypeTutor.Logic.Tests.Helpers;

/// <summary>
/// Fixture für den DI-Container, um Services für Tests bereitzustellen.
/// Registriert alle TypeTutor.Logic-Services via AddModulesFromAssemblies.
/// </summary>
public sealed class ServiceProviderFixture : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public ServiceProviderFixture()
    {
        var services = new ServiceCollection();
        
        // Registriere alle Module aus den relevanten Assemblies
        // Die Reihenfolge ist wichtig: DataToolKit zuerst, dann TypeTutor
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,  // DataToolKit-Infrastruktur
            typeof(TypeTutorServiceModule).Assembly);   // TypeTutor-Services
        
        _serviceProvider = services.BuildServiceProvider();
        
        // Leere die DataStores für eine saubere Test-Umgebung
        ClearDataStores();
    }

    /// <summary>
    /// Leert alle DataStores, um eine saubere Test-Umgebung zu schaffen.
    /// </summary>
    private void ClearDataStores()
    {
        try
        {
            var provider = _serviceProvider.GetRequiredService<IDataStoreProvider>();
            var factory = _serviceProvider.GetRequiredService<IRepositoryFactory>();
            
            var lessonStore = provider.GetLessonDataStore(factory);
            var guideStore = provider.GetLessonGuideDataStore(factory);
            
            lessonStore.Clear();
            guideStore.Clear();
        }
        catch
        {
            // Ignoriere Fehler beim Leeren (z.B. wenn Stores noch nicht existieren)
        }
    }

    /// <summary>
    /// Gibt den konfigurierten ServiceProvider zurück.
    /// </summary>
    public IServiceProvider ServiceProvider => _serviceProvider;

    /// <summary>
    /// Löst einen Service aus dem DI-Container auf.
    /// </summary>
    public T GetRequiredService<T>() where T : notnull
        => _serviceProvider.GetRequiredService<T>();

    /// <summary>
    /// Erstellt einen neuen Scope für Tests, die scoped Services benötigen.
    /// </summary>
    public IServiceScope CreateScope()
        => _serviceProvider.CreateScope();

    public void Dispose()
    {
        // Leere die DataStores beim Dispose
        ClearDataStores();
        _serviceProvider.Dispose();
    }
}
