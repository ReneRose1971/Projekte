using System;
using System.IO;
using Common.Bootstrap;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.DI;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.DataStores;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Scriptum.Progress;
using TestHelper.TestUtils;
using Xunit;

namespace Scriptum.Persistence.Tests;

[Collection("LiteDB Tests")]
public sealed class ScriptumPersistenceIntegrationTests : IDisposable
{
    private readonly TestDirectorySandbox _sandbox;
    private ServiceProvider? _serviceProvider;

    public ScriptumPersistenceIntegrationTests()
    {
        _sandbox = new TestDirectorySandbox();
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
        _sandbox.Dispose();
    }

    private void ClearRepository()
    {
        if (_serviceProvider != null)
        {
            _serviceProvider.GetRequiredService<IRepositoryBase<TrainingSession>>().Clear();
        }
    }

    [Fact]
    public void FullWorkflow_Should_CreateAndPersistTrainingSession()
    {
        var services = new ServiceCollection();
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,
            typeof(ScriptumPersistenceServiceModule).Assembly);

        _serviceProvider = services.BuildServiceProvider();
        ClearRepository();
        
        var initializer = new ScriptumDataStoreInitializer();
        initializer.Initialize(_serviceProvider);

        var dataStoreProvider = _serviceProvider.GetRequiredService<IDataStoreProvider>();
        var dataStore = (PersistentDataStore<TrainingSession>)dataStoreProvider.GetDataStore<TrainingSession>();

        dataStore.Items.Should().BeEmpty();

        var session = new TrainingSession
        {
            LessonId = "Lesson1",
            ModuleId = "Module1",
            StartedAt = DateTimeOffset.UtcNow,
            IsCompleted = false
        };

        dataStore.Add(session);

        dataStore.Items.Should().HaveCount(1);
        session.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Repository_Should_BeResolvable()
    {
        var services = new ServiceCollection();
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,
            typeof(ScriptumPersistenceServiceModule).Assembly);

        _serviceProvider = services.BuildServiceProvider();

        var repository = _serviceProvider.GetService<IRepository<TrainingSession>>();
        var repositoryBase = _serviceProvider.GetService<IRepositoryBase<TrainingSession>>();

        repository.Should().NotBeNull();
        repositoryBase.Should().NotBeNull();
        ReferenceEquals(repository, repositoryBase).Should().BeTrue();
    }

    [Fact]
    public void DataStore_Should_AutoLoad_OnInitialization()
    {
        var services = new ServiceCollection();
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,
            typeof(ScriptumPersistenceServiceModule).Assembly);

        _serviceProvider = services.BuildServiceProvider();
        ClearRepository();

        var repository = _serviceProvider.GetRequiredService<IRepository<TrainingSession>>();
        var testSession = new TrainingSession
        {
            LessonId = "PreSeeded",
            ModuleId = "Module1",
            StartedAt = DateTimeOffset.UtcNow,
            IsCompleted = false
        };
        repository.Write(new[] { testSession });

        var initializer = new ScriptumDataStoreInitializer();
        initializer.Initialize(_serviceProvider);

        var dataStoreProvider = _serviceProvider.GetRequiredService<IDataStoreProvider>();
        var dataStore = dataStoreProvider.GetDataStore<TrainingSession>();

        dataStore.Items.Should().HaveCount(1);
        dataStore.Items.Should().Contain(s => s.LessonId == "PreSeeded");
    }

    [Fact]
    public void PersistentDataStore_Should_PersistChanges_Immediately()
    {
        var services = new ServiceCollection();
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,
            typeof(ScriptumPersistenceServiceModule).Assembly);

        _serviceProvider = services.BuildServiceProvider();
        ClearRepository();
        
        var initializer = new ScriptumDataStoreInitializer();
        initializer.Initialize(_serviceProvider);

        var dataStoreProvider = _serviceProvider.GetRequiredService<IDataStoreProvider>();
        var dataStore = (PersistentDataStore<TrainingSession>)dataStoreProvider.GetDataStore<TrainingSession>();

        var session = new TrainingSession
        {
            LessonId = "Lesson1",
            ModuleId = "Module1",
            StartedAt = DateTimeOffset.UtcNow,
            IsCompleted = false
        };

        dataStore.Add(session);

        var repository = _serviceProvider.GetRequiredService<IRepository<TrainingSession>>();
        var persisted = repository.Load();

        persisted.Should().HaveCount(1);
        persisted[0].LessonId.Should().Be("Lesson1");
    }
}
