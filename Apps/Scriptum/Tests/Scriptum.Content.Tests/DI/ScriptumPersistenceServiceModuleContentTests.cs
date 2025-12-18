using DataToolKit.Abstractions.Repositories;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Scriptum.Content.Comparers;
using Scriptum.Content.Data;
using Scriptum.Persistence;
using Xunit;

namespace Scriptum.Content.Tests.DI;

[Collection("LiteDB Tests")]
public sealed class ScriptumPersistenceServiceModuleContentTests : IDisposable
{
    private ServiceProvider? _serviceProvider;

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    [Fact]
    public void Register_Should_Register_ModuleDataComparer()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();

        module.Register(services);
        _serviceProvider = services.BuildServiceProvider();

        var comparer = _serviceProvider.GetService<IEqualityComparer<ModuleData>>();

        comparer.Should().NotBeNull();
        comparer.Should().BeOfType<ModuleDataComparer>();
    }

    [Fact]
    public void Register_Should_Register_LessonDataComparer()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();

        module.Register(services);
        _serviceProvider = services.BuildServiceProvider();

        var comparer = _serviceProvider.GetService<IEqualityComparer<LessonData>>();

        comparer.Should().NotBeNull();
        comparer.Should().BeOfType<LessonDataComparer>();
    }

    [Fact]
    public void Register_Should_Register_LessonGuideDataComparer()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();

        module.Register(services);
        _serviceProvider = services.BuildServiceProvider();

        var comparer = _serviceProvider.GetService<IEqualityComparer<LessonGuideData>>();

        comparer.Should().NotBeNull();
        comparer.Should().BeOfType<LessonGuideDataComparer>();
    }

    [Fact]
    public void Register_Should_Register_IRepositoryBaseOfModuleData()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();

        module.Register(services);
        _serviceProvider = services.BuildServiceProvider();

        var repository = _serviceProvider.GetService<IRepositoryBase<ModuleData>>();

        repository.Should().NotBeNull();
    }

    [Fact]
    public void Register_Should_Register_IRepositoryBaseOfLessonData()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();

        module.Register(services);
        _serviceProvider = services.BuildServiceProvider();

        var repository = _serviceProvider.GetService<IRepositoryBase<LessonData>>();

        repository.Should().NotBeNull();
    }

    [Fact]
    public void Register_Should_Register_IRepositoryBaseOfLessonGuideData()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();

        module.Register(services);
        _serviceProvider = services.BuildServiceProvider();

        var repository = _serviceProvider.GetService<IRepositoryBase<LessonGuideData>>();

        repository.Should().NotBeNull();
    }

    [Fact]
    public void Register_Should_NotRegister_IRepositoryOfModuleData()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();

        module.Register(services);
        _serviceProvider = services.BuildServiceProvider();

        var repository = _serviceProvider.GetService<IRepository<ModuleData>>();

        repository.Should().BeNull();
    }

    [Fact]
    public void Register_Should_NotRegister_IRepositoryOfLessonData()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();

        module.Register(services);
        _serviceProvider = services.BuildServiceProvider();

        var repository = _serviceProvider.GetService<IRepository<LessonData>>();

        repository.Should().BeNull();
    }

    [Fact]
    public void Register_Should_NotRegister_IRepositoryOfLessonGuideData()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();

        module.Register(services);
        _serviceProvider = services.BuildServiceProvider();

        var repository = _serviceProvider.GetService<IRepository<LessonGuideData>>();

        repository.Should().BeNull();
    }
}
