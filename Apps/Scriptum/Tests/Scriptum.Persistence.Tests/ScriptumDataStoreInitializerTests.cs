using System;
using Common.Bootstrap;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.DI;
using DataToolKit.Storage.DataStores;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Scriptum.Progress;
using Xunit;

namespace Scriptum.Persistence.Tests;

[Collection("LiteDB Tests")]
public sealed class ScriptumDataStoreInitializerTests : IDisposable
{
    private ServiceProvider? _serviceProvider;

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    [Fact]
    public void Initialize_Should_Create_PersistentDataStore()
    {
        var services = new ServiceCollection();
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,
            typeof(ScriptumPersistenceServiceModule).Assembly);
        _serviceProvider = services.BuildServiceProvider();

        var initializer = new ScriptumDataStoreInitializer();

        initializer.Initialize(_serviceProvider);

        var dataStoreProvider = _serviceProvider.GetRequiredService<IDataStoreProvider>();
        var dataStore = dataStoreProvider.GetDataStore<TrainingSession>();

        dataStore.Should().NotBeNull();
        dataStore.Should().BeOfType<PersistentDataStore<TrainingSession>>();
    }

    [Fact]
    public void Initialize_Should_Be_Idempotent()
    {
        var services = new ServiceCollection();
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,
            typeof(ScriptumPersistenceServiceModule).Assembly);
        _serviceProvider = services.BuildServiceProvider();

        var initializer = new ScriptumDataStoreInitializer();

        initializer.Initialize(_serviceProvider);
        var dataStoreProvider = _serviceProvider.GetRequiredService<IDataStoreProvider>();
        var firstStore = dataStoreProvider.GetDataStore<TrainingSession>();

        initializer.Initialize(_serviceProvider);
        var secondStore = dataStoreProvider.GetDataStore<TrainingSession>();

        ReferenceEquals(firstStore, secondStore).Should().BeTrue();
    }

    [Fact]
    public void Initialize_Should_Return_SameInstance_OnMultipleCalls()
    {
        var services = new ServiceCollection();
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,
            typeof(ScriptumPersistenceServiceModule).Assembly);
        _serviceProvider = services.BuildServiceProvider();

        var initializer = new ScriptumDataStoreInitializer();
        initializer.Initialize(_serviceProvider);

        var dataStoreProvider = _serviceProvider.GetRequiredService<IDataStoreProvider>();
        var firstCall = dataStoreProvider.GetDataStore<TrainingSession>();
        var secondCall = dataStoreProvider.GetDataStore<TrainingSession>();

        ReferenceEquals(firstCall, secondCall).Should().BeTrue();
    }

    [Fact]
    public void Initialize_Should_Throw_WhenIDataStoreProviderNotRegistered()
    {
        var services = new ServiceCollection();
        _serviceProvider = services.BuildServiceProvider();

        var initializer = new ScriptumDataStoreInitializer();

        Action act = () => initializer.Initialize(_serviceProvider);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Initialize_Should_Throw_WhenIRepositoryFactoryNotRegistered()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDataStoreProvider, DataStoreProvider>();
        _serviceProvider = services.BuildServiceProvider();

        var initializer = new ScriptumDataStoreInitializer();

        Action act = () => initializer.Initialize(_serviceProvider);

        act.Should().Throw<InvalidOperationException>();
    }
}
