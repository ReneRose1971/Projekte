using CustomWPFControls.Factories;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Storage.DataStores;
using SolutionBundler.Core.Abstractions;
using SolutionBundler.Core.Models;
using SolutionBundler.Core.Storage;
using SolutionBundler.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SolutionBundler.Tests;

/// <summary>
/// Umfassende Tests für ProjectListEditorViewModel.
/// Testet den kompletten Workflow: Dateiauswahl, ProjectInfo-Erzeugung, Items-Collection, Duplikate, Commands.
/// </summary>
public class ProjectListEditorViewModelTests : IDisposable
{
    private readonly TestDataStoreProvider _dataStoreProvider;
    private readonly ProjectStore _projectStore;
    private readonly IViewModelFactory<ProjectInfo, ProjectInfoViewModel> _viewModelFactory;
    private readonly IEqualityComparer<ProjectInfo> _comparer;
    private readonly string _testProjectPath;
    private readonly string _testProjectPath2;

    public ProjectListEditorViewModelTests()
    {
        // Setup: In-Memory DataStoreProvider für isolierte Tests
        _dataStoreProvider = new TestDataStoreProvider();

        _projectStore = new ProjectStore(_dataStoreProvider);

        // Setup: ViewModelFactory für ProjectInfoViewModel
        _viewModelFactory = new SimpleViewModelFactory<ProjectInfo, ProjectInfoViewModel>(
            model => new ProjectInfoViewModel(model));

        // Setup: EqualityComparer für ProjectInfo (basierend auf Name)
        _comparer = new ProjectInfoEqualityComparer();

        // Setup: Erstelle temporäre Test-.csproj-Dateien
        _testProjectPath = Path.Combine(Path.GetTempPath(), "TestProject1.csproj");
        _testProjectPath2 = Path.Combine(Path.GetTempPath(), "TestProject2.csproj");
        File.WriteAllText(_testProjectPath, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");
        File.WriteAllText(_testProjectPath2, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");
    }

    public void Dispose()
    {
        // Cleanup: Lösche temporäre Dateien
        if (File.Exists(_testProjectPath))
            File.Delete(_testProjectPath);
        if (File.Exists(_testProjectPath2))
            File.Delete(_testProjectPath2);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_CreatesViewModel()
    {
        // Arrange
        var mockOrchestrator = new MockBundleOrchestrator();
        
        // Act
        var viewModel = new ProjectListEditorViewModel(
            _projectStore,
            _dataStoreProvider,
            _viewModelFactory,
            _comparer,
            mockOrchestrator);

        // Assert
        Assert.NotNull(viewModel);
        Assert.NotNull(viewModel.Projects);
        Assert.Empty(viewModel.Projects);
    }

    [Fact]
    public void Constructor_WithNullProjectStore_ThrowsArgumentNullException()
    {
        // Arrange
        var mockOrchestrator = new MockBundleOrchestrator();
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ProjectListEditorViewModel(
                null!,
                _dataStoreProvider,
                _viewModelFactory,
                _comparer,
                mockOrchestrator));
    }

    #endregion

    #region ProjectStore Integration Tests

    [Fact]
    public void AddProject_ValidCsprojFile_AddsProjectInfoToItemsCollection()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var initialCount = viewModel.Projects.Count;

        // Act
        var added = _projectStore.AddProject(_testProjectPath);

        // Assert
        Assert.True(added);
        Assert.Equal(initialCount + 1, viewModel.Projects.Count);
        
        var addedProject = viewModel.Projects.FirstOrDefault(p => p.Name == "TestProject1");
        Assert.NotNull(addedProject);
        Assert.Equal(_testProjectPath, addedProject.Path);
    }

    [Fact]
    public void AddProject_MultipleDifferentProjects_AddsAllToCollection()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        var added1 = _projectStore.AddProject(_testProjectPath);
        var added2 = _projectStore.AddProject(_testProjectPath2);

        // Assert
        Assert.True(added1);
        Assert.True(added2);
        Assert.Equal(2, viewModel.Projects.Count);
        Assert.Contains(viewModel.Projects, p => p.Name == "TestProject1");
        Assert.Contains(viewModel.Projects, p => p.Name == "TestProject2");
    }

    [Fact]
    public void AddProject_NonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "NonExistent.csproj");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() =>
            _projectStore.AddProject(nonExistentPath));
    }

    [Fact]
    public void AddProject_InvalidExtension_ThrowsArgumentException()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var invalidPath = Path.Combine(Path.GetTempPath(), "Test.txt");
        File.WriteAllText(invalidPath, "test");

        try
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                _projectStore.AddProject(invalidPath));
        }
        finally
        {
            File.Delete(invalidPath);
        }
    }

    #endregion

    #region Duplicate Handling Tests

    [Fact]
    public void AddProject_DuplicateByPath_ReturnsFalseAndDoesNotAddDuplicate()
    {
        // Arrange
        var viewModel = CreateViewModel();
        _projectStore.AddProject(_testProjectPath);

        // Act
        var addedDuplicate = _projectStore.AddProject(_testProjectPath);

        // Assert
        Assert.False(addedDuplicate);
        Assert.Single(viewModel.Projects);
    }

    [Fact]
    public void AddProject_DuplicateByName_ReturnsFalseAndDoesNotAddDuplicate()
    {
        // Arrange
        var viewModel = CreateViewModel();
        _projectStore.AddProject(_testProjectPath);

        // Erstelle eine zweite Datei mit gleichem Namen in anderem Verzeichnis
        var duplicateDir = Path.Combine(Path.GetTempPath(), "Duplicate");
        Directory.CreateDirectory(duplicateDir);
        var duplicatePath = Path.Combine(duplicateDir, "TestProject1.csproj");
        File.WriteAllText(duplicatePath, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");

        try
        {
            // Act
            var addedDuplicate = _projectStore.AddProject(duplicatePath);

            // Assert
            Assert.False(addedDuplicate);
            Assert.Single(viewModel.Projects);
            
            // Ursprüngliches Projekt sollte beibehalten werden
            var project = viewModel.Projects.First();
            Assert.Equal(_testProjectPath, project.Path);
        }
        finally
        {
            File.Delete(duplicatePath);
            Directory.Delete(duplicateDir);
        }
    }

    [Fact]
    public void ContainsProject_ExistingProject_ReturnsTrue()
    {
        // Arrange
        var viewModel = CreateViewModel();
        _projectStore.AddProject(_testProjectPath);

        // Act
        var contains = _projectStore.ContainsProject("TestProject1");

        // Assert
        Assert.True(contains);
    }

    [Fact]
    public void ContainsProject_NonExistingProject_ReturnsFalse()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        var contains = _projectStore.ContainsProject("NonExistent");

        // Assert
        Assert.False(contains);
    }

    #endregion

    #region RemoveProjectCommand Tests

    [Fact]
    public void RemoveProjectCommand_CanExecute_ReturnsTrueWhenProjectSelected()
    {
        // Arrange
        var viewModel = CreateViewModel();
        _projectStore.AddProject(_testProjectPath);
        viewModel.SelectedProject = viewModel.Projects.First();

        // Act
        var canExecute = viewModel.RemoveProjectCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void RemoveProjectCommand_CanExecute_ReturnsFalseWhenNoProjectSelected()
    {
        // Arrange
        var viewModel = CreateViewModel();
        _projectStore.AddProject(_testProjectPath);
        viewModel.SelectedProject = null;

        // Act
        var canExecute = viewModel.RemoveProjectCommand.CanExecute(null);

        // Assert
        Assert.False(canExecute);
    }

    [Fact]
    public void RemoveProjectCommand_Execute_RemovesSelectedProject()
    {
        // Arrange
        var viewModel = CreateViewModel();
        _projectStore.AddProject(_testProjectPath);
        _projectStore.AddProject(_testProjectPath2);
        viewModel.SelectedProject = viewModel.Projects.First(p => p.Name == "TestProject1");

        // Act
        viewModel.RemoveProjectCommand.Execute(null);

        // Assert
        Assert.Single(viewModel.Projects);
        Assert.DoesNotContain(viewModel.Projects, p => p.Name == "TestProject1");
        Assert.Contains(viewModel.Projects, p => p.Name == "TestProject2");
    }

    [Fact]
    public void RemoveProject_ByName_RemovesProjectFromCollection()
    {
        // Arrange
        var viewModel = CreateViewModel();
        _projectStore.AddProject(_testProjectPath);
        _projectStore.AddProject(_testProjectPath2);

        // Act
        var removed = _projectStore.RemoveProject("TestProject1");

        // Assert
        Assert.True(removed);
        Assert.Single(viewModel.Projects);
        Assert.DoesNotContain(viewModel.Projects, p => p.Name == "TestProject1");
    }

    [Fact]
    public void RemoveProject_NonExistingProject_ReturnsFalse()
    {
        // Arrange
        var viewModel = CreateViewModel();
        _projectStore.AddProject(_testProjectPath);

        // Act
        var removed = _projectStore.RemoveProject("NonExistent");

        // Assert
        Assert.False(removed);
        Assert.Single(viewModel.Projects);
    }

    #endregion

    #region ClearProjectsCommand Tests

    [Fact]
    public void ClearProjectsCommand_CanExecute_ReturnsTrueWhenProjectsExist()
    {
        // Arrange
        var viewModel = CreateViewModel();
        _projectStore.AddProject(_testProjectPath);

        // Act
        var canExecute = viewModel.ClearProjectsCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void ClearProjectsCommand_CanExecute_ReturnsFalseWhenNoProjects()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        var canExecute = viewModel.ClearProjectsCommand.CanExecute(null);

        // Assert
        Assert.False(canExecute);
    }

    [Fact]
    public void Clear_RemovesAllProjects()
    {
        // Arrange
        var viewModel = CreateViewModel();
        _projectStore.AddProject(_testProjectPath);
        _projectStore.AddProject(_testProjectPath2);

        // Act
        _projectStore.Clear();

        // Assert
        Assert.Empty(viewModel.Projects);
    }

    #endregion

    #region ProjectInfo ViewModel Mapping Tests

    [Fact]
    public void ProjectInfoViewModel_PropertiesAreCorrectlyMapped()
    {
        // Arrange
        var viewModel = CreateViewModel();
        _projectStore.AddProject(_testProjectPath);

        // Act
        var projectViewModel = viewModel.Projects.First();

        // Assert
        Assert.Equal("TestProject1", projectViewModel.Name);
        Assert.Equal(_testProjectPath, projectViewModel.Path);
        Assert.True(projectViewModel.FileExists);
        Assert.NotEmpty(projectViewModel.StatusText);
    }

    [Fact]
    public void ProjectInfoViewModel_FileExists_ReturnsFalseForDeletedFile()
    {
        // Arrange
        var viewModel = CreateViewModel();
        _projectStore.AddProject(_testProjectPath);
        var projectViewModel = viewModel.Projects.First();
        
        // Act
        File.Delete(_testProjectPath);
        
        // Assert
        Assert.False(projectViewModel.FileExists);
    }

    #endregion

    #region Properties Tests

    [Fact]
    public void Projects_Property_ReturnsSameCollectionAsItems()
    {
        // Arrange
        var viewModel = CreateViewModel();
        _projectStore.AddProject(_testProjectPath);

        // Act & Assert
        Assert.Same(viewModel.Items, viewModel.Projects);
    }

    [Fact]
    public void SelectedProject_Property_CanBeSetAndRetrieved()
    {
        // Arrange
        var viewModel = CreateViewModel();
        _projectStore.AddProject(_testProjectPath);
        var project = viewModel.Projects.First();

        // Act
        viewModel.SelectedProject = project;

        // Assert
        Assert.Equal(project, viewModel.SelectedProject);
        Assert.Equal(project, viewModel.SelectedItem);
    }

    [Fact]
    public void AddProjectCommand_ReturnsAddCommand()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act & Assert
        Assert.Same(viewModel.AddCommand, viewModel.AddProjectCommand);
    }

    [Fact]
    public void ShowProjectDetailsCommand_ReturnsEditCommand()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act & Assert
        Assert.Same(viewModel.EditCommand, viewModel.ShowProjectDetailsCommand);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FullWorkflow_AddMultipleProjects_SelectAndRemove_VerifyCollectionState()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act 1: Add projects
        _projectStore.AddProject(_testProjectPath);
        _projectStore.AddProject(_testProjectPath2);
        Assert.Equal(2, viewModel.Projects.Count);

        // Act 2: Select and remove first project
        viewModel.SelectedProject = viewModel.Projects.First(p => p.Name == "TestProject1");
        viewModel.RemoveProjectCommand.Execute(null);
        Assert.Single(viewModel.Projects);

        // Act 3: Verify remaining project
        var remaining = viewModel.Projects.First();
        Assert.Equal("TestProject2", remaining.Name);
        Assert.True(remaining.FileExists);

        // Act 4: Clear all
        _projectStore.Clear();
        Assert.Empty(viewModel.Projects);
    }

    [Fact]
    public void DataStore_Synchronization_ProjectStoreAndViewModelStayInSync()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act & Assert: Verify synchronization through multiple operations
        Assert.Empty(viewModel.Projects);

        _projectStore.AddProject(_testProjectPath);
        Assert.Single(viewModel.Projects);
        Assert.Equal("TestProject1", viewModel.Projects.First().Name);

        _projectStore.AddProject(_testProjectPath2);
        Assert.Equal(2, viewModel.Projects.Count);

        _projectStore.RemoveProject("TestProject1");
        Assert.Single(viewModel.Projects);
        Assert.Equal("TestProject2", viewModel.Projects.First().Name);

        _projectStore.Clear();
        Assert.Empty(viewModel.Projects);
    }

    #endregion

    #region Helper Methods

    private ProjectListEditorViewModel CreateViewModel()
    {
        // Mock BundleOrchestrator für Tests
        var mockOrchestrator = new MockBundleOrchestrator();
        
        return new ProjectListEditorViewModel(
            _projectStore,
            _dataStoreProvider,
            _viewModelFactory,
            _comparer,
            mockOrchestrator);
    }

    #endregion
}

/// <summary>
/// EqualityComparer für ProjectInfo basierend auf dem Name-Property.
/// </summary>
internal class ProjectInfoEqualityComparer : IEqualityComparer<ProjectInfo>
{
    public bool Equals(ProjectInfo? x, ProjectInfo? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(ProjectInfo obj)
    {
        return obj?.Name?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0;
    }
}

/// <summary>
/// Test-Implementation eines IDataStoreProvider für isolierte Unit-Tests.
/// </summary>
internal class TestDataStoreProvider : IDataStoreProvider
{
    private readonly Dictionary<Type, object> _dataStores = new();

    public IDataStore<T> GetDataStore<T>() where T : class
    {
        if (!_dataStores.TryGetValue(typeof(T), out var store))
        {
            store = new InMemoryDataStore<T>();
            _dataStores[typeof(T)] = store;
        }
        return (IDataStore<T>)store;
    }

    public Task<IDataStore<T>> GetDataStoreAsync<T>() where T : class
    {
        return Task.FromResult(GetDataStore<T>());
    }

    public InMemoryDataStore<T> GetInMemory<T>(bool isSingleton = false, IEqualityComparer<T>? comparer = null) where T : class
    {
        if (!_dataStores.TryGetValue(typeof(T), out var store))
        {
            store = new InMemoryDataStore<T>();
            _dataStores[typeof(T)] = store;
        }
        return (InMemoryDataStore<T>)store;
    }

    public Task<InMemoryDataStore<T>> GetInMemoryAsync<T>(bool isSingleton = false, IEqualityComparer<T>? comparer = null) where T : class
    {
        return Task.FromResult((InMemoryDataStore<T>)GetDataStore<T>());
    }

    public PersistentDataStore<T> GetPersistent<T>(
        DataToolKit.Storage.Repositories.IRepositoryFactory repositoryFactory,
        bool isSingleton = false,
        bool trackPropertyChanges = false,
        bool autoLoad = false) where T : class
    {
        if (!_dataStores.TryGetValue(typeof(T), out var store))
        {
            store = new InMemoryDataStore<T>();
            _dataStores[typeof(T)] = store;
        }
        return (PersistentDataStore<T>)store;
    }

    public Task<PersistentDataStore<T>> GetPersistentAsync<T>(
        DataToolKit.Storage.Repositories.IRepositoryFactory repositoryFactory,
        bool isSingleton = false,
        bool trackPropertyChanges = false,
        bool autoLoad = false) where T : class
    {
        throw new NotImplementedException();
    }

    public bool RemoveSingleton<T>() where T : class
    {
        return _dataStores.Remove(typeof(T));
    }

    public void ClearAll()
    {
        _dataStores.Clear();
    }
}

/// <summary>
/// Mock-Implementation von IBundleOrchestrator für Tests.
/// </summary>
internal class MockBundleOrchestrator : IBundleOrchestrator
{
    public string Run(string rootPath, ScanSettings settings)
    {
        // Simuliere erfolgreichen Scan ohne echte Dateioperationen
        var outputFile = Path.Combine(rootPath, settings.OutputFileName);
        return outputFile;
    }
}
