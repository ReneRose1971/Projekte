using FluentAssertions;
using Scriptum.Core;
using Scriptum.Progress;
using Xunit;

namespace Scriptum.Progress.Tests;

/// <summary>
/// Tests für <see cref="StoredInput"/>.
/// </summary>
public sealed class StoredInputTests
{
    [Fact]
    public void Constructor_WithValidValues_CreatesInstance()
    {
        var zeitpunkt = DateTimeOffset.UtcNow;
        var taste = KeyId.A;
        var umschalter = ModifierSet.None;
        var art = StoredInputKind.Zeichen;
        var graphem = "a";

        var input = new StoredInput
        {
            Zeitpunkt = zeitpunkt,
            Taste = taste,
            Umschalter = umschalter,
            Art = art,
            ErzeugtesGraphem = graphem
        };

        input.Zeitpunkt.Should().Be(zeitpunkt);
    }

    [Fact]
    public void Zeitpunkt_DefaultValue_ThrowsArgumentException()
    {
        var act = () => new StoredInput
        {
            Zeitpunkt = default,
            Taste = KeyId.A,
            Umschalter = ModifierSet.None,
            Art = StoredInputKind.Zeichen,
            ErzeugtesGraphem = "a"
        };

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Zeitpunkt*");
    }

    [Fact]
    public void ErzeugtesGraphem_Null_ThrowsArgumentException()
    {
        var act = () => new StoredInput
        {
            Zeitpunkt = DateTimeOffset.UtcNow,
            Taste = KeyId.A,
            Umschalter = ModifierSet.None,
            Art = StoredInputKind.Zeichen,
            ErzeugtesGraphem = null!
        };

        act.Should().Throw<ArgumentException>()
            .WithMessage("*ErzeugtesGraphem*");
    }

    [Fact]
    public void ErzeugtesGraphem_EmptyString_IsAllowed()
    {
        var input = new StoredInput
        {
            Zeitpunkt = DateTimeOffset.UtcNow,
            Taste = KeyId.Backspace,
            Umschalter = ModifierSet.None,
            Art = StoredInputKind.Ruecktaste,
            ErzeugtesGraphem = string.Empty
        };

        input.ErzeugtesGraphem.Should().BeEmpty();
    }

    [Fact]
    public void Taste_SetCorrectly()
    {
        var input = new StoredInput
        {
            Zeitpunkt = DateTimeOffset.UtcNow,
            Taste = KeyId.B,
            Umschalter = ModifierSet.None,
            Art = StoredInputKind.Zeichen,
            ErzeugtesGraphem = "b"
        };

        input.Taste.Should().Be(KeyId.B);
    }

    [Fact]
    public void Umschalter_SetCorrectly()
    {
        var input = new StoredInput
        {
            Zeitpunkt = DateTimeOffset.UtcNow,
            Taste = KeyId.A,
            Umschalter = ModifierSet.Shift,
            Art = StoredInputKind.Zeichen,
            ErzeugtesGraphem = "A"
        };

        input.Umschalter.Should().Be(ModifierSet.Shift);
    }

    [Fact]
    public void Art_SetCorrectly()
    {
        var input = new StoredInput
        {
            Zeitpunkt = DateTimeOffset.UtcNow,
            Taste = KeyId.Backspace,
            Umschalter = ModifierSet.None,
            Art = StoredInputKind.Ruecktaste,
            ErzeugtesGraphem = string.Empty
        };

        input.Art.Should().Be(StoredInputKind.Ruecktaste);
    }

    [Fact]
    public void Record_Equality_WithSameValues_AreEqual()
    {
        var zeitpunkt = DateTimeOffset.UtcNow;
        var input1 = new StoredInput
        {
            Zeitpunkt = zeitpunkt,
            Taste = KeyId.A,
            Umschalter = ModifierSet.None,
            Art = StoredInputKind.Zeichen,
            ErzeugtesGraphem = "a"
        };
        
        var input2 = new StoredInput
        {
            Zeitpunkt = zeitpunkt,
            Taste = KeyId.A,
            Umschalter = ModifierSet.None,
            Art = StoredInputKind.Zeichen,
            ErzeugtesGraphem = "a"
        };

        input1.Should().Be(input2);
    }

    [Fact]
    public void Record_Equality_WithDifferentValues_AreNotEqual()
    {
        var zeitpunkt = DateTimeOffset.UtcNow;
        var input1 = new StoredInput
        {
            Zeitpunkt = zeitpunkt,
            Taste = KeyId.A,
            Umschalter = ModifierSet.None,
            Art = StoredInputKind.Zeichen,
            ErzeugtesGraphem = "a"
        };
        
        var input2 = new StoredInput
        {
            Zeitpunkt = zeitpunkt,
            Taste = KeyId.B,
            Umschalter = ModifierSet.None,
            Art = StoredInputKind.Zeichen,
            ErzeugtesGraphem = "b"
        };

        input1.Should().NotBe(input2);
    }
}
