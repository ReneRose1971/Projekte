using FluentAssertions;
using Scriptum.Content.Data;
using Xunit;

namespace Scriptum.Content.Tests.Data;

public sealed class ModuleDataTests
{
    [Fact]
    public void Constructor_Should_Throw_WhenModuleIdIsEmpty()
    {
        var act = () => new ModuleData("", "Titel");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("moduleId");
    }

    [Fact]
    public void Constructor_Should_Throw_WhenModuleIdIsWhitespace()
    {
        var act = () => new ModuleData("   ", "Titel");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("moduleId");
    }

    [Fact]
    public void Constructor_Should_Throw_WhenTitelIsEmpty()
    {
        var act = () => new ModuleData("module1", "");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("titel");
    }

    [Fact]
    public void Constructor_Should_Throw_WhenTitelIsWhitespace()
    {
        var act = () => new ModuleData("module1", "   ");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("titel");
    }

    [Fact]
    public void Constructor_Should_SetBeschreibungToEmptyString_WhenNotProvided()
    {
        var module = new ModuleData("module1", "Titel");

        module.Beschreibung.Should().Be(string.Empty);
    }

    [Fact]
    public void Constructor_Should_SetBeschreibungToEmptyString_WhenNull()
    {
        var module = new ModuleData("module1", "Titel", beschreibung: null!);

        module.Beschreibung.Should().Be(string.Empty);
    }

    [Fact]
    public void Constructor_Should_SetOrderToZero_WhenNotProvided()
    {
        var module = new ModuleData("module1", "Titel");

        module.Order.Should().Be(0);
    }

    [Fact]
    public void Constructor_Should_CreateValidInstance_WithAllParameters()
    {
        var module = new ModuleData(
            moduleId: "module1",
            titel: "Grundlagen",
            beschreibung: "Einführung in die Basics",
            order: 1);

        module.ModuleId.Should().Be("module1");
        module.Titel.Should().Be("Grundlagen");
        module.Beschreibung.Should().Be("Einführung in die Basics");
        module.Order.Should().Be(1);
    }
}
