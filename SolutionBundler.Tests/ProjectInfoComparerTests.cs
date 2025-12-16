using SolutionBundler.Core.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace SolutionBundler.Tests;

/// <summary>
/// Unit-Tests für ProjectInfoComparer.
/// </summary>
public class ProjectInfoComparerTests
{
    private readonly ProjectInfoComparer _comparer = new();

    [Fact]
    public void Equals_ReturnsTrueForSameName_CaseInsensitive()
    {
        // Arrange
        var project1 = new ProjectInfo { Path = @"C:\Temp\MyApp.csproj" };
        var project2 = new ProjectInfo { Path = @"D:\Other\myapp.csproj" };

        // Act
        var result = _comparer.Equals(project1, project2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_ReturnsFalseForDifferentName()
    {
        // Arrange
        var project1 = new ProjectInfo { Path = @"C:\Temp\MyApp.csproj" };
        var project2 = new ProjectInfo { Path = @"C:\Temp\OtherApp.csproj" };

        // Act
        var result = _comparer.Equals(project1, project2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_ReturnsTrueForSameReference()
    {
        // Arrange
        var project = new ProjectInfo { Path = @"C:\Temp\MyApp.csproj" };

        // Act
        var result = _comparer.Equals(project, project);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_ReturnsTrueForBothNull()
    {
        // Act
        var result = _comparer.Equals(null, null);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_ReturnsFalseWhenFirstIsNull()
    {
        // Arrange
        var project = new ProjectInfo { Path = @"C:\Temp\MyApp.csproj" };

        // Act
        var result = _comparer.Equals(null, project);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_ReturnsFalseWhenSecondIsNull()
    {
        // Arrange
        var project = new ProjectInfo { Path = @"C:\Temp\MyApp.csproj" };

        // Act
        var result = _comparer.Equals(project, null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_IsSameForSameName_CaseInsensitive()
    {
        // Arrange
        var project1 = new ProjectInfo { Path = @"C:\Temp\MyApp.csproj" };
        var project2 = new ProjectInfo { Path = @"D:\Other\myapp.csproj" };

        // Act
        var hash1 = _comparer.GetHashCode(project1);
        var hash2 = _comparer.GetHashCode(project2);

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
        var hash1 = _comparer.GetHashCode(project1);
        var hash2 = _comparer.GetHashCode(project2);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_ThrowsForNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _comparer.GetHashCode(null!));
    }

    [Fact]
    public void CanBeUsedWithDictionary()
    {
        // Arrange
        var dictionary = new Dictionary<ProjectInfo, string>(_comparer);
        var project1 = new ProjectInfo { Path = @"C:\Temp\MyApp.csproj" };
        var project2 = new ProjectInfo { Path = @"D:\Other\myapp.csproj" }; // Same name

        // Act
        dictionary[project1] = "Value1";
        dictionary[project2] = "Value2"; // Should overwrite

        // Assert
        Assert.Single(dictionary);
        Assert.Equal("Value2", dictionary[project1]);
    }

    [Fact]
    public void CanBeUsedWithHashSet()
    {
        // Arrange
        var projects = new HashSet<ProjectInfo>(_comparer);
        var project1 = new ProjectInfo { Path = @"C:\Temp\MyApp.csproj" };
        var project2 = new ProjectInfo { Path = @"D:\Other\myapp.csproj" }; // Same name

        // Act
        var added1 = projects.Add(project1);
        var added2 = projects.Add(project2);

        // Assert
        Assert.True(added1);
        Assert.False(added2); // Duplicate
        Assert.Single(projects);
    }
}
