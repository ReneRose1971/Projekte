using SolutionBundler.Core.Models;
using SolutionBundler.Core.Models.Persistence;
using System;
using System.Collections.Generic;
using Xunit;

namespace SolutionBundler.Tests;

/// <summary>
/// Unit-Tests für ProjectInfo Domain-Model.
/// </summary>
public class ProjectInfoTests
{
    [Fact]
    public void Name_ExtractsProjectNameFromPath()
    {
        // Arrange
        var projectInfo = new ProjectInfo 
        { 
            Path = @"C:\Projects\MyApp\MyApp.Core.csproj" 
        };

        // Act
        var name = projectInfo.Name;

        // Assert
        Assert.Equal("MyApp.Core", name);
    }

    [Fact]
    public void Name_HandlesPathWithoutDirectory()
    {
        // Arrange
        var projectInfo = new ProjectInfo 
        { 
            Path = "MyApp.csproj" 
        };

        // Act
        var name = projectInfo.Name;

        // Assert
        Assert.Equal("MyApp", name);
    }

    [Fact]
    public void Name_HandlesEmptyPath()
    {
        // Arrange
        var projectInfo = new ProjectInfo { Path = string.Empty };

        // Act
        var name = projectInfo.Name;

        // Assert
        Assert.Equal(string.Empty, name);
    }

    [Fact]
    public void Equals_ReturnsTrueForSameName_CaseInsensitive()
    {
        // Arrange
        var project1 = new ProjectInfo { Path = @"C:\Temp\MyApp.csproj" };
        var project2 = new ProjectInfo { Path = @"D:\Other\myapp.csproj" };

        // Act
        var areEqual = project1.Equals(project2);

        // Assert
        Assert.True(areEqual);
    }

    [Fact]
    public void Equals_ReturnsFalseForDifferentName()
    {
        // Arrange
        var project1 = new ProjectInfo { Path = @"C:\Temp\MyApp.csproj" };
        var project2 = new ProjectInfo { Path = @"C:\Temp\OtherApp.csproj" };

        // Act
        var areEqual = project1.Equals(project2);

        // Assert
        Assert.False(areEqual);
    }

    [Fact]
    public void Equals_ReturnsTrueForSameReference()
    {
        // Arrange
        var project = new ProjectInfo { Path = @"C:\Temp\MyApp.csproj" };

        // Act
        var areEqual = project.Equals(project);

        // Assert
        Assert.True(areEqual);
    }

    [Fact]
    public void Equals_ReturnsFalseForNull()
    {
        // Arrange
        var project = new ProjectInfo { Path = @"C:\Temp\MyApp.csproj" };

        // Act
        var areEqual = project.Equals(null);

        // Assert
        Assert.False(areEqual);
    }

    [Fact]
    public void GetHashCode_IsSameForSameName_CaseInsensitive()
    {
        // Arrange
        var project1 = new ProjectInfo { Path = @"C:\Temp\MyApp.csproj" };
        var project2 = new ProjectInfo { Path = @"D:\Other\myapp.csproj" };

        // Act
        var hash1 = project1.GetHashCode();
        var hash2 = project2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_IsDifferentForDifferentName()
    {
        // Arrange
        var project1 = new ProjectInfo { Path = @"C:\Temp\MyApp.csproj" };
        var project2 = new ProjectInfo { Path = @"C:\Temp\OtherApp.csproj" };

        // Act
        var hash1 = project1.GetHashCode();
        var hash2 = project2.GetHashCode();

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void ToString_ReturnsProjectName()
    {
        // Arrange
        var projectInfo = new ProjectInfo { Path = @"C:\Temp\MyApp.Core.csproj" };

        // Act
        var result = projectInfo.ToString();

        // Assert
        Assert.Equal("MyApp.Core", result);
    }

    [Fact]
    public void CanBeUsedInHashSet()
    {
        // Arrange
        var projects = new HashSet<ProjectInfo>();
        var project1 = new ProjectInfo { Path = @"C:\Temp\MyApp.csproj" };
        var project2 = new ProjectInfo { Path = @"D:\Other\myapp.csproj" }; // Same name, case-insensitive

        // Act
        var added1 = projects.Add(project1);
        var added2 = projects.Add(project2);

        // Assert
        Assert.True(added1);
        Assert.False(added2); // Should not be added (duplicate)
        Assert.Single(projects);
    }
}
