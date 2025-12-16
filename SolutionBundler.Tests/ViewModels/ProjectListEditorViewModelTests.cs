using DataToolKit.Tests.Fakes.Providers;
using SolutionBundler.Core.Abstractions;
using SolutionBundler.Core.Models;
using SolutionBundler.Core.Storage;
using SolutionBundler.WPF.ViewModels;
using Xunit;

namespace SolutionBundler.Tests.ViewModels;

public class ProjectListEditorViewModelTests : IDisposable
{
    private readonly FakeDataStoreProvider _fakeProvider;
    private readonly ProjectStore _projectStore;
    private readonly ProjectListEditorViewModel _viewModel;
    private readonly string _testProjectPath;

    public ProjectListEditorViewModelTests()
    {
        _fakeProvider = new FakeDataStoreProvider();
        
        // Setup PersistentDataStore für ProjectInfo
        _fakeProvider.GetPersistent<ProjectInfo>(
            _fakeProvider.RepositoryFactory,
            isSingleton: true,
            trackPropertyChanges: false,
            autoLoad: false);
        
        _projectStore = new ProjectStore(_fakeProvider);
        
        // Mock BundleOrchestrator
        var mockOrchestrator = new MockBundleOrchestrator();
        
        // Erstelle ViewModel mit Fake-Dependencies
        _viewModel = new ProjectListEditorViewModel(
            _projectStore,
            _fakeProvider,
            new ProjectInfoViewModelFactory(),
            EqualityComparer<ProjectInfo>.Default,
            mockOrchestrator);
        
        // Erstelle temporäre Test-Projektdatei
        _testProjectPath = Path.Combine(Path.GetTempPath(), $"TestProject_{Guid.NewGuid()}.csproj");
        File.WriteAllText(_testProjectPath, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");
    }

    public void Dispose()
    {
        if (File.Exists(_testProjectPath))
            File.Delete(_testProjectPath);
        
        _fakeProvider.ClearAll();
    }

    [Fact]
    public void Constructor_ShouldInitializeWithEmptyProjects()
    {
        // Assert
        Assert.NotNull(_viewModel.Projects);
        Assert.Empty(_viewModel.Projects);
    }

    [Fact]
    public void Projects_ShouldBeReadOnly()
    {
        // Assert
        Assert.IsAssignableFrom<System.Collections.ObjectModel.ReadOnlyObservableCollection<ProjectInfoViewModel>>(_viewModel.Projects);
    }

    [Fact]
    public void SelectedProject_ShouldGetAndSetCorrectly()
    {
        // Arrange
        _projectStore.AddProject(_testProjectPath);
        var projectViewModel = _viewModel.Projects[0];

        // Act
        _viewModel.SelectedProject = projectViewModel;

        // Assert
        Assert.Equal(projectViewModel, _viewModel.SelectedProject);
    }

    [Fact]
    public void AddProjectCommand_ShouldBeAvailable()
    {
        // Assert
        Assert.NotNull(_viewModel.AddProjectCommand);
    }

    [Fact]
    public void RemoveProjectCommand_WhenNoSelection_CannotExecute()
    {
        // Arrange
        _viewModel.SelectedProject = null;

        // Act
        var canExecute = _viewModel.RemoveProjectCommand.CanExecute(null);

        // Assert
        Assert.False(canExecute);
    }

    [Fact]
    public void RemoveProjectCommand_WhenProjectSelected_CanExecute()
    {
        // Arrange
        _projectStore.AddProject(_testProjectPath);
        _viewModel.SelectedProject = _viewModel.Projects[0];

        // Act
        var canExecute = _viewModel.RemoveProjectCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void RemoveProjectCommand_ShouldRemoveSelectedProject()
    {
        // Arrange
        _projectStore.AddProject(_testProjectPath);
        _viewModel.SelectedProject = _viewModel.Projects[0];
        Assert.Single(_viewModel.Projects);

        // Act
        _viewModel.RemoveProjectCommand.Execute(null);

        // Assert
        Assert.Empty(_viewModel.Projects);
    }

    [Fact]
    public void ClearProjectsCommand_WhenProjectsExist_CanExecute()
    {
        // Arrange
        _projectStore.AddProject(_testProjectPath);

        // Act
        var canExecute = _viewModel.ClearProjectsCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void ClearProjectsCommand_WhenNoProjects_CannotExecute()
    {
        // Act
        var canExecute = _viewModel.ClearProjectsCommand.CanExecute(null);

        // Assert
        Assert.False(canExecute);
    }

    [Fact]
    public void Projects_ShouldReflectDataStoreChanges()
    {
        // Arrange
        Assert.Empty(_viewModel.Projects);

        // Act - Add project via ProjectStore
        _projectStore.AddProject(_testProjectPath);

        // Assert - ViewModel collection should be updated
        Assert.Single(_viewModel.Projects);
        Assert.Equal(Path.GetFileNameWithoutExtension(_testProjectPath), _viewModel.Projects[0].Name);
    }

    [Fact]
    public void Projects_ShouldUpdateWhenProjectStoreChanges()
    {
        // Arrange
        var secondProjectPath = Path.Combine(Path.GetTempPath(), $"TestProject2_{Guid.NewGuid()}.csproj");
        File.WriteAllText(secondProjectPath, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");

        try
        {
            // Act - Add multiple projects
            _projectStore.AddProject(_testProjectPath);
            _projectStore.AddProject(secondProjectPath);

            // Assert
            Assert.Equal(2, _viewModel.Projects.Count);

            // Act - Remove one project
            _projectStore.RemoveProject(Path.GetFileNameWithoutExtension(_testProjectPath));

            // Assert
            Assert.Single(_viewModel.Projects);
            Assert.Equal(Path.GetFileNameWithoutExtension(secondProjectPath), _viewModel.Projects[0].Name);
        }
        finally
        {
            if (File.Exists(secondProjectPath))
                File.Delete(secondProjectPath);
        }
    }

    [Fact]
    public void ShowProjectDetailsCommand_ShouldBeAvailable()
    {
        // Assert
        Assert.NotNull(_viewModel.ShowProjectDetailsCommand);
    }

    // Helper: Simple ViewModelFactory for tests
    private class ProjectInfoViewModelFactory : CustomWPFControls.Factories.IViewModelFactory<ProjectInfo, ProjectInfoViewModel>
    {
        public ProjectInfoViewModel Create(ProjectInfo model)
        {
            return new ProjectInfoViewModel(model);
        }
    }
    
    // Helper: Mock BundleOrchestrator for tests
    private class MockBundleOrchestrator : IBundleOrchestrator
    {
        public string Run(string rootPath, ScanSettings settings)
        {
            // Simuliere erfolgreichen Scan ohne echte Dateioperationen
            var outputFile = Path.Combine(rootPath, settings.OutputFileName);
            return outputFile;
        }
    }
}
