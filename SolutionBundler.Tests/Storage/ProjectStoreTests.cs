using DataToolKit.Tests.Fakes.Providers;
using SolutionBundler.Core.Models;
using SolutionBundler.Core.Storage;
using System;
using System.IO;
using Xunit;

namespace SolutionBundler.Tests.Storage;

public class ProjectStoreTests : IDisposable
{
    private readonly FakeDataStoreProvider _fakeProvider;
    private readonly ProjectStore _store;
    private readonly string _testProjectPath;

    public ProjectStoreTests()
    {
        _fakeProvider = new FakeDataStoreProvider();
        
        // ProjectInfo ist ein POCO ? JSON-Repository (kein IEntity!)
        _fakeProvider.GetPersistent<ProjectInfo>(
            _fakeProvider.RepositoryFactory,
            isSingleton: true,
            trackPropertyChanges: false,
            autoLoad: false);
        
        _store = new ProjectStore(_fakeProvider);
        
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
    public void AddProject_WithValidPath_ShouldAddProject()
    {
        // Act
        var result = _store.AddProject(_testProjectPath);

        // Assert
        Assert.True(result);
        Assert.Single(_store.Projects);
        Assert.Equal(Path.GetFileNameWithoutExtension(_testProjectPath), _store.Projects[0].Name);
    }

    [Fact]
    public void AddProject_WithDuplicatePath_ShouldReturnFalse()
    {
        // Arrange
        _store.AddProject(_testProjectPath);

        // Act
        var result = _store.AddProject(_testProjectPath);

        // Assert
        Assert.False(result);
        Assert.Single(_store.Projects);
    }

    [Fact]
    public void AddProject_WithNullPath_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _store.AddProject(null!));
    }

    [Fact]
    public void AddProject_WithEmptyPath_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _store.AddProject(string.Empty));
    }

    [Fact]
    public void AddProject_WithNonExistentPath_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = @"C:\NonExistent\Project.csproj";

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => _store.AddProject(nonExistentPath));
    }

    [Fact]
    public void AddProject_WithNonCsprojFile_ShouldThrowArgumentException()
    {
        // Arrange
        var txtFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.txt");
        File.WriteAllText(txtFile, "test");

        try
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _store.AddProject(txtFile));
        }
        finally
        {
            File.Delete(txtFile);
        }
    }

    [Fact]
    public void RemoveProject_WithExistingProject_ShouldRemoveProject()
    {
        // Arrange
        _store.AddProject(_testProjectPath);
        var projectName = Path.GetFileNameWithoutExtension(_testProjectPath);

        // Act
        var result = _store.RemoveProject(projectName);

        // Assert
        Assert.True(result);
        Assert.Empty(_store.Projects);
    }

    [Fact]
    public void RemoveProject_WithNonExistentProject_ShouldReturnFalse()
    {
        // Act
        var result = _store.RemoveProject("NonExistentProject");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RemoveProject_WithNullName_ShouldReturnFalse()
    {
        // Act
        var result = _store.RemoveProject(null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RemoveProject_WithEmptyName_ShouldReturnFalse()
    {
        // Act
        var result = _store.RemoveProject(string.Empty);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ContainsProject_WithExistingProject_ShouldReturnTrue()
    {
        // Arrange
        _store.AddProject(_testProjectPath);
        var projectName = Path.GetFileNameWithoutExtension(_testProjectPath);

        // Act
        var result = _store.ContainsProject(projectName);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ContainsProject_WithNonExistentProject_ShouldReturnFalse()
    {
        // Act
        var result = _store.ContainsProject("NonExistentProject");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ContainsProject_IsCaseInsensitive()
    {
        // Arrange
        _store.AddProject(_testProjectPath);
        var projectName = Path.GetFileNameWithoutExtension(_testProjectPath);

        // Act
        var result = _store.ContainsProject(projectName.ToUpper());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Clear_ShouldRemoveAllProjects()
    {
        // Arrange
        _store.AddProject(_testProjectPath);

        // Create second test project
        var secondProjectPath = Path.Combine(Path.GetTempPath(), $"TestProject2_{Guid.NewGuid()}.csproj");
        File.WriteAllText(secondProjectPath, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");

        try
        {
            _store.AddProject(secondProjectPath);
            Assert.Equal(2, _store.Projects.Count);

            // Act
            _store.Clear();

            // Assert
            Assert.Empty(_store.Projects);
        }
        finally
        {
            if (File.Exists(secondProjectPath))
                File.Delete(secondProjectPath);
        }
    }

    [Fact]
    public void Projects_ShouldBeReadOnly()
    {
        // Arrange
        _store.AddProject(_testProjectPath);

        // Act
        var projects = _store.Projects;

        // Assert
        Assert.IsAssignableFrom<System.Collections.ObjectModel.ReadOnlyObservableCollection<ProjectInfo>>(projects);
    }

    [Fact]
    public void AddProject_WithFullPath_ShouldNormalizePath()
    {
        // Act
        _store.AddProject(_testProjectPath);

        // Assert
        var addedProject = _store.Projects[0];
        Assert.True(Path.IsPathFullyQualified(addedProject.Path));
    }

    [Fact]
    public void AddMultipleProjects_ShouldMaintainCollection()
    {
        // Arrange
        var secondProjectPath = Path.Combine(Path.GetTempPath(), $"TestProject2_{Guid.NewGuid()}.csproj");
        var thirdProjectPath = Path.Combine(Path.GetTempPath(), $"TestProject3_{Guid.NewGuid()}.csproj");
        
        File.WriteAllText(secondProjectPath, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");
        File.WriteAllText(thirdProjectPath, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");

        try
        {
            // Act
            _store.AddProject(_testProjectPath);
            _store.AddProject(secondProjectPath);
            _store.AddProject(thirdProjectPath);

            // Assert
            Assert.Equal(3, _store.Projects.Count);
            Assert.Contains(_store.Projects, p => p.Name == Path.GetFileNameWithoutExtension(_testProjectPath));
            Assert.Contains(_store.Projects, p => p.Name == Path.GetFileNameWithoutExtension(secondProjectPath));
            Assert.Contains(_store.Projects, p => p.Name == Path.GetFileNameWithoutExtension(thirdProjectPath));
        }
        finally
        {
            if (File.Exists(secondProjectPath))
                File.Delete(secondProjectPath);
            if (File.Exists(thirdProjectPath))
                File.Delete(thirdProjectPath);
        }
    }
}
