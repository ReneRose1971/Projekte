using SolutionBundler.Core.Implementations.MetadataReading;
using SolutionBundler.Core.Models;
using Xunit;

namespace SolutionBundler.Tests.MetadataReading;

public class BuildActionMapperTests
{
    [Theory]
    [InlineData("Compile", BuildAction.Compile)]
    [InlineData("Page", BuildAction.Page)]
    [InlineData("Resource", BuildAction.Resource)]
    [InlineData("Content", BuildAction.Content)]
    [InlineData("None", BuildAction.None)]
    [InlineData("UnknownElement", BuildAction.Unknown)]
    [InlineData("", BuildAction.Unknown)]
    public void MapElementToBuildAction_ReturnsCorrectBuildAction(string elementName, BuildAction expected)
    {
        var result = BuildActionMapper.MapElementToBuildAction(elementName);
        
        Assert.Equal(expected, result);
    }

    [Fact]
    public void MapElementToBuildAction_IsCaseSensitive()
    {
        var result1 = BuildActionMapper.MapElementToBuildAction("compile");
        var result2 = BuildActionMapper.MapElementToBuildAction("COMPILE");
        
        Assert.Equal(BuildAction.Unknown, result1);
        Assert.Equal(BuildAction.Unknown, result2);
    }
}
