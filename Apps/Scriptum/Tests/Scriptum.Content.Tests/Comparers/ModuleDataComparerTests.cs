using FluentAssertions;
using Scriptum.Content.Comparers;
using Scriptum.Content.Data;
using Xunit;

namespace Scriptum.Content.Tests.Comparers;

public sealed class ModuleDataComparerTests
{
    private readonly ModuleDataComparer _comparer = new();

    [Fact]
    public void Equals_Should_Return_True_ForSameInstance()
    {
        var module = new ModuleData("module1", "Titel");

        var result = _comparer.Equals(module, module);

        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_Return_True_ForBothNull()
    {
        var result = _comparer.Equals(null, null);

        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_Return_False_WhenFirstIsNull()
    {
        var module = new ModuleData("module1", "Titel");

        var result = _comparer.Equals(null, module);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_Should_Return_False_WhenSecondIsNull()
    {
        var module = new ModuleData("module1", "Titel");

        var result = _comparer.Equals(module, null);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_Should_Return_True_ForSameModuleId()
    {
        var module1 = new ModuleData("module1", "Titel 1");
        var module2 = new ModuleData("module1", "Titel 2");

        var result = _comparer.Equals(module1, module2);

        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_Return_False_ForDifferentModuleIds()
    {
        var module1 = new ModuleData("module1", "Titel");
        var module2 = new ModuleData("module2", "Titel");

        var result = _comparer.Equals(module1, module2);

        result.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_Should_ReturnSameValue_ForSameModuleId()
    {
        var module1 = new ModuleData("module1", "Titel 1");
        var module2 = new ModuleData("module1", "Titel 2");

        var hash1 = _comparer.GetHashCode(module1);
        var hash2 = _comparer.GetHashCode(module2);

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetHashCode_Should_Throw_WhenModuleIsNull()
    {
        var act = () => _comparer.GetHashCode(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("obj");
    }
}
