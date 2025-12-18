using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.DataStores;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Scriptum.Content.Data;
using Scriptum.Persistence;
using Xunit;

namespace Scriptum.Content.Tests.Integration;

public sealed class ScriptumContentPersistenceIntegrationTests
{
    [Fact]
    public void FullWorkflow_Should_CreateAndPersistModuleData()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();
        module.Register(services);

        var provider = services.BuildServiceProvider();
        var initializer = new ScriptumDataStoreInitializer();

        initializer.Initialize(provider);

        var dataStoreProvider = provider.GetRequiredService<IDataStoreProvider>();
        var dataStore = (PersistentDataStore<ModuleData>)dataStoreProvider.GetDataStore<ModuleData>();

        dataStore.Items.Should().BeEmpty();

        var module1 = new ModuleData("module1", "Grundlagen", "Einführung", 1);

        dataStore.Add(module1);

        dataStore.Items.Should().HaveCount(1);
    }

    [Fact]
    public void FullWorkflow_Should_CreateAndPersistLessonData()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();
        module.Register(services);

        var provider = services.BuildServiceProvider();
        var initializer = new ScriptumDataStoreInitializer();

        initializer.Initialize(provider);

        var dataStoreProvider = provider.GetRequiredService<IDataStoreProvider>();
        var dataStore = (PersistentDataStore<LessonData>)dataStoreProvider.GetDataStore<LessonData>();

        dataStore.Items.Should().BeEmpty();

        var lesson = new LessonData(
            "lesson1", 
            "module1", 
            "Lektion 1",
            tags: new[] { "anfänger" },
            uebungstext: "aaa bbb");

        dataStore.Add(lesson);

        dataStore.Items.Should().HaveCount(1);
    }

    [Fact]
    public void FullWorkflow_Should_CreateAndPersistLessonGuideData()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();
        module.Register(services);

        var provider = services.BuildServiceProvider();
        var initializer = new ScriptumDataStoreInitializer();

        initializer.Initialize(provider);

        var dataStoreProvider = provider.GetRequiredService<IDataStoreProvider>();
        var dataStore = (PersistentDataStore<LessonGuideData>)dataStoreProvider.GetDataStore<LessonGuideData>();

        dataStore.Items.Should().BeEmpty();

        var guide = new LessonGuideData("lesson1", "# Anleitung");

        dataStore.Add(guide);

        dataStore.Items.Should().HaveCount(1);
    }

    [Fact]
    public void Repository_Should_BeResolvable_ForModuleData()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();
        module.Register(services);

        var provider = services.BuildServiceProvider();

        var repository = provider.GetService<IRepositoryBase<ModuleData>>();

        repository.Should().NotBeNull();
    }

    [Fact]
    public void Repository_Should_BeResolvable_ForLessonData()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();
        module.Register(services);

        var provider = services.BuildServiceProvider();

        var repository = provider.GetService<IRepositoryBase<LessonData>>();

        repository.Should().NotBeNull();
    }

    [Fact]
    public void Repository_Should_BeResolvable_ForLessonGuideData()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();
        module.Register(services);

        var provider = services.BuildServiceProvider();

        var repository = provider.GetService<IRepositoryBase<LessonGuideData>>();

        repository.Should().NotBeNull();
    }

    [Fact]
    public void DataStore_Should_AutoLoad_ModuleData_OnInitialization()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();
        module.Register(services);

        var provider = services.BuildServiceProvider();

        var repository = provider.GetRequiredService<IRepositoryBase<ModuleData>>();
        var testModule = new ModuleData("preseeded", "PreSeeded Module");
        repository.Write(new[] { testModule });

        var initializer = new ScriptumDataStoreInitializer();
        initializer.Initialize(provider);

        var dataStoreProvider = provider.GetRequiredService<IDataStoreProvider>();
        var dataStore = dataStoreProvider.GetDataStore<ModuleData>();

        dataStore.Items.Should().HaveCount(1);
        dataStore.Items.Should().Contain(m => m.ModuleId == "preseeded");
    }

    [Fact]
    public void DataStore_Should_AutoLoad_LessonData_OnInitialization()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();
        module.Register(services);

        var provider = services.BuildServiceProvider();

        var repository = provider.GetRequiredService<IRepositoryBase<LessonData>>();
        var testLesson = new LessonData("preseeded", "module1", "PreSeeded Lesson", uebungstext: "test");
        repository.Write(new[] { testLesson });

        var initializer = new ScriptumDataStoreInitializer();
        initializer.Initialize(provider);

        var dataStoreProvider = provider.GetRequiredService<IDataStoreProvider>();
        var dataStore = dataStoreProvider.GetDataStore<LessonData>();

        dataStore.Items.Should().HaveCount(1);
        dataStore.Items.Should().Contain(l => l.LessonId == "preseeded");
    }

    [Fact]
    public void DataStore_Should_AutoLoad_LessonGuideData_OnInitialization()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();
        module.Register(services);

        var provider = services.BuildServiceProvider();

        var repository = provider.GetRequiredService<IRepositoryBase<LessonGuideData>>();
        var testGuide = new LessonGuideData("preseeded", "# Guide");
        repository.Write(new[] { testGuide });

        var initializer = new ScriptumDataStoreInitializer();
        initializer.Initialize(provider);

        var dataStoreProvider = provider.GetRequiredService<IDataStoreProvider>();
        var dataStore = dataStoreProvider.GetDataStore<LessonGuideData>();

        dataStore.Items.Should().HaveCount(1);
        dataStore.Items.Should().Contain(g => g.LessonId == "preseeded");
    }

    [Fact]
    public void PersistentDataStore_Should_PersistModuleData_Immediately()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();
        module.Register(services);

        var provider = services.BuildServiceProvider();
        var initializer = new ScriptumDataStoreInitializer();
        initializer.Initialize(provider);

        var dataStoreProvider = provider.GetRequiredService<IDataStoreProvider>();
        var dataStore = (PersistentDataStore<ModuleData>)dataStoreProvider.GetDataStore<ModuleData>();

        var moduleData = new ModuleData("module1", "Grundlagen", "Test", 1);

        dataStore.Add(moduleData);

        var repository = provider.GetRequiredService<IRepositoryBase<ModuleData>>();
        var persisted = repository.Load();

        persisted.Should().HaveCount(1);
        persisted[0].ModuleId.Should().Be("module1");
        persisted[0].Titel.Should().Be("Grundlagen");
    }

    [Fact]
    public void PersistentDataStore_Should_PersistLessonData_Immediately()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();
        module.Register(services);

        var provider = services.BuildServiceProvider();
        var initializer = new ScriptumDataStoreInitializer();
        initializer.Initialize(provider);

        var dataStoreProvider = provider.GetRequiredService<IDataStoreProvider>();
        var dataStore = (PersistentDataStore<LessonData>)dataStoreProvider.GetDataStore<LessonData>();

        var lessonData = new LessonData("lesson1", "module1", "Lektion 1", uebungstext: "text");

        dataStore.Add(lessonData);

        var repository = provider.GetRequiredService<IRepositoryBase<LessonData>>();
        var persisted = repository.Load();

        persisted.Should().HaveCount(1);
        persisted[0].LessonId.Should().Be("lesson1");
        persisted[0].Titel.Should().Be("Lektion 1");
    }

    [Fact]
    public void PersistentDataStore_Should_PersistLessonGuideData_Immediately()
    {
        var services = new ServiceCollection();
        var module = new ScriptumPersistenceServiceModule();
        module.Register(services);

        var provider = services.BuildServiceProvider();
        var initializer = new ScriptumDataStoreInitializer();
        initializer.Initialize(provider);

        var dataStoreProvider = provider.GetRequiredService<IDataStoreProvider>();
        var dataStore = (PersistentDataStore<LessonGuideData>)dataStoreProvider.GetDataStore<LessonGuideData>();

        var guideData = new LessonGuideData("lesson1", "# Guide");

        dataStore.Add(guideData);

        var repository = provider.GetRequiredService<IRepositoryBase<LessonGuideData>>();
        var persisted = repository.Load();

        persisted.Should().HaveCount(1);
        persisted[0].LessonId.Should().Be("lesson1");
        persisted[0].GuideTextMarkdown.Should().Be("# Guide");
    }
}
