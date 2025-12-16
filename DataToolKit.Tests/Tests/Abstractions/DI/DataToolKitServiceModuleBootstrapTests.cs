using Common.Bootstrap;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.DI;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.DataStores;
using DataToolKit.Storage.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DataToolKit.Tests.Abstractions.DI;

/// <summary>
/// Tests für den kompletten Bootstrap-Prozess des DataToolKitServiceModule.
/// Simuliert, wie eine App das Modul über <see cref="ServiceCollectionModuleExtensions.AddModulesFromAssemblies"/> registriert.
/// </summary>
public sealed class DataToolKitServiceModuleBootstrapTests
{
    /// <summary>
    /// Testet den kompletten Bootstrap-Prozess wie in einer echten App:
    /// 1. CommonBootstrapServiceModule wird registriert
    /// 2. DataToolKitServiceModule wird über AddModulesFromAssemblies gefunden und registriert
    /// 3. Alle Services können aus dem DI-Container geholt werden
    /// </summary>
    [Fact]
    public void Bootstrap_RegistersAllServices_WhenUsingAddModulesFromAssemblies()
    {
        // Arrange - Simuliert den typischen App-Bootstrap-Prozess
        var services = new ServiceCollection();
        
        // Schritt 1: CommonBootstrap registrieren (Abhängigkeit)
        new CommonBootstrapServiceModule().Register(services);
        
        // Schritt 2: Automatische Modul-Erkennung (wie in Program.cs)
        // Das DataToolKitServiceModule wird automatisch gefunden und registriert
        services.AddModulesFromAssemblies(typeof(DataToolKitServiceModule).Assembly);
        
        // Act
        var provider = services.BuildServiceProvider();
        
        // Assert - Alle drei Services müssen registriert sein
        var repositoryFactory = provider.GetService<IRepositoryFactory>();
        var dataStoreFactory = provider.GetService<IDataStoreFactory>();
        var dataStoreProvider = provider.GetService<IDataStoreProvider>();
        
        Assert.NotNull(repositoryFactory);
        Assert.NotNull(dataStoreFactory);
        Assert.NotNull(dataStoreProvider);
    }

    /// <summary>
    /// Testet, dass IRepositoryFactory aus dem DI-Container aufgelöst werden kann
    /// und die richtige Implementierung zurückgibt.
    /// </summary>
    [Fact]
    public void Bootstrap_CanResolve_IRepositoryFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        new CommonBootstrapServiceModule().Register(services);
        services.AddModulesFromAssemblies(typeof(DataToolKitServiceModule).Assembly);
        
        // Act
        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IRepositoryFactory>();
        
        // Assert
        Assert.NotNull(factory);
        Assert.IsType<RepositoryFactory>(factory);
    }

    /// <summary>
    /// Testet, dass IDataStoreFactory aus dem DI-Container aufgelöst werden kann
    /// und die richtige Implementierung zurückgibt.
    /// </summary>
    [Fact]
    public void Bootstrap_CanResolve_IDataStoreFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        new CommonBootstrapServiceModule().Register(services);
        services.AddModulesFromAssemblies(typeof(DataToolKitServiceModule).Assembly);
        
        // Act
        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IDataStoreFactory>();
        
        // Assert
        Assert.NotNull(factory);
        Assert.IsType<DataStoreFactory>(factory);
    }

    /// <summary>
    /// Testet, dass IDataStoreProvider aus dem DI-Container aufgelöst werden kann
    /// und die richtige Implementierung zurückgibt.
    /// </summary>
    [Fact]
    public void Bootstrap_CanResolve_IDataStoreProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        new CommonBootstrapServiceModule().Register(services);
        services.AddModulesFromAssemblies(typeof(DataToolKitServiceModule).Assembly);
        
        // Act
        var provider = services.BuildServiceProvider();
        var dataStoreProvider = provider.GetRequiredService<IDataStoreProvider>();
        
        // Assert
        Assert.NotNull(dataStoreProvider);
        Assert.IsType<DataStoreProvider>(dataStoreProvider);
    }

    /// <summary>
    /// Testet, dass alle Services als Singleton registriert sind.
    /// Mehrfaches Auflösen sollte die gleiche Instanz zurückgeben.
    /// </summary>
    [Fact]
    public void Bootstrap_AllServicesAreSingletons()
    {
        // Arrange
        var services = new ServiceCollection();
        new CommonBootstrapServiceModule().Register(services);
        services.AddModulesFromAssemblies(typeof(DataToolKitServiceModule).Assembly);
        var provider = services.BuildServiceProvider();
        
        // Act
        var repositoryFactory1 = provider.GetRequiredService<IRepositoryFactory>();
        var repositoryFactory2 = provider.GetRequiredService<IRepositoryFactory>();
        
        var dataStoreFactory1 = provider.GetRequiredService<IDataStoreFactory>();
        var dataStoreFactory2 = provider.GetRequiredService<IDataStoreFactory>();
        
        var dataStoreProvider1 = provider.GetRequiredService<IDataStoreProvider>();
        var dataStoreProvider2 = provider.GetRequiredService<IDataStoreProvider>();
        
        // Assert - Alle sollten die gleiche Instanz sein
        Assert.Same(repositoryFactory1, repositoryFactory2);
        Assert.Same(dataStoreFactory1, dataStoreFactory2);
        Assert.Same(dataStoreProvider1, dataStoreProvider2);
    }

    /// <summary>
    /// Testet die Abhängigkeitskette: DataStoreProvider benötigt DataStoreFactory.
    /// Der Provider sollte korrekt mit der Factory initialisiert werden.
    /// </summary>
    [Fact]
    public void Bootstrap_DataStoreProvider_UsesInjectedFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        new CommonBootstrapServiceModule().Register(services);
        services.AddModulesFromAssemblies(typeof(DataToolKitServiceModule).Assembly);
        var provider = services.BuildServiceProvider();
        
        // Act
        var dataStoreProvider = provider.GetRequiredService<IDataStoreProvider>();
        var dataStoreFactory = provider.GetRequiredService<IDataStoreFactory>();
        
        // Assert - Provider sollte funktionieren (verwendet intern die Factory)
        Assert.NotNull(dataStoreProvider);
        Assert.NotNull(dataStoreFactory);
        
        // Teste, ob der Provider tatsächlich funktioniert
        var inMemoryStore = dataStoreProvider.GetInMemory<TestEntity>(isSingleton: false);
        Assert.NotNull(inMemoryStore);
        Assert.IsType<InMemoryDataStore<TestEntity>>(inMemoryStore);
    }

    /// <summary>
    /// Testet die komplette Integrationskette:
    /// Bootstrap ? IRepositoryFactory ? IDataStoreProvider ? DataStore-Erstellung
    /// </summary>
    [Fact]
    public void Bootstrap_CompleteIntegrationChain_Works()
    {
        // Arrange
        var services = new ServiceCollection();
        new CommonBootstrapServiceModule().Register(services);
        services.AddModulesFromAssemblies(typeof(DataToolKitServiceModule).Assembly);
        var provider = services.BuildServiceProvider();
        
        // Act - Hole alle Services
        var repositoryFactory = provider.GetRequiredService<IRepositoryFactory>();
        var dataStoreFactory = provider.GetRequiredService<IDataStoreFactory>();
        var dataStoreProvider = provider.GetRequiredService<IDataStoreProvider>();
        
        // Assert - Teste die Funktionalität
        Assert.NotNull(repositoryFactory);
        Assert.NotNull(dataStoreFactory);
        Assert.NotNull(dataStoreProvider);
        
        // Teste DataStoreFactory direkt
        var inMemoryStore1 = dataStoreFactory.CreateInMemoryStore<TestEntity>();
        Assert.NotNull(inMemoryStore1);
        
        // Teste DataStoreProvider
        var inMemoryStore2 = dataStoreProvider.GetInMemory<TestEntity>(isSingleton: false);
        Assert.NotNull(inMemoryStore2);
    }

    /// <summary>
    /// Testet, dass mehrfache Registrierung des Moduls nicht zu Duplikaten führt
    /// (TryAddSingleton sorgt für Idempotenz).
    /// </summary>
    [Fact]
    public void Bootstrap_MultipleRegistrations_AreIdempotent()
    {
        // Arrange
        var services = new ServiceCollection();
        new CommonBootstrapServiceModule().Register(services);
        
        // Act - Module mehrfach registrieren (könnte in komplexen Apps passieren)
        new DataToolKitServiceModule().Register(services);
        new DataToolKitServiceModule().Register(services);
        new DataToolKitServiceModule().Register(services);
        
        var provider = services.BuildServiceProvider();
        
        // Assert - Sollte trotzdem nur Singleton-Instanzen geben
        var factory1 = provider.GetRequiredService<IRepositoryFactory>();
        var factory2 = provider.GetRequiredService<IRepositoryFactory>();
        
        Assert.Same(factory1, factory2);
    }

    // Test-Entität für die Tests
    private sealed class TestEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
