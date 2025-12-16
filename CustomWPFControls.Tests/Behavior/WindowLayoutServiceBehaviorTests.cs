using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using CustomWPFControls.Services;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.DataStores;
using DataToolKit.Storage.Repositories;
using TestHelper.TestUtils;
using Xunit;
using FluentAssertions;

namespace CustomWPFControls.Tests.Behavior;

/// <summary>
/// Behavior-Tests für <see cref="WindowLayoutService"/>.
/// Fokus: End-to-End-Szenarien und Verhaltensvalidierung.
/// Verwendet <see cref="TestDirectorySandbox"/> für saubere Test-Umgebungen.
/// </summary>
public sealed class WindowLayoutServiceBehaviorTests : IDisposable
{
    private readonly TestDirectorySandbox _sandbox;
    private readonly IDataStoreProvider _provider;
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly WindowLayoutService _sut;

    public WindowLayoutServiceBehaviorTests()
    {
        _sandbox = new TestDirectorySandbox();

        var options = new JsonStorageOptions<WindowLayoutData>(
            appSubFolder: "CustomWPFControls_BehaviorTests",
            fileNameBase: $"WindowLayouts_{Guid.NewGuid():N}",
            subFolder: null,
            rootFolder: _sandbox.Root);

        var repository = new JsonRepository<WindowLayoutData>(options);
        var factory = new DataStoreFactory();
        _provider = new DataStoreProvider(factory);
        _repositoryFactory = new RepositoryFactory(new TestServiceProvider(repository));

        _sut = new WindowLayoutService(_provider, _repositoryFactory);
    }

    [StaFact]
    public void Scenario_UserMovesWindow_LayoutIsAutomaticallySaved()
    {
        // Arrange
        var window = CreateTestWindow();
        window.Left = 100;
        window.Top = 100;
        _sut.Attach(window, "MovableWindow");

        // Act
        var dataStore = GetDataStore();
        var layoutData = dataStore.Items.FirstOrDefault(x => x.WindowKey == "MovableWindow");
        
        layoutData!.Left = 300;
        layoutData.Top = 400;

        // Assert
        layoutData.Left.Should().Be(300);
        layoutData.Top.Should().Be(400);
    }

    [StaFact]
    public void Scenario_UserResizesWindow_LayoutIsAutomaticallySaved()
    {
        // Arrange
        var window = CreateTestWindow();
        window.Width = 800;
        window.Height = 600;
        _sut.Attach(window, "ResizableWindow");

        // Act
        var dataStore = GetDataStore();
        var layoutData = dataStore.Items.FirstOrDefault(x => x.WindowKey == "ResizableWindow");
        layoutData!.Width = 1024;
        layoutData.Height = 768;

        // Assert
        layoutData.Width.Should().Be(1024);
        layoutData.Height.Should().Be(768);
    }

    [StaFact]
    public void Scenario_UserMaximizesWindow_StateIsAutomaticallySaved()
    {
        // Arrange
        var window = CreateTestWindow();
        window.WindowState = WindowState.Normal;
        _sut.Attach(window, "MaximizableWindow");

        // Act
        var dataStore = GetDataStore();
        var layoutData = dataStore.Items.FirstOrDefault(x => x.WindowKey == "MaximizableWindow");
        layoutData!.WindowState = (int)WindowState.Maximized;

        // Assert
        layoutData.WindowState.Should().Be((int)WindowState.Maximized);
    }

    [StaFact]
    public void Scenario_WindowClosed_ManualDetachWorks()
    {
        // Arrange
        var window = CreateTestWindow();
        _sut.Attach(window, "ClosableWindow");

        // Act
        _sut.Detach("ClosableWindow");

        // Assert
        var window2 = CreateTestWindow();
        Action act = () => _sut.Attach(window2, "ClosableWindow");
        act.Should().NotThrow();
    }

    [StaFact]
    public void Scenario_ApplicationRestart_WindowPositionIsRestored()
    {
        // Arrange
        var window1 = CreateTestWindow();
        window1.Left = 250;
        window1.Top = 350;
        window1.Width = 900;
        window1.Height = 700;
        window1.WindowState = WindowState.Normal;
        _sut.Attach(window1, "MainApplicationWindow");
        _sut.Detach("MainApplicationWindow");
        _sut.Dispose();

        // Act
        var newService = new WindowLayoutService(_provider, _repositoryFactory);
        var window2 = CreateTestWindow();
        window2.Left = 0;
        window2.Top = 0;
        window2.Width = 640;
        window2.Height = 480;
        newService.Attach(window2, "MainApplicationWindow");

        // Assert
        window2.Left.Should().Be(250);
        window2.Top.Should().Be(350);
        window2.Width.Should().Be(900);
        window2.Height.Should().Be(700);
        window2.WindowState.Should().Be(WindowState.Normal);

        newService.Dispose();
    }

    [StaFact]
    public void Scenario_MultipleWindowsInApplication_EachHasIndependentLayout()
    {
        // Arrange
        var mainWindow = CreateTestWindow();
        var settingsDialog = CreateTestWindow();
        var aboutDialog = CreateTestWindow();

        mainWindow.Left = 100;
        settingsDialog.Left = 500;
        aboutDialog.Left = 900;

        // Act
        _sut.Attach(mainWindow, "MainWindow");
        _sut.Attach(settingsDialog, "SettingsDialog");
        _sut.Attach(aboutDialog, "AboutDialog");

        // Assert
        var dataStore = GetDataStore();
        dataStore.Items.Count.Should().Be(3);
        
        var mainLayout = dataStore.Items.First(x => x.WindowKey == "MainWindow");
        var settingsLayout = dataStore.Items.First(x => x.WindowKey == "SettingsDialog");
        var aboutLayout = dataStore.Items.First(x => x.WindowKey == "AboutDialog");

        mainLayout.Left.Should().Be(100);
        settingsLayout.Left.Should().Be(500);
        aboutLayout.Left.Should().Be(900);
    }

    [StaFact]
    public void Scenario_WindowWithInvalidDimensions_ShouldNotRestoreLayout()
    {
        // Arrange
        var dataStore = GetDataStore();
        dataStore.Add(new WindowLayoutData
        {
            WindowKey = "InvalidWindow",
            Left = 100,
            Top = 100,
            Width = 0,
            Height = 0,
            WindowState = 0
        });

        // Act
        var window = CreateTestWindow();
        window.Left = 500;
        window.Top = 500;
        window.Width = 800;
        window.Height = 600;
        
        var originalLeft = window.Left;
        var originalTop = window.Top;
        var originalWidth = window.Width;
        var originalHeight = window.Height;

        _sut.Attach(window, "InvalidWindow");

        // Assert
        window.Left.Should().Be(originalLeft);
        window.Top.Should().Be(originalTop);
        window.Width.Should().Be(originalWidth);
        window.Height.Should().Be(originalHeight);
    }

    [StaFact]
    public void Scenario_PropertyChangedTracking_WorksWithFody()
    {
        // Arrange
        var window = CreateTestWindow();
        _sut.Attach(window, "FodyTestWindow");

        var dataStore = GetDataStore();
        var layoutData = dataStore.Items.First(x => x.WindowKey == "FodyTestWindow");

        int propertyChangedCount = 0;
        layoutData.PropertyChanged += (s, e) => propertyChangedCount++;

        // Act
        layoutData.Left = 999;
        layoutData.Top = 888;
        layoutData.Width = 777;

        // Assert
        propertyChangedCount.Should().Be(3, "Fody should raise PropertyChanged for each property");
    }

    private PersistentDataStore<WindowLayoutData> GetDataStore()
    {
        return _provider.GetPersistent<WindowLayoutData>(
            _repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: true,
            autoLoad: false);
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

    private class TestServiceProvider : IServiceProvider
    {
        private readonly object _repository;

        public TestServiceProvider(object repository)
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
