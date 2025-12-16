using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.Repositories;
using FluentAssertions;
using Moq;
using TypeTutor.Logic.Data;
using TypeTutor.Logic.Tests.Helpers;
using Xunit;

namespace TypeTutor.Logic.Tests.Data;

/// <summary>
/// Tests für DataStoreWrapper.
/// Validiert die korrekte Integration mit PersistentDataStores und Collections.
/// </summary>
public sealed class DataStoreWrapperTests
{
    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Arrange
        using var fixture = new ServiceProviderFixture();
        var provider = fixture.GetRequiredService<IDataStoreProvider>();
        var factory = fixture.GetRequiredService<IRepositoryFactory>();

        // Act
        var wrapper = new DataStoreWrapper(provider, factory);

        // Assert
        wrapper.Should().NotBeNull();
        wrapper.Lessons.Should().NotBeNull();
        wrapper.LessonGuides.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullProvider_ShouldThrowArgumentNullException()
    {
        // Arrange
        var factory = Mock.Of<IRepositoryFactory>();

        // Act
        Action act = () => new DataStoreWrapper(null!, factory);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("provider");
    }

    [Fact]
    public void Constructor_WithNullFactory_ShouldThrowArgumentNullException()
    {
        // Arrange
        var provider = Mock.Of<IDataStoreProvider>();

        // Act
        Action act = () => new DataStoreWrapper(provider, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("repositoryFactory");
    }

    [Fact]
    public void Lessons_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        using var fixture = new ServiceProviderFixture();
        var wrapper = fixture.GetRequiredService<DataStoreWrapper>();

        // Act
        var lessons = wrapper.Lessons;

        // Assert
        lessons.Should().NotBeNull();
        lessons.Should().BeAssignableTo<IReadOnlyList<LessonData>>();
    }

    [Fact]
    public void LessonGuides_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        using var fixture = new ServiceProviderFixture();
        var wrapper = fixture.GetRequiredService<DataStoreWrapper>();

        // Act
        var guides = wrapper.LessonGuides;

        // Assert
        guides.Should().NotBeNull();
        guides.Should().BeAssignableTo<IReadOnlyList<LessonGuideData>>();
    }

    [Fact]
    public void LessonDataStore_ShouldProvideDirectAccess()
    {
        // Arrange
        using var fixture = new ServiceProviderFixture();
        var wrapper = fixture.GetRequiredService<DataStoreWrapper>();

        // Act
        var dataStore = wrapper.LessonDataStore;

        // Assert
        dataStore.Should().NotBeNull();
    }

    [Fact]
    public void LessonGuideDataStore_ShouldProvideDirectAccess()
    {
        // Arrange
        using var fixture = new ServiceProviderFixture();
        var wrapper = fixture.GetRequiredService<DataStoreWrapper>();

        // Act
        var dataStore = wrapper.LessonGuideDataStore;

        // Assert
        dataStore.Should().NotBeNull();
    }

    [Fact]
    public void Reload_ShouldNotThrowException()
    {
        // Arrange
        using var fixture = new ServiceProviderFixture();
        var wrapper = fixture.GetRequiredService<DataStoreWrapper>();

        // Act
        Action act = () => wrapper.Reload();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Lessons_AfterAddingToDataStore_ShouldReflectChanges()
    {
        // Arrange
        using var fixture = new ServiceProviderFixture();
        var wrapper = fixture.GetRequiredService<DataStoreWrapper>();
        var initialCount = wrapper.Lessons.Count;
        var newLesson = TestDataBuilder.CreateLessonData(title: "New Test Lesson");

        // Act
        wrapper.LessonDataStore.Add(newLesson);

        // Assert
        wrapper.Lessons.Should().HaveCount(initialCount + 1);
        wrapper.Lessons.Should().Contain(l => l.Title == "New Test Lesson");
    }

    [Fact]
    public void LessonGuides_AfterAddingToDataStore_ShouldReflectChanges()
    {
        // Arrange
        using var fixture = new ServiceProviderFixture();
        var wrapper = fixture.GetRequiredService<DataStoreWrapper>();
        var initialCount = wrapper.LessonGuides.Count;
        var newGuide = TestDataBuilder.CreateLessonGuideData(title: "New Test Guide");

        // Act
        wrapper.LessonGuideDataStore.Add(newGuide);

        // Assert
        wrapper.LessonGuides.Should().HaveCount(initialCount + 1);
        wrapper.LessonGuides.Should().Contain(g => g.Title == "New Test Guide");
    }

    [Fact]
    public void MultipleInstances_FromSameProvider_ShouldShareData()
    {
        // Arrange
        using var fixture = new ServiceProviderFixture();
        var wrapper1 = fixture.GetRequiredService<DataStoreWrapper>();
        var wrapper2 = fixture.GetRequiredService<DataStoreWrapper>();

        // Act & Assert
        wrapper1.Should().BeSameAs(wrapper2);
    }
}
