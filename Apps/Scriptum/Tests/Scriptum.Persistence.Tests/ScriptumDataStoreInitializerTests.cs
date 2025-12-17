using System;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Storage.DataStores;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Scriptum.Progress;
using Xunit;

namespace Scriptum.Persistence.Tests;

public sealed class ScriptumDataStoreInitializerTests
{
    [Fact]
    public void Initialize_Should_Create_PersistentDataStore()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();
        module.Register(services);
        var provider = services.BuildServiceProvider();

        var initializer = new ScriptumDataStoreInitializer();

        initializer.Initialize(provider);

        var dataStoreProvider = provider.GetRequiredService<IDataStoreProvider>();
        var dataStore = dataStoreProvider.GetDataStore<TrainingSession>();

        dataStore.Should().NotBeNull();
        dataStore.Should().BeOfType<PersistentDataStore<TrainingSession>>();
    }

    [Fact]
    public void Initialize_Should_Be_Idempotent()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();
        module.Register(services);
        var provider = services.BuildServiceProvider();

        var initializer = new ScriptumDataStoreInitializer();

        initializer.Initialize(provider);
        var dataStoreProvider = provider.GetRequiredService<IDataStoreProvider>();
        var firstStore = dataStoreProvider.GetDataStore<TrainingSession>();

        initializer.Initialize(provider);
        var secondStore = dataStoreProvider.GetDataStore<TrainingSession>();

        ReferenceEquals(firstStore, secondStore).Should().BeTrue();
    }

    [Fact]
    public void Initialize_Should_Return_SameInstance_OnMultipleCalls()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();
        module.Register(services);
        var provider = services.BuildServiceProvider();

        var initializer = new ScriptumDataStoreInitializer();
        initializer.Initialize(provider);

        var dataStoreProvider = provider.GetRequiredService<IDataStoreProvider>();
        var firstCall = dataStoreProvider.GetDataStore<TrainingSession>();
        var secondCall = dataStoreProvider.GetDataStore<TrainingSession>();

        ReferenceEquals(firstCall, secondCall).Should().BeTrue();
    }

    [Fact]
    public void Initialize_Should_Throw_WhenIDataStoreProviderNotRegistered()
    {
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();

        var initializer = new ScriptumDataStoreInitializer();

        Action act = () => initializer.Initialize(provider);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Initialize_Should_Throw_WhenIRepositoryFactoryNotRegistered()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDataStoreProvider, DataStoreProvider>();
        var provider = services.BuildServiceProvider();

        var initializer = new ScriptumDataStoreInitializer();

        Action act = () => initializer.Initialize(provider);

        act.Should().Throw<InvalidOperationException>();
    }
}
