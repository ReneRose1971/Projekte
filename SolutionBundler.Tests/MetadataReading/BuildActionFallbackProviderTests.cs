using SolutionBundler.Core.Implementations.MetadataReading;
using SolutionBundler.Core.Models;
using Xunit;

namespace SolutionBundler.Tests.MetadataReading;

public class BuildActionFallbackProviderTests
{
    [Theory]
    [InlineData("Program.cs", BuildAction.Compile)]
    [InlineData("MainWindow.xaml", BuildAction.Page)]
    [InlineData("Strings.resx", BuildAction.Resource)]
    [InlineData("appsettings.json", BuildAction.Content)]
    [InlineData("app.config", BuildAction.Content)]
    [InlineData("Directory.Build.props", BuildAction.Content)]
    [InlineData("build.targets", BuildAction.Content)]
    [InlineData("README.md", BuildAction.Unknown)]
    [InlineData("image.png", BuildAction.Unknown)]
    [InlineData("noextension", BuildAction.Unknown)]
    public void GetBuildActionByExtension_ReturnsCorrectBuildAction(string filePath, BuildAction expected)
    {
        var result = BuildActionFallbackProvider.GetBuildActionByExtension(filePath);
        
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Test.CS")]
    [InlineData("Test.Cs")]
    [InlineData("Test.cS")]
    public void GetBuildActionByExtension_IsCaseInsensitive(string filePath)
    {
        var result = BuildActionFallbackProvider.GetBuildActionByExtension(filePath);
        
        Assert.Equal(BuildAction.Compile, result);
    }

    [Fact]
    public void GetBuildActionByExtension_WorksWithFullPaths()
    {
        var result = BuildActionFallbackProvider.GetBuildActionByExtension(@"C:\Projects\MyApp\src\Program.cs");
        
        Assert.Equal(BuildAction.Compile, result);
    }

    [Fact]
    public void GetBuildActionByExtension_WorksWithRelativePaths()
    {
        var result = BuildActionFallbackProvider.GetBuildActionByExtension("src/subfolder/Helper.cs");
        
        Assert.Equal(BuildAction.Compile, result);
    }
}
