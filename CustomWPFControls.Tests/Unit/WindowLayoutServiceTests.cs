using System;
using CustomWPFControls.Services;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Storage.DataStores;
using DataToolKit.Storage.Repositories;
using Moq;
using Xunit;
using FluentAssertions;

namespace CustomWPFControls.Tests.Unit;

/// <summary>
/// Unit-Tests für <see cref="WindowLayoutService"/>.
/// Fokus: Isolierte Tests der Kernlogik ohne Window-Objekte.
/// </summary>
public sealed class WindowLayoutServiceTests : IDisposable
{
    private readonly Mock<IDataStoreProvider> _mockProvider;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly InMemoryDataStore<WindowLayoutData> _dataStore;
    private readonly WindowLayoutService _sut;

    public WindowLayoutServiceTests()
    {
        _mockProvider = new Mock<IDataStoreProvider>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        
        // Echten InMemoryDataStore verwenden (kann nicht gemockt werden)
        _dataStore = new InMemoryDataStore<WindowLayoutData>();
        
        // Repository-Mock
        var mockRepo = new Mock<DataToolKit.Abstractions.Repositories.IRepositoryBase<WindowLayoutData>>();
        mockRepo.Setup(r => r.Load()).Returns(Array.Empty<WindowLayoutData>());
        
        // Echten PersistentDataStore erstellen (sealed class kann nicht gemockt werden)
        var persistentStore = new PersistentDataStore<WindowLayoutData>(mockRepo.Object, trackPropertyChanges: false);

        _mockProvider
            .Setup(p => p.GetPersistent<WindowLayoutData>(
                It.IsAny<IRepositoryFactory>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(persistentStore);

        _sut = new WindowLayoutService(_mockProvider.Object, _mockRepositoryFactory.Object);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenProviderIsNull()
    {
        // Act
        Action act = () => new WindowLayoutService(null!, _mockRepositoryFactory.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("provider");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenRepositoryFactoryIsNull()
    {
        // Act
        Action act = () => new WindowLayoutService(_mockProvider.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("repositoryFactory");
    }

    [Fact]
    public void Constructor_ShouldRequestPersistentDataStoreWithCorrectParameters()
    {
        // Assert
        _mockProvider.Verify(p => p.GetPersistent<WindowLayoutData>(
            _mockRepositoryFactory.Object,
            true,  // isSingleton
            true,  // trackPropertyChanges
            true), // autoLoad
            Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Attach_ShouldThrowArgumentException_WhenKeyIsNullOrWhitespace(string? key)
    {
        // Act - Test ohne echtes Window-Objekt
        Action act = () => _sut.Attach(null!, key!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Detach_ShouldNotThrow_WhenKeyIsNull()
    {
        // Act
        Action act = () => _sut.Detach(null!);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Detach_ShouldNotThrow_WhenKeyDoesNotExist()
    {
        // Act
        Action act = () => _sut.Detach("NonExistentKey");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_ShouldBeIdempotent()
    {
        // Act
        _sut.Dispose();
        Action act = () => _sut.Dispose();

        // Assert
        act.Should().NotThrow();
    }

    public void Dispose()
    {
        _sut?.Dispose();
    }
}
