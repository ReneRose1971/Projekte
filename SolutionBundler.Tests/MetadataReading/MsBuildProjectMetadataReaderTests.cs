using SolutionBundler.Core.Implementations;
using SolutionBundler.Core.Models;
using Xunit;

namespace SolutionBundler.Tests;

public class MsBuildProjectMetadataReaderTests
{
    private readonly string _tempDir;

    public MsBuildProjectMetadataReaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "MsBuildTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public void EnrichBuildActions_Sets_Action_For_CS_Fallback()
    {
        var entries = new List<FileEntry>
        {
            new() { RelativePath = "src/Program.cs", FullPath = "src/Program.cs", Size = 0 },
            new() { RelativePath = "views/MainWindow.xaml", FullPath = "views/MainWindow.xaml", Size = 0 }
        };

        var reader = new MsBuildProjectMetadataReader();
        reader.EnrichBuildActions(entries, Directory.GetCurrentDirectory());

        Assert.Equal(BuildAction.Compile, entries.First(e => e.RelativePath.EndsWith("Program.cs")).Action);
        Assert.Equal(BuildAction.Page, entries.First(e => e.RelativePath.EndsWith("MainWindow.xaml")).Action);
    }

    [Fact]
    public void EnrichBuildActions_WithEmptyList_DoesNotThrow()
    {
        var entries = new List<FileEntry>();
        var reader = new MsBuildProjectMetadataReader();

        var exception = Record.Exception(() => reader.EnrichBuildActions(entries, _tempDir));

        Assert.Null(exception);
    }

    [Fact]
    public void EnrichBuildActions_ParsesCsprojFile_SetsCompileAction()
    {
        // Arrange
        var projectDir = Path.Combine(_tempDir, "TestProject");
        Directory.CreateDirectory(projectDir);

        var csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include=""Program.cs"" />
  </ItemGroup>
</Project>";
        var csprojPath = Path.Combine(projectDir, "TestProject.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        var programPath = Path.Combine(projectDir, "Program.cs");
        File.WriteAllText(programPath, "// test");

        var entries = new List<FileEntry>
        {
            new() { RelativePath = "TestProject/TestProject.csproj", FullPath = csprojPath, Size = 0 },
            new() { RelativePath = "TestProject/Program.cs", FullPath = programPath, Size = 0 }
        };

        var reader = new MsBuildProjectMetadataReader();

        // Act
        reader.EnrichBuildActions(entries, _tempDir);

        // Assert
        var programEntry = entries.First(e => e.RelativePath.EndsWith("Program.cs"));
        Assert.Equal(BuildAction.Compile, programEntry.Action);
    }

    [Fact]
    public void EnrichBuildActions_ParsesCsprojFile_SetsPageAction()
    {
        // Arrange
        var projectDir = Path.Combine(_tempDir, "WpfProject");
        Directory.CreateDirectory(projectDir);

        var csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <Page Include=""MainWindow.xaml"" />
  </ItemGroup>
</Project>";
        var csprojPath = Path.Combine(projectDir, "WpfProject.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        var xamlPath = Path.Combine(projectDir, "MainWindow.xaml");
        File.WriteAllText(xamlPath, "<Window />");

        var entries = new List<FileEntry>
        {
            new() { RelativePath = "WpfProject/WpfProject.csproj", FullPath = csprojPath, Size = 0 },
            new() { RelativePath = "WpfProject/MainWindow.xaml", FullPath = xamlPath, Size = 0 }
        };

        var reader = new MsBuildProjectMetadataReader();

        // Act
        reader.EnrichBuildActions(entries, _tempDir);

        // Assert
        var xamlEntry = entries.First(e => e.RelativePath.EndsWith("MainWindow.xaml"));
        Assert.Equal(BuildAction.Page, xamlEntry.Action);
    }

    [Fact]
    public void EnrichBuildActions_ParsesCsprojFile_SetsResourceAction()
    {
        // Arrange
        var projectDir = Path.Combine(_tempDir, "ResourceProject");
        Directory.CreateDirectory(projectDir);

        var csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <Resource Include=""icon.png"" />
  </ItemGroup>
</Project>";
        var csprojPath = Path.Combine(projectDir, "ResourceProject.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        var iconPath = Path.Combine(projectDir, "icon.png");
        File.WriteAllText(iconPath, "fake png");

        var entries = new List<FileEntry>
        {
            new() { RelativePath = "ResourceProject/ResourceProject.csproj", FullPath = csprojPath, Size = 0 },
            new() { RelativePath = "ResourceProject/icon.png", FullPath = iconPath, Size = 0 }
        };

        var reader = new MsBuildProjectMetadataReader();

        // Act
        reader.EnrichBuildActions(entries, _tempDir);

        // Assert
        var iconEntry = entries.First(e => e.RelativePath.EndsWith("icon.png"));
        Assert.Equal(BuildAction.Resource, iconEntry.Action);
    }

    [Fact]
    public void EnrichBuildActions_ParsesCsprojFile_SetsContentAction()
    {
        // Arrange
        var projectDir = Path.Combine(_tempDir, "ContentProject");
        Directory.CreateDirectory(projectDir);

        var csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <Content Include=""appsettings.json"" />
  </ItemGroup>
</Project>";
        var csprojPath = Path.Combine(projectDir, "ContentProject.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        var jsonPath = Path.Combine(projectDir, "appsettings.json");
        File.WriteAllText(jsonPath, "{}");

        var entries = new List<FileEntry>
        {
            new() { RelativePath = "ContentProject/ContentProject.csproj", FullPath = csprojPath, Size = 0 },
            new() { RelativePath = "ContentProject/appsettings.json", FullPath = jsonPath, Size = 0 }
        };

        var reader = new MsBuildProjectMetadataReader();

        // Act
        reader.EnrichBuildActions(entries, _tempDir);

        // Assert
        var jsonEntry = entries.First(e => e.RelativePath.EndsWith("appsettings.json"));
        Assert.Equal(BuildAction.Content, jsonEntry.Action);
    }

    [Fact]
    public void EnrichBuildActions_WithMalformedCsproj_ContinuesGracefully()
    {
        // Arrange
        var projectDir = Path.Combine(_tempDir, "BrokenProject");
        Directory.CreateDirectory(projectDir);

        var csprojContent = @"<Project><ItemGroup><Compile Include=""Program.cs"" /></Project>"; // Missing closing tag
        var csprojPath = Path.Combine(projectDir, "BrokenProject.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        var programPath = Path.Combine(projectDir, "Program.cs");
        File.WriteAllText(programPath, "// test");

        var entries = new List<FileEntry>
        {
            new() { RelativePath = "BrokenProject/BrokenProject.csproj", FullPath = csprojPath, Size = 0 },
            new() { RelativePath = "BrokenProject/Program.cs", FullPath = programPath, Size = 0 }
        };

        var reader = new MsBuildProjectMetadataReader();

        // Act
        var exception = Record.Exception(() => reader.EnrichBuildActions(entries, _tempDir));

        // Assert
        Assert.Null(exception);
        var programEntry = entries.First(e => e.RelativePath.EndsWith("Program.cs"));
        Assert.Equal(BuildAction.Compile, programEntry.Action); // Fallback should apply
    }

    [Fact]
    public void EnrichBuildActions_FallbackLogic_SetsCorrectActions()
    {
        // Arrange
        var entries = new List<FileEntry>
        {
            new() { RelativePath = "Test.cs", FullPath = "Test.cs", Size = 0 },
            new() { RelativePath = "Main.xaml", FullPath = "Main.xaml", Size = 0 },
            new() { RelativePath = "Strings.resx", FullPath = "Strings.resx", Size = 0 },
            new() { RelativePath = "app.config", FullPath = "app.config", Size = 0 },
            new() { RelativePath = "settings.json", FullPath = "settings.json", Size = 0 },
            new() { RelativePath = "build.props", FullPath = "build.props", Size = 0 },
            new() { RelativePath = "build.targets", FullPath = "build.targets", Size = 0 },
            new() { RelativePath = "README.md", FullPath = "README.md", Size = 0 }
        };

        var reader = new MsBuildProjectMetadataReader();

        // Act
        reader.EnrichBuildActions(entries, _tempDir);

        // Assert
        Assert.Equal(BuildAction.Compile, entries.First(e => e.RelativePath == "Test.cs").Action);
        Assert.Equal(BuildAction.Page, entries.First(e => e.RelativePath == "Main.xaml").Action);
        Assert.Equal(BuildAction.Resource, entries.First(e => e.RelativePath == "Strings.resx").Action);
        Assert.Equal(BuildAction.Content, entries.First(e => e.RelativePath == "app.config").Action);
        Assert.Equal(BuildAction.Content, entries.First(e => e.RelativePath == "settings.json").Action);
        Assert.Equal(BuildAction.Content, entries.First(e => e.RelativePath == "build.props").Action);
        Assert.Equal(BuildAction.Content, entries.First(e => e.RelativePath == "build.targets").Action);
        Assert.Equal(BuildAction.Unknown, entries.First(e => e.RelativePath == "README.md").Action);
    }

    [Fact]
    public void EnrichBuildActions_CaseInsensitiveExtensions_WorksCorrectly()
    {
        // Arrange
        var entries = new List<FileEntry>
        {
            new() { RelativePath = "Test.CS", FullPath = "Test.CS", Size = 0 },
            new() { RelativePath = "Main.XAML", FullPath = "Main.XAML", Size = 0 },
            new() { RelativePath = "Strings.RESX", FullPath = "Strings.RESX", Size = 0 },
            new() { RelativePath = "app.CONFIG", FullPath = "app.CONFIG", Size = 0 }
        };

        var reader = new MsBuildProjectMetadataReader();

        // Act
        reader.EnrichBuildActions(entries, _tempDir);

        // Assert
        Assert.Equal(BuildAction.Compile, entries.First(e => e.RelativePath == "Test.CS").Action);
        Assert.Equal(BuildAction.Page, entries.First(e => e.RelativePath == "Main.XAML").Action);
        Assert.Equal(BuildAction.Resource, entries.First(e => e.RelativePath == "Strings.RESX").Action);
        Assert.Equal(BuildAction.Content, entries.First(e => e.RelativePath == "app.CONFIG").Action);
    }

    [Fact]
    public void EnrichBuildActions_WithMultipleCsprojFiles_ParsesAll()
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

        var class1Path = Path.Combine(project1Dir, "Class1.cs");
        var class2Path = Path.Combine(project2Dir, "Class2.cs");
        File.WriteAllText(class1Path, "// class1");
        File.WriteAllText(class2Path, "// class2");

        var entries = new List<FileEntry>
        {
            new() { RelativePath = "Project1/Project1.csproj", FullPath = csproj1Path, Size = 0 },
            new() { RelativePath = "Project1/Class1.cs", FullPath = class1Path, Size = 0 },
            new() { RelativePath = "Project2/Project2.csproj", FullPath = csproj2Path, Size = 0 },
            new() { RelativePath = "Project2/Class2.cs", FullPath = class2Path, Size = 0 }
        };

        var reader = new MsBuildProjectMetadataReader();

        // Act
        reader.EnrichBuildActions(entries, _tempDir);

        // Assert
        Assert.Equal(BuildAction.Compile, entries.First(e => e.RelativePath.EndsWith("Class1.cs")).Action);
        Assert.Equal(BuildAction.Compile, entries.First(e => e.RelativePath.EndsWith("Class2.cs")).Action);
    }

    [Fact]
    public void EnrichBuildActions_WithoutIncludeAttribute_IgnoresElement()
    {
        // Arrange
        var projectDir = Path.Combine(_tempDir, "NoIncludeProject");
        Directory.CreateDirectory(projectDir);

        var csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <Compile />
    <Compile Include="""" />
    <Compile Include=""   "" />
  </ItemGroup>
</Project>";
        var csprojPath = Path.Combine(projectDir, "NoIncludeProject.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        var entries = new List<FileEntry>
        {
            new() { RelativePath = "NoIncludeProject/NoIncludeProject.csproj", FullPath = csprojPath, Size = 0 }
        };

        var reader = new MsBuildProjectMetadataReader();

        // Act
        var exception = Record.Exception(() => reader.EnrichBuildActions(entries, _tempDir));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void EnrichBuildActions_HandlesBackslashesInPaths()
    {
        // Arrange
        var projectDir = Path.Combine(_tempDir, "PathProject");
        Directory.CreateDirectory(projectDir);
        var subDir = Path.Combine(projectDir, "SubFolder");
        Directory.CreateDirectory(subDir);

        var csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <Compile Include=""SubFolder\Helper.cs"" />
  </ItemGroup>
</Project>";
        var csprojPath = Path.Combine(projectDir, "PathProject.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        var helperPath = Path.Combine(subDir, "Helper.cs");
        File.WriteAllText(helperPath, "// helper");

        var entries = new List<FileEntry>
        {
            new() { RelativePath = "PathProject/PathProject.csproj", FullPath = csprojPath, Size = 0 },
            new() { RelativePath = "PathProject/SubFolder/Helper.cs", FullPath = helperPath, Size = 0 }
        };

        var reader = new MsBuildProjectMetadataReader();

        // Act
        reader.EnrichBuildActions(entries, _tempDir);

        // Assert
        var helperEntry = entries.First(e => e.RelativePath.EndsWith("Helper.cs"));
        Assert.Equal(BuildAction.Compile, helperEntry.Action);
    }

    [Fact]
    public void EnrichBuildActions_UnknownBuildAction_SetsUnknown()
    {
        // Arrange
        var projectDir = Path.Combine(_tempDir, "UnknownActionProject");
        Directory.CreateDirectory(projectDir);

        var csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <CustomBuildAction Include=""custom.txt"" />
  </ItemGroup>
</Project>";
        var csprojPath = Path.Combine(projectDir, "UnknownActionProject.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        var customPath = Path.Combine(projectDir, "custom.txt");
        File.WriteAllText(customPath, "custom");

        var entries = new List<FileEntry>
        {
            new() { RelativePath = "UnknownActionProject/UnknownActionProject.csproj", FullPath = csprojPath, Size = 0 },
            new() { RelativePath = "UnknownActionProject/custom.txt", FullPath = customPath, Size = 0 }
        };

        var reader = new MsBuildProjectMetadataReader();

        // Act
        reader.EnrichBuildActions(entries, _tempDir);

        // Assert
        var customEntry = entries.First(e => e.RelativePath.EndsWith("custom.txt"));
        Assert.Equal(BuildAction.Unknown, customEntry.Action);
    }

    [Fact]
    public void EnrichBuildActions_CsprojTakesPrecedenceOverFallback()
    {
        // Arrange
        var projectDir = Path.Combine(_tempDir, "PrecedenceProject");
        Directory.CreateDirectory(projectDir);

        var csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <Content Include=""Script.cs"" />
  </ItemGroup>
</Project>";
        var csprojPath = Path.Combine(projectDir, "PrecedenceProject.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        var scriptPath = Path.Combine(projectDir, "Script.cs");
        File.WriteAllText(scriptPath, "// script");

        var entries = new List<FileEntry>
        {
            new() { RelativePath = "PrecedenceProject/PrecedenceProject.csproj", FullPath = csprojPath, Size = 0 },
            new() { RelativePath = "PrecedenceProject/Script.cs", FullPath = scriptPath, Size = 0 }
        };

        var reader = new MsBuildProjectMetadataReader();

        // Act
        reader.EnrichBuildActions(entries, _tempDir);

        // Assert
        var scriptEntry = entries.First(e => e.RelativePath.EndsWith("Script.cs"));
        Assert.Equal(BuildAction.Content, scriptEntry.Action); // Should be Content from csproj, not Compile from fallback
    }

    [Fact]
    public void EnrichBuildActions_FirstCsprojEntryWins_WhenDuplicates()
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

        var testPath = Path.Combine(projectDir, "Test.cs");
        File.WriteAllText(testPath, "// test");

        var entries = new List<FileEntry>
        {
            new() { RelativePath = "DuplicateProject/DuplicateProject.csproj", FullPath = csprojPath, Size = 0 },
            new() { RelativePath = "DuplicateProject/Test.cs", FullPath = testPath, Size = 0 }
        };

        var reader = new MsBuildProjectMetadataReader();

        // Act
        reader.EnrichBuildActions(entries, _tempDir);

        // Assert
        var testEntry = entries.First(e => e.RelativePath.EndsWith("Test.cs"));
        Assert.Equal(BuildAction.Compile, testEntry.Action); // First entry should win
    }
}