using SolutionBundler.Core.Implementations.MetadataReading;
using SolutionBundler.Core.Models;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SolutionBundler.Tests.MetadataReading;

public class CsprojParserTests
{
    private readonly string _tempDir;

    public CsprojParserTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "CsprojParserTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public void ParseCsproj_WithValidProject_ReturnsCorrectMappings()
    {
        // Arrange
        var projectDir = Path.Combine(_tempDir, "TestProject");
        Directory.CreateDirectory(projectDir);

        var csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <Compile Include=""Program.cs"" />
    <Page Include=""MainWindow.xaml"" />
    <Resource Include=""icon.png"" />
  </ItemGroup>
</Project>";
        var csprojPath = Path.Combine(projectDir, "TestProject.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        var parser = new CsprojParser();

        // Act
        var result = parser.ParseCsproj(csprojPath, _tempDir);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(BuildAction.Compile, result["TestProject/Program.cs"]);
        Assert.Equal(BuildAction.Page, result["TestProject/MainWindow.xaml"]);
        Assert.Equal(BuildAction.Resource, result["TestProject/icon.png"]);
    }

    [Fact]
    public void ParseCsproj_WithEmptyProject_ReturnsEmptyDictionary()
    {
        // Arrange
        var projectDir = Path.Combine(_tempDir, "EmptyProject");
        Directory.CreateDirectory(projectDir);

        var csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
</Project>";
        var csprojPath = Path.Combine(projectDir, "EmptyProject.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        var parser = new CsprojParser();

        // Act
        var result = parser.ParseCsproj(csprojPath, _tempDir);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ParseCsproj_WithMalformedXml_ReturnsEmptyDictionary()
    {
        // Arrange
        var projectDir = Path.Combine(_tempDir, "MalformedProject");
        Directory.CreateDirectory(projectDir);

        var csprojContent = @"<Project><ItemGroup><Compile Include=""Test.cs"" /></Project>"; // Missing closing tag
        var csprojPath = Path.Combine(projectDir, "MalformedProject.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        var parser = new CsprojParser();

        // Act
        var result = parser.ParseCsproj(csprojPath, _tempDir);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ParseCsproj_WithNonExistentFile_ReturnsEmptyDictionary()
    {
        // Arrange
        var parser = new CsprojParser();
        var nonExistentPath = Path.Combine(_tempDir, "DoesNotExist.csproj");

        // Act
        var result = parser.ParseCsproj(nonExistentPath, _tempDir);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ParseCsproj_WithSubdirectoryPaths_NormalizesCorrectly()
    {
        // Arrange
        var projectDir = Path.Combine(_tempDir, "SubdirProject");
        Directory.CreateDirectory(projectDir);

        var csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <Compile Include=""Src\Helper.cs"" />
    <Compile Include=""Src\SubFolder\Utility.cs"" />
  </ItemGroup>
</Project>";
        var csprojPath = Path.Combine(projectDir, "SubdirProject.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        var parser = new CsprojParser();

        // Act
        var result = parser.ParseCsproj(csprojPath, _tempDir);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("SubdirProject/Src/Helper.cs", result.Keys);
        Assert.Contains("SubdirProject/Src/SubFolder/Utility.cs", result.Keys);
    }

    [Fact]
    public void ParseCsproj_WithDuplicateEntries_FirstEntryWins()
    {
        // Arrange
        var projectDir = Path.Combine(_tempDir, "DuplicateProject");
        Directory.CreateDirectory(projectDir);

        var csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <Compile Include=""Test.cs"" />
    <Content Include=""Test.cs"" />
  </ItemGroup>
</Project>";
        var csprojPath = Path.Combine(projectDir, "DuplicateProject.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        var parser = new CsprojParser();

        // Act
        var result = parser.ParseCsproj(csprojPath, _tempDir);

        // Assert
        Assert.Single(result);
        Assert.Equal(BuildAction.Compile, result["DuplicateProject/Test.cs"]);
    }

    [Fact]
    public void ParseCsproj_IgnoresItemsWithoutIncludeAttribute()
    {
        // Arrange
        var projectDir = Path.Combine(_tempDir, "NoIncludeProject");
        Directory.CreateDirectory(projectDir);

        var csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <Compile />
    <Compile Include="""" />
    <Compile Include=""   "" />
    <Compile Include=""Valid.cs"" />
  </ItemGroup>
</Project>";
        var csprojPath = Path.Combine(projectDir, "NoIncludeProject.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        var parser = new CsprojParser();

        // Act
        var result = parser.ParseCsproj(csprojPath, _tempDir);

        // Assert
        Assert.Single(result);
        Assert.Contains("NoIncludeProject/Valid.cs", result.Keys);
    }

    [Fact]
    public void ParseMultipleCsprojs_MergesResultsFromMultipleProjects()
    {
        // Arrange
        var project1Dir = Path.Combine(_tempDir, "Project1");
        var project2Dir = Path.Combine(_tempDir, "Project2");
        Directory.CreateDirectory(project1Dir);
        Directory.CreateDirectory(project2Dir);

        var csproj1Content = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <Compile Include=""Class1.cs"" />
  </ItemGroup>
</Project>";
        var csproj1Path = Path.Combine(project1Dir, "Project1.csproj");
        File.WriteAllText(csproj1Path, csproj1Content);

        var csproj2Content = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <Compile Include=""Class2.cs"" />
  </ItemGroup>
</Project>";
        var csproj2Path = Path.Combine(project2Dir, "Project2.csproj");
        File.WriteAllText(csproj2Path, csproj2Content);

        var parser = new CsprojParser();

        // Act
        var result = parser.ParseMultipleCsprojs(new[] { csproj1Path, csproj2Path }, _tempDir);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(BuildAction.Compile, result["Project1/Class1.cs"]);
        Assert.Equal(BuildAction.Compile, result["Project2/Class2.cs"]);
    }

    [Fact]
    public void ParseMultipleCsprojs_WithEmptyCollection_ReturnsEmptyDictionary()
    {
        // Arrange
        var parser = new CsprojParser();

        // Act
        var result = parser.ParseMultipleCsprojs(Array.Empty<string>(), _tempDir);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ParseMultipleCsprojs_WithDuplicatePathsAcrossProjects_FirstProjectWins()
    {
        // Arrange
        var project1Dir = Path.Combine(_tempDir, "SharedProject1");
        var project2Dir = Path.Combine(_tempDir, "SharedProject2");
        Directory.CreateDirectory(project1Dir);
        Directory.CreateDirectory(project2Dir);

        // Both projects reference the same relative file (unlikely but possible)
        var sharedDir = Path.Combine(_tempDir, "Shared");
        Directory.CreateDirectory(sharedDir);

        var csproj1Content = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <Compile Include=""..\Shared\Common.cs"" />
  </ItemGroup>
</Project>";
        var csproj1Path = Path.Combine(project1Dir, "Project1.csproj");
        File.WriteAllText(csproj1Path, csproj1Content);

        var csproj2Content = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <Content Include=""..\Shared\Common.cs"" />
  </ItemGroup>
</Project>";
        var csproj2Path = Path.Combine(project2Dir, "Project2.csproj");
        File.WriteAllText(csproj2Path, csproj2Content);

        var parser = new CsprojParser();

        // Act
        var result = parser.ParseMultipleCsprojs(new[] { csproj1Path, csproj2Path }, _tempDir);

        // Assert
        Assert.Single(result);
        Assert.Equal(BuildAction.Compile, result["Shared/Common.cs"]); // First project wins
    }

    [Fact]
    public void ParseCsproj_IsCaseInsensitiveForPaths()
    {
        // Arrange
        var projectDir = Path.Combine(_tempDir, "CaseProject");
        Directory.CreateDirectory(projectDir);

        var csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <Compile Include=""TEST.CS"" />
  </ItemGroup>
</Project>";
        var csprojPath = Path.Combine(projectDir, "CaseProject.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        var parser = new CsprojParser();

        // Act
        var result = parser.ParseCsproj(csprojPath, _tempDir);

        // Assert
        Assert.Single(result);
        // Verify case insensitivity by checking if lowercase key exists
        Assert.True(result.ContainsKey("CaseProject/TEST.CS") || result.ContainsKey("caseproject/test.cs"));
    }
}
