using System;
using System.IO;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.DataStores;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Scriptum.Progress;
using TestHelper.TestUtils;
using Xunit;

namespace Scriptum.Persistence.Tests;

/// <summary>
/// Integrationstest für das Zusammenspiel von ServiceModule, DataStoreInitializer und Repository.
/// </summary>
public sealed class ScriptumPersistenceIntegrationTests : IDisposable
{
    private readonly TestDirectorySandbox _sandbox;

    public ScriptumPersistenceIntegrationTests()
    {
        _sandbox = new TestDirectorySandbox();
    }

    public void Dispose()
    {
        _sandbox.Dispose();
    }

    [Fact]
    public void FullWorkflow_Should_CreateAndPersistTrainingSession()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();
        module.Register(services);

        var provider = services.BuildServiceProvider();
        var initializer = new ScriptumDataStoreInitializer();

        initializer.Initialize(provider);

        var dataStoreProvider = provider.GetRequiredService<IDataStoreProvider>();
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
        var module = new ScriptumPersistenceServiceModule();
        module.Register(services);

        var provider = services.BuildServiceProvider();

        var repository = provider.GetService<IRepository<TrainingSession>>();
        var repositoryBase = provider.GetService<IRepositoryBase<TrainingSession>>();

        repository.Should().NotBeNull();
        repositoryBase.Should().NotBeNull();
        ReferenceEquals(repository, repositoryBase).Should().BeTrue();
    }

    [Fact]
    public void DataStore_Should_AutoLoad_OnInitialization()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();
        module.Register(services);

        var provider = services.BuildServiceProvider();

        var repository = provider.GetRequiredService<IRepository<TrainingSession>>();
        var testSession = new TrainingSession
        {
            LessonId = "PreSeeded",
            ModuleId = "Module1",
            StartedAt = DateTimeOffset.UtcNow,
            IsCompleted = false
        };
        repository.Write(new[] { testSession });

        var initializer = new ScriptumDataStoreInitializer();
        initializer.Initialize(provider);

        var dataStoreProvider = provider.GetRequiredService<IDataStoreProvider>();
        var dataStore = dataStoreProvider.GetDataStore<TrainingSession>();

        dataStore.Items.Should().HaveCount(1);
        dataStore.Items.Should().Contain(s => s.LessonId == "PreSeeded");
    }

    [Fact]
    public void PersistentDataStore_Should_PersistChanges_Immediately()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();
        module.Register(services);

        var provider = services.BuildServiceProvider();
        var initializer = new ScriptumDataStoreInitializer();
        initializer.Initialize(provider);

        var dataStoreProvider = provider.GetRequiredService<IDataStoreProvider>();
        var dataStore = (PersistentDataStore<TrainingSession>)dataStoreProvider.GetDataStore<TrainingSession>();

        var session = new TrainingSession
        {
            LessonId = "Lesson1",
            ModuleId = "Module1",
            StartedAt = DateTimeOffset.UtcNow,
            IsCompleted = false
        };

        dataStore.Add(session);

        var repository = provider.GetRequiredService<IRepository<TrainingSession>>();
        var persisted = repository.Load();

        persisted.Should().HaveCount(1);
        persisted[0].LessonId.Should().Be("Lesson1");
    }
}
