using FluentAssertions;
using Scriptum.Engine;
using Xunit;

namespace Scriptum.Engine.Tests;

public class SystemClockTests
{
    [Fact]
    public void Now_ShouldReturnCurrentTime()
    {
        var clock = new SystemClock();
        var beforeCall = DateTime.Now;
        
        var result = clock.Now;
        
        var afterCall = DateTime.Now;
        result.Should().BeOnOrAfter(beforeCall);
        result.Should().BeOnOrBefore(afterCall);
    }
    
    [Fact]
    public void Now_CalledMultipleTimes_ShouldReturnIncreasingOrEqualTimes()
    {
        var clock = new SystemClock();
        
        var time1 = clock.Now;
        var time2 = clock.Now;
        
        time2.Should().BeOnOrAfter(time1);
    }
}
