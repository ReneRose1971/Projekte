using System;
using CustomWPFControls.Services;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Storage.DataStores;
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
    private readonly WindowLayoutService _sut;

    public WindowLayoutServiceTests()
    {
        _mockProvider = new Mock<IDataStoreProvider>();
        
        // Repository-Mock
        var mockRepo = new Mock<DataToolKit.Abstractions.Repositories.IRepositoryBase<WindowLayoutData>>();
        mockRepo.Setup(r => r.Load()).Returns(Array.Empty<WindowLayoutData>());
        
        // Echten PersistentDataStore erstellen (sealed class kann nicht gemockt werden)
        var persistentStore = new PersistentDataStore<WindowLayoutData>(mockRepo.Object, trackPropertyChanges: false);

        // Provider gibt den DataStore über GetDataStore zurück
        _mockProvider
            .Setup(p => p.GetDataStore<WindowLayoutData>())
            .Returns(persistentStore);

        _sut = new WindowLayoutService(_mockProvider.Object);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenProviderIsNull()
    {
        // Act
        Action act = () => new WindowLayoutService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("provider");
    }

    [Fact]
    public void Constructor_ShouldRequestDataStore()
    {
        // Assert - Verify that GetDataStore was called
        _mockProvider.Verify(p => p.GetDataStore<WindowLayoutData>(), Times.Once);
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
