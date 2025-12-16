using Microsoft.Extensions.DependencyInjection;
using SolutionBundler.Core;
using SolutionBundler.Core.Abstractions;
using SolutionBundler.Core.Models;
using SolutionBundler.Core.Storage;
using Xunit;

namespace SolutionBundler.Tests;

/// <summary>
/// Integration-Tests für SolutionBundlerCoreModule.
/// Testet die korrekte Registrierung aller Services im DI-Container.
/// </summary>
public class SolutionBundlerCoreModuleTests
{
    private readonly IServiceProvider _serviceProvider;

    public SolutionBundlerCoreModuleTests()
    {
        var services = new ServiceCollection();
        
        // Registriere Module
        new SolutionBundlerCoreModule().Register(services);
        
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void Module_RegistersProjectStore()
    {
        // Act
        var store = _serviceProvider.GetService<ProjectStore>();

        // Assert
        Assert.NotNull(store);
    }

    [Fact]
    public void Module_RegistersProjectInfoComparer()
    {
        // Act
        var comparer = _serviceProvider.GetService<IEqualityComparer<ProjectInfo>>();

        // Assert
        Assert.NotNull(comparer);
        Assert.IsType<ProjectInfoComparer>(comparer);
    }

    [Fact]
    public void Module_RegistersIFileScanner()
    {
        // Act
        var scanner = _serviceProvider.GetService<IFileScanner>();

        // Assert
        Assert.NotNull(scanner);
    }

    [Fact]
    public void Module_RegistersIProjectMetadataReader()
    {
        // Act
        var reader = _serviceProvider.GetService<IProjectMetadataReader>();

        // Assert
        Assert.NotNull(reader);
    }

    [Fact]
    public void Module_RegistersIContentClassifier()
    {
        // Act
        var classifier = _serviceProvider.GetService<IContentClassifier>();

        // Assert
        Assert.NotNull(classifier);
    }

    [Fact]
    public void Module_RegistersIHashCalculator()
    {
        // Act
        var calculator = _serviceProvider.GetService<IHashCalculator>();

        // Assert
        Assert.NotNull(calculator);
    }

    [Fact]
    public void Module_RegistersISecretMasker()
    {
        // Act
        var masker = _serviceProvider.GetService<ISecretMasker>();

        // Assert
        Assert.NotNull(masker);
    }

    [Fact]
    public void Module_RegistersIBundleWriter()
    {
        // Act
        var writer = _serviceProvider.GetService<IBundleWriter>();

        // Assert
        Assert.NotNull(writer);
    }

    [Fact]
    public void Module_RegistersIBundleOrchestrator()
    {
        // Act
        var orchestrator = _serviceProvider.GetService<IBundleOrchestrator>();

        // Assert
        Assert.NotNull(orchestrator);
    }

    [Fact]
    public void Module_RegistersAllServicesAsSingleton()
    {
        // Act
        var store1 = _serviceProvider.GetService<ProjectStore>();
        var store2 = _serviceProvider.GetService<ProjectStore>();

        // Assert
        Assert.Same(store1, store2);
    }

    [Fact]
    public void Module_CanResolveComplexDependencyGraph()
    {
        // Act - IBundleOrchestrator hat mehrere Dependencies
        var orchestrator = _serviceProvider.GetService<IBundleOrchestrator>();

        // Assert
        Assert.NotNull(orchestrator);
    }
}
