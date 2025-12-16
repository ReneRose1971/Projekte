using Microsoft.Extensions.DependencyInjection;
using TypeTutor.Logic.DI;

namespace TypeTutor.Logic.Tests.Helpers;

/// <summary>
/// Fixture für den DI-Container, um Services für Tests bereitzustellen.
/// Registriert alle TypeTutor.Logic-Services via TypeTutorServiceModule.
/// </summary>
public sealed class ServiceProviderFixture : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public ServiceProviderFixture()
    {
        var services = new ServiceCollection();
        
        // Registriere das TypeTutor Service-Modul
        new TypeTutorServiceModule().Register(services);
        
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
