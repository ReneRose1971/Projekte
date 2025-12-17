using FluentAssertions;
using Scriptum.Engine;
using Xunit;

namespace Scriptum.Engine.Tests;

public class EngineOptionsTests
{
    [Fact]
    public void DefaultConstructor_ShouldSetMustCorrectErrorsToTrue()
    {
        var options = new EngineOptions();
        
        options.MustCorrectErrors.Should().BeTrue();
    }
    
    [Fact]
    public void DefaultConstructor_ShouldSetLineBreaksAreTargetsToTrue()
    {
        var options = new EngineOptions();
        
        options.LineBreaksAreTargets.Should().BeTrue();
    }
    
    [Fact]
    public void InitOnlyProperties_CanBeSetViaObjectInitializer()
    {
        var options = new EngineOptions
        {
            MustCorrectErrors = false,
            LineBreaksAreTargets = false
        };
        
        options.MustCorrectErrors.Should().BeFalse();
        options.LineBreaksAreTargets.Should().BeFalse();
    }
}
