using Common.Bootstrap;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.DI;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.Repositories;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Scriptum.Progress;
using Xunit;

namespace Scriptum.Persistence.Tests;

[Collection("LiteDB Tests")]
public sealed class ScriptumPersistenceServiceModuleTests : IDisposable
{
    private ServiceProvider? _serviceProvider;

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    [Fact]
    public void Register_Should_Register_IRepositoryOfTrainingSession()
    {
        var services = new ServiceCollection();
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,
            typeof(ScriptumPersistenceServiceModule).Assembly);

        _serviceProvider = services.BuildServiceProvider();

        var repository = _serviceProvider.GetService<IRepository<TrainingSession>>();

        repository.Should().NotBeNull();
    }

    [Fact]
    public void Register_Should_Register_IRepositoryBaseOfTrainingSession()
    {
        var services = new ServiceCollection();
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,
            typeof(ScriptumPersistenceServiceModule).Assembly);

        _serviceProvider = services.BuildServiceProvider();

        var repository = _serviceProvider.GetService<IRepositoryBase<TrainingSession>>();

        repository.Should().NotBeNull();
    }

    [Fact]
    public void Register_Should_Register_IDataStoreProvider()
    {
        var services = new ServiceCollection();
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,
            typeof(ScriptumPersistenceServiceModule).Assembly);

        _serviceProvider = services.BuildServiceProvider();

        var dataStoreProvider = _serviceProvider.GetService<IDataStoreProvider>();

        dataStoreProvider.Should().NotBeNull();
    }

    [Fact]
    public void Register_Should_Register_IRepositoryFactory()
    {
        var services = new ServiceCollection();
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,
            typeof(ScriptumPersistenceServiceModule).Assembly);

        _serviceProvider = services.BuildServiceProvider();

        var repositoryFactory = _serviceProvider.GetService<IRepositoryFactory>();

        repositoryFactory.Should().NotBeNull();
    }

    [Fact]
    public void Register_Should_Return_SameInstance_ForIRepositoryAndIRepositoryBase()
    {
        var services = new ServiceCollection();
        services.AddModulesFromAssemblies(
            typeof(DataToolKitServiceModule).Assembly,
            typeof(ScriptumPersistenceServiceModule).Assembly);

        _serviceProvider = services.BuildServiceProvider();

        var repoBase = _serviceProvider.GetRequiredService<IRepositoryBase<TrainingSession>>();
        var repo = _serviceProvider.GetRequiredService<IRepository<TrainingSession>>();

        ReferenceEquals(repoBase, repo).Should().BeTrue();
    }
}
