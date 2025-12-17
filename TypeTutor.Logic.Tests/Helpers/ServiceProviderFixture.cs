using Common.Bootstrap;
using Microsoft.Extensions.DependencyInjection;
using TypeTutor.Logic.DI;
using DataToolKit.Abstractions.DI;

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
        _serviceProvider.Dispose();
    }
}
