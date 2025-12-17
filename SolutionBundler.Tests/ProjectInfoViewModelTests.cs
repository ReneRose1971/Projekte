using SolutionBundler.Core.Models;
using SolutionBundler.Core.Models.Persistence;
using SolutionBundler.WPF.ViewModels;
using Xunit;

namespace SolutionBundler.Tests;

public class ProjectInfoViewModelTests
{
    [Fact]
    public void Constructor_WithValidProjectInfo_CreatesViewModel()
    {
        // Arrange
        var projectInfo = new ProjectInfo { Path = @"C:\Test\MyProject.csproj" };

        // Act
        var viewModel = new ProjectInfoViewModel(projectInfo);

        // Assert
        Assert.NotNull(viewModel);
        Assert.Equal("MyProject", viewModel.Name);
        Assert.Equal(@"C:\Test\MyProject.csproj", viewModel.Path);
    }

    [Fact]
    public void FileExists_WhenFileDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var projectInfo = new ProjectInfo { Path = @"C:\NonExistent\Project.csproj" };
        var viewModel = new ProjectInfoViewModel(projectInfo);

        // Act
        var exists = viewModel.FileExists;

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public void StatusText_ContainsPathInformation()
    {
        // Arrange
        var projectPath = @"C:\Test\MyProject.csproj";
        var projectInfo = new ProjectInfo { Path = projectPath };
        var viewModel = new ProjectInfoViewModel(projectInfo);

        // Act
        var statusText = viewModel.StatusText;

        // Assert
        Assert.Contains(projectPath, statusText);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var projectInfo = new ProjectInfo { Path = @"C:\Test\MyProject.csproj" };
        var viewModel = new ProjectInfoViewModel(projectInfo);

        // Act
        var result = viewModel.ToString();

        // Assert
        Assert.Contains("MyProject", result);
        Assert.Contains(@"C:\Test\MyProject.csproj", result);
    }
}
