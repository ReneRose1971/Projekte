using Common.Bootstrap;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.DI;
using DataToolKit.Storage.DataStores;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Scriptum.Content.Data;
using Scriptum.Persistence;
using Xunit;

namespace Scriptum.Content.Tests.DI;

[Collection("LiteDB Tests")]
public sealed class ScriptumDataStoreInitializerContentTests : IDisposable
{
    private ServiceProvider? _serviceProvider;

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    [Fact]
    public void Initialize_Should_Create_PersistentDataStore_ForModuleData()
    {
        var services = new ServiceCollection();
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,
            typeof(ScriptumPersistenceServiceModule).Assembly);
        _serviceProvider = services.BuildServiceProvider();

        var initializer = new ScriptumDataStoreInitializer();
        initializer.Initialize(_serviceProvider);

        var dataStoreProvider = _serviceProvider.GetRequiredService<IDataStoreProvider>();
        var dataStore = dataStoreProvider.GetDataStore<ModuleData>();

        dataStore.Should().NotBeNull();
        dataStore.Should().BeOfType<PersistentDataStore<ModuleData>>();
    }

    [Fact]
    public void Initialize_Should_Create_PersistentDataStore_ForLessonData()
    {
        var services = new ServiceCollection();
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,
            typeof(ScriptumPersistenceServiceModule).Assembly);
        _serviceProvider = services.BuildServiceProvider();

        var initializer = new ScriptumDataStoreInitializer();
        initializer.Initialize(_serviceProvider);

        var dataStoreProvider = _serviceProvider.GetRequiredService<IDataStoreProvider>();
        var dataStore = dataStoreProvider.GetDataStore<LessonData>();

        dataStore.Should().NotBeNull();
        dataStore.Should().BeOfType<PersistentDataStore<LessonData>>();
    }

    [Fact]
    public void Initialize_Should_Create_PersistentDataStore_ForLessonGuideData()
    {
        var services = new ServiceCollection();
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,
            typeof(ScriptumPersistenceServiceModule).Assembly);
        _serviceProvider = services.BuildServiceProvider();

        var initializer = new ScriptumDataStoreInitializer();
        initializer.Initialize(_serviceProvider);

        var dataStoreProvider = _serviceProvider.GetRequiredService<IDataStoreProvider>();
        var dataStore = dataStoreProvider.GetDataStore<LessonGuideData>();

        dataStore.Should().NotBeNull();
        dataStore.Should().BeOfType<PersistentDataStore<LessonGuideData>>();
    }

    [Fact]
    public void Initialize_Should_Be_Idempotent_ForModuleData()
    {
        var services = new ServiceCollection();
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,
            typeof(ScriptumPersistenceServiceModule).Assembly);
        _serviceProvider = services.BuildServiceProvider();

        var initializer = new ScriptumDataStoreInitializer();

        initializer.Initialize(_serviceProvider);
        var dataStoreProvider = _serviceProvider.GetRequiredService<IDataStoreProvider>();
        var firstStore = dataStoreProvider.GetDataStore<ModuleData>();

        initializer.Initialize(_serviceProvider);
        var secondStore = dataStoreProvider.GetDataStore<ModuleData>();

        ReferenceEquals(firstStore, secondStore).Should().BeTrue();
    }

    [Fact]
    public void Initialize_Should_Be_Idempotent_ForLessonData()
    {
        var services = new ServiceCollection();
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,
            typeof(ScriptumPersistenceServiceModule).Assembly);
        _serviceProvider = services.BuildServiceProvider();

        var initializer = new ScriptumDataStoreInitializer();

        initializer.Initialize(_serviceProvider);
        var dataStoreProvider = _serviceProvider.GetRequiredService<IDataStoreProvider>();
        var firstStore = dataStoreProvider.GetDataStore<LessonData>();

        initializer.Initialize(_serviceProvider);
        var secondStore = dataStoreProvider.GetDataStore<LessonData>();

        ReferenceEquals(firstStore, secondStore).Should().BeTrue();
    }

    [Fact]
    public void Initialize_Should_Be_Idempotent_ForLessonGuideData()
    {
        var services = new ServiceCollection();
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,
            typeof(ScriptumPersistenceServiceModule).Assembly);
        _serviceProvider = services.BuildServiceProvider();

        var initializer = new ScriptumDataStoreInitializer();

        initializer.Initialize(_serviceProvider);
        var dataStoreProvider = _serviceProvider.GetRequiredService<IDataStoreProvider>();
        var firstStore = dataStoreProvider.GetDataStore<LessonGuideData>();

        initializer.Initialize(_serviceProvider);
        var secondStore = dataStoreProvider.GetDataStore<LessonGuideData>();

        ReferenceEquals(firstStore, secondStore).Should().BeTrue();
    }

    [Fact]
    public void Initialize_Should_Return_SameInstance_OnMultipleCalls_ForModuleData()
    {
        var services = new ServiceCollection();
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,
            typeof(ScriptumPersistenceServiceModule).Assembly);
        _serviceProvider = services.BuildServiceProvider();

        var initializer = new ScriptumDataStoreInitializer();
        initializer.Initialize(_serviceProvider);

        var dataStoreProvider = _serviceProvider.GetRequiredService<IDataStoreProvider>();
        var firstCall = dataStoreProvider.GetDataStore<ModuleData>();
        var secondCall = dataStoreProvider.GetDataStore<ModuleData>();

        ReferenceEquals(firstCall, secondCall).Should().BeTrue();
    }

    [Fact]
    public void Initialize_Should_Return_SameInstance_OnMultipleCalls_ForLessonData()
    {
        var services = new ServiceCollection();
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,
            typeof(ScriptumPersistenceServiceModule).Assembly);
        _serviceProvider = services.BuildServiceProvider();

        var initializer = new ScriptumDataStoreInitializer();
        initializer.Initialize(_serviceProvider);

        var dataStoreProvider = _serviceProvider.GetRequiredService<IDataStoreProvider>();
        var firstCall = dataStoreProvider.GetDataStore<LessonData>();
        var secondCall = dataStoreProvider.GetDataStore<LessonData>();

        ReferenceEquals(firstCall, secondCall).Should().BeTrue();
    }

    [Fact]
    public void Initialize_Should_Return_SameInstance_OnMultipleCalls_ForLessonGuideData()
    {
        var services = new ServiceCollection();
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,
            typeof(ScriptumPersistenceServiceModule).Assembly);
        _serviceProvider = services.BuildServiceProvider();

        var initializer = new ScriptumDataStoreInitializer();
        initializer.Initialize(_serviceProvider);

        var dataStoreProvider = _serviceProvider.GetRequiredService<IDataStoreProvider>();
        var firstCall = dataStoreProvider.GetDataStore<LessonGuideData>();
        var secondCall = dataStoreProvider.GetDataStore<LessonGuideData>();

        ReferenceEquals(firstCall, secondCall).Should().BeTrue();
    }
}
