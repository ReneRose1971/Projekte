using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.Repositories;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Scriptum.Progress;
using Xunit;

namespace Scriptum.Persistence.Tests;

public sealed class ScriptumPersistenceServiceModuleTests
{
    [Fact]
    public void Register_Should_Register_IRepositoryOfTrainingSession()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();

        module.Register(services);
        var provider = services.BuildServiceProvider();

        var repository = provider.GetService<IRepository<TrainingSession>>();

        repository.Should().NotBeNull();
    }

    [Fact]
    public void Register_Should_Register_IRepositoryBaseOfTrainingSession()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();

        module.Register(services);
        var provider = services.BuildServiceProvider();

        var repository = provider.GetService<IRepositoryBase<TrainingSession>>();

        repository.Should().NotBeNull();
    }

    [Fact]
    public void Register_Should_Register_IDataStoreProvider()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();

        module.Register(services);
        var provider = services.BuildServiceProvider();

        var dataStoreProvider = provider.GetService<IDataStoreProvider>();

        dataStoreProvider.Should().NotBeNull();
    }

    [Fact]
    public void Register_Should_Register_IRepositoryFactory()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();

        module.Register(services);
        var provider = services.BuildServiceProvider();

        var repositoryFactory = provider.GetService<IRepositoryFactory>();

        repositoryFactory.Should().NotBeNull();
    }

    [Fact]
    public void Register_Should_Return_SameInstance_ForIRepositoryAndIRepositoryBase()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();

        module.Register(services);
        var provider = services.BuildServiceProvider();

        var repoBase = provider.GetRequiredService<IRepositoryBase<TrainingSession>>();
        var repo = provider.GetRequiredService<IRepository<TrainingSession>>();

        ReferenceEquals(repoBase, repo).Should().BeTrue();
    }
}
