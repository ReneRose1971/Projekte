using FluentAssertions;
using Xunit;

namespace Scriptum.Core.Tests;

public class ModifierSetTests
{
    [Fact]
    public void None_Has_Value_Zero()
    {
        ((int)ModifierSet.None).Should().Be(0);
    }
    
    [Fact]
    public void Shift_Has_Value_One()
    {
        ((int)ModifierSet.Shift).Should().Be(1);
    }
    
    [Fact]
    public void AltGr_Has_Value_Two()
    {
        ((int)ModifierSet.AltGr).Should().Be(2);
    }
    
    [Fact]
    public void Flags_Can_Be_Combined()
    {
        var combined = ModifierSet.Shift | ModifierSet.AltGr;
        combined.Should().HaveFlag(ModifierSet.Shift);
    }
    
    [Fact]
    public void Combined_Flags_Include_AltGr()
    {
        var combined = ModifierSet.Shift | ModifierSet.AltGr;
        combined.Should().HaveFlag(ModifierSet.AltGr);
    }
}
