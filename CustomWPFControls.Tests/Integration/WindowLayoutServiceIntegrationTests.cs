using System;
using System.Linq;
using System.Windows;
using CustomWPFControls.Services;
using DataToolKit.Abstractions;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.DataStores;
using DataToolKit.Storage.Repositories;
using TestHelper.TestUtils;
using Xunit;
using FluentAssertions;

namespace CustomWPFControls.Tests.Integration;

/// <summary>
/// Integrationstests für <see cref="WindowLayoutService"/> mit echtem DataStore und Repository.
/// Verwendet <see cref="TestDirectorySandbox"/> für saubere Test-Umgebungen.
/// </summary>
public sealed class WindowLayoutServiceIntegrationTests : IDisposable
{
    private readonly TestDirectorySandbox _sandbox;
    private readonly IDataStoreProvider _provider;
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly WindowLayoutService _sut;

    public WindowLayoutServiceIntegrationTests()
    {
        _sandbox = new TestDirectorySandbox();

        // Echte Dependencies mit Sandbox aufbauen
        var options = new JsonStorageOptions<WindowLayoutData>(
            appSubFolder: "CustomWPFControls_Tests",
            fileNameBase: $"WindowLayouts_{Guid.NewGuid():N}",
            subFolder: null,
            rootFolder: _sandbox.Root);

        var repository = new JsonRepository<WindowLayoutData>(options);
        var dataStoreFactory = new DataStoreFactory();
        _provider = new DataStoreProvider(dataStoreFactory);
        _repositoryFactory = new RepositoryFactory(new MockServiceProvider(repository));

        _sut = new WindowLayoutService(_provider, _repositoryFactory);
    }

    [StaFact]
    public void Attach_NewWindow_ShouldCreateNewLayoutData()
    {
        // Arrange
        var window = CreateTestWindow();
        window.Left = 100;
        window.Top = 200;
        window.Width = 800;
        window.Height = 600;

        // Act
        _sut.Attach(window, "TestWindow");

        // Assert
        var dataStore = _provider.GetPersistent<WindowLayoutData>(
            _repositoryFactory, 
            isSingleton: true, 
            trackPropertyChanges: true, 
            autoLoad: false);

        var layoutData = dataStore.Items.FirstOrDefault(x => x.WindowKey == "TestWindow");
        layoutData.Should().NotBeNull();
        layoutData!.Left.Should().Be(100);
        layoutData.Top.Should().Be(200);
        layoutData.Width.Should().Be(800);
        layoutData.Height.Should().Be(600);
    }

    [StaFact]
    public void Attach_ExistingLayout_ShouldRestoreWindowPosition()
    {
        // Arrange - Erste Window-Instanz mit Position
        var window1 = CreateTestWindow();
        window1.Left = 150;
        window1.Top = 250;
        window1.Width = 1024;
        window1.Height = 768;
        _sut.Attach(window1, "RestoredWindow");
        _sut.Detach("RestoredWindow");

        // Act - Zweite Window-Instanz sollte Position wiederherstellen
        var window2 = CreateTestWindow();
        window2.Left = 0;
        window2.Top = 0;
        _sut.Attach(window2, "RestoredWindow");

        // Assert
        window2.Left.Should().Be(150);
        window2.Top.Should().Be(250);
        window2.Width.Should().Be(1024);
        window2.Height.Should().Be(768);
    }

    [StaFact]
    public void Detach_ShouldRemoveWindowFromTracking()
    {
        // Arrange
        var window = CreateTestWindow();
        _sut.Attach(window, "DetachTest");

        // Act
        _sut.Detach("DetachTest");

        // Assert - Sollte kein zweites Attach mit gleichem Key verhindern
        var window2 = CreateTestWindow();
        Action act = () => _sut.Attach(window2, "DetachTest");
        act.Should().NotThrow();
    }

    [StaFact]
    public void MultipleWindows_ShouldBeManagedIndependently()
    {
        // Arrange
        var window1 = CreateTestWindow();
        var window2 = CreateTestWindow();
        var window3 = CreateTestWindow();

        window1.Left = 100;
        window2.Left = 200;
        window3.Left = 300;

        // Act
        _sut.Attach(window1, "Window1");
        _sut.Attach(window2, "Window2");
        _sut.Attach(window3, "Window3");

        // Assert
        var dataStore = _provider.GetPersistent<WindowLayoutData>(
            _repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: true,
            autoLoad: false);

        dataStore.Items.Count.Should().Be(3);
        dataStore.Items.First(x => x.WindowKey == "Window1").Left.Should().Be(100);
        dataStore.Items.First(x => x.WindowKey == "Window2").Left.Should().Be(200);
        dataStore.Items.First(x => x.WindowKey == "Window3").Left.Should().Be(300);
    }

    [StaFact]
    public void Persistence_ShouldSurviveServiceRecreation()
    {
        // Arrange - Erste Service-Instanz erstellt Layout
        var window1 = CreateTestWindow();
        window1.Left = 500;
        window1.Top = 500;
        _sut.Attach(window1, "PersistenceTest");
        _sut.Dispose();

        // Act - Neue Service-Instanz sollte Daten laden
        var newService = new WindowLayoutService(_provider, _repositoryFactory);
        var window2 = CreateTestWindow();
        newService.Attach(window2, "PersistenceTest");

        // Assert
        window2.Left.Should().Be(500);
        window2.Top.Should().Be(500);

        newService.Dispose();
    }

    private static Window CreateTestWindow()
    {
        return new Window
        {
            Width = 640,
            Height = 480,
            WindowStartupLocation = WindowStartupLocation.Manual,
            ShowActivated = false,
            Visibility = Visibility.Hidden
        };
    }

    public void Dispose()
    {
        _sut?.Dispose();
        _sandbox?.Dispose();
    }

    private class MockServiceProvider : IServiceProvider
    {
        private readonly object _repository;

        public MockServiceProvider(object repository)
        {
            _repository = repository;
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IRepositoryBase<WindowLayoutData>))
                return _repository;
            return null;
        }
    }
}
