using Common.Bootstrap;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.Repositories;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TypeTutor.Logic.Data;
using TypeTutor.Logic.DI;
using TypeTutor.Logic.Tests.Helpers;
using Xunit;

namespace TypeTutor.Logic.Tests.DI;

/// <summary>
/// Tests für das TypeTutorServiceModule.
/// Validiert die korrekte Service-Registrierung und Dependency Injection.
/// </summary>
public sealed class TypeTutorServiceModuleTests : IDisposable
{
    private readonly ServiceProviderFixture _fixture;

    public TypeTutorServiceModuleTests()
    {
        _fixture = new ServiceProviderFixture();
    }

    [Fact]
    public void Register_ShouldRegisterLessonDataEqualityComparer()
    {
        // Act
        var comparer = _fixture.ServiceProvider.GetService<IEqualityComparer<LessonData>>();

        // Assert
        comparer.Should().NotBeNull();
        comparer.Should().BeOfType<LessonDataEqualityComparer>();
    }

    [Fact]
    public void Register_ShouldRegisterLessonGuideDataEqualityComparer()
    {
        // Act
        var comparer = _fixture.ServiceProvider.GetService<IEqualityComparer<LessonGuideData>>();

        // Assert
        comparer.Should().NotBeNull();
        comparer.Should().BeOfType<LessonGuideDataEqualityComparer>();
    }

    [Fact]
    public void Register_ShouldRegisterLessonDataRepository()
    {
        // Act
        var repository = _fixture.ServiceProvider.GetService<IRepositoryBase<LessonData>>();

        // Assert
        repository.Should().NotBeNull();
    }

    [Fact]
    public void Register_ShouldRegisterLessonGuideDataRepository()
    {
        // Act
        var repository = _fixture.ServiceProvider.GetService<IRepositoryBase<LessonGuideData>>();

        // Assert
        repository.Should().NotBeNull();
    }

    [Fact]
    public void Register_ShouldRegisterDataStoreWrapper()
    {
        // Act
        var wrapper = _fixture.ServiceProvider.GetService<DataStoreWrapper>();

        // Assert
        wrapper.Should().NotBeNull();
    }

    [Fact]
    public void Register_ShouldRegisterDataStoreWrapperAsSingleton()
    {
        // Act
        var wrapper1 = _fixture.ServiceProvider.GetService<DataStoreWrapper>();
        var wrapper2 = _fixture.ServiceProvider.GetService<DataStoreWrapper>();

        // Assert
        wrapper1.Should().BeSameAs(wrapper2);
    }

    [Fact]
    public void Register_ShouldRegisterDataStoreProvider()
    {
        // Act
        var provider = _fixture.ServiceProvider.GetService<IDataStoreProvider>();

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void Register_ShouldRegisterRepositoryFactory()
    {
        // Act
        var factory = _fixture.ServiceProvider.GetService<IRepositoryFactory>();

        // Assert
        factory.Should().NotBeNull();
    }

    [Fact]
    public void DataStoreWrapper_ShouldResolveWithDependencies()
    {
        // Act
        var wrapper = _fixture.GetRequiredService<DataStoreWrapper>();

        // Assert
        wrapper.Should().NotBeNull();
        wrapper.Lessons.Should().NotBeNull();
        wrapper.LessonGuides.Should().NotBeNull();
    }

    [Fact]
    public void Register_WithNewServiceCollection_ShouldRegisterAllServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Verwende AddModulesFromAssemblies statt manuelle Instanziierung
        services.AddModulesFromAssemblies(
            typeof(DataToolKit.Abstractions.DI.DataToolKitServiceModule).Assembly,
            typeof(TypeTutorServiceModule).Assembly);
        
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<IEqualityComparer<LessonData>>().Should().NotBeNull();
        provider.GetService<IEqualityComparer<LessonGuideData>>().Should().NotBeNull();
        provider.GetService<DataStoreWrapper>().Should().NotBeNull();
        provider.GetService<IDataStoreProvider>().Should().NotBeNull();
        provider.GetService<IRepositoryFactory>().Should().NotBeNull();
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }
}
