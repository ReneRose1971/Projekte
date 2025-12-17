using FluentAssertions;
using Xunit;

namespace Scriptum.Core.Tests;

public class KeyIdTests
{
    [Fact]
    public void KeyId_None_Has_Value_Zero()
    {
        ((int)KeyId.None).Should().Be(0);
    }
    
    [Fact]
    public void KeyId_Has_All_Letter_Keys()
    {
        KeyId.A.Should().BeDefined();
        KeyId.Z.Should().BeDefined();
    }
    
    [Fact]
    public void KeyId_Has_All_Digit_Keys()
    {
        KeyId.Digit0.Should().BeDefined();
        KeyId.Digit9.Should().BeDefined();
    }
    
    [Fact]
    public void KeyId_Has_Space_Key()
    {
        KeyId.Space.Should().BeDefined();
    }
    
    [Fact]
    public void KeyId_Has_Enter_Key()
    {
        KeyId.Enter.Should().BeDefined();
    }
    
    [Fact]
    public void KeyId_Has_Backspace_Key()
    {
        KeyId.Backspace.Should().BeDefined();
    }
}
