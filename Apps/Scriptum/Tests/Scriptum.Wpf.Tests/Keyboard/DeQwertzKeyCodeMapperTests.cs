using System.Windows.Input;
using FluentAssertions;
using Scriptum.Wpf.Keyboard;
using Xunit;

namespace Scriptum.Wpf.Tests.Keyboard;

/// <summary>
/// Tests für DeQwertzKeyCodeMapper.
/// </summary>
public sealed class DeQwertzKeyCodeMapperTests
{
    private readonly DeQwertzKeyCodeMapper _mapper = new();

    #region Letter Keys

    [Theory]
    [InlineData(Key.A, "A")]
    [InlineData(Key.B, "B")]
    [InlineData(Key.Z, "Z")]
    public void MapToLabel_WithLetterKeys_ShouldReturnUppercaseLetter(Key key, string expected)
    {
        var result = _mapper.MapToLabel(key);

        result.Should().Be(expected);
    }

    #endregion

    #region Digit Keys

    [Theory]
    [InlineData(Key.D0, "0")]
    [InlineData(Key.D1, "1")]
    [InlineData(Key.D9, "9")]
    public void MapToLabel_WithDigitKeys_ShouldReturnDigit(Key key, string expected)
    {
        var result = _mapper.MapToLabel(key);

        result.Should().Be(expected);
    }

    #endregion

    #region Special Keys

    [Fact]
    public void MapToLabel_WithSpace_ShouldReturnSpace()
    {
        var result = _mapper.MapToLabel(Key.Space);

        result.Should().Be("Space");
    }

    [Theory]
    [InlineData(Key.Enter)]
    [InlineData(Key.Return)]
    public void MapToLabel_WithEnterOrReturn_ShouldReturnEnter(Key key)
    {
        var result = _mapper.MapToLabel(key);

        result.Should().Be("Enter");
    }

    [Fact]
    public void MapToLabel_WithBackspace_ShouldReturnBackspace()
    {
        var result = _mapper.MapToLabel(Key.Back);

        result.Should().Be("Backspace");
    }

    [Fact]
    public void MapToLabel_WithTab_ShouldReturnTab()
    {
        var result = _mapper.MapToLabel(Key.Tab);

        result.Should().Be("Tab");
    }

    [Fact]
    public void MapToLabel_WithEscape_ShouldReturnEsc()
    {
        var result = _mapper.MapToLabel(Key.Escape);

        result.Should().Be("Esc");
    }

    #endregion

    #region German Umlauts

    [Fact]
    public void MapToLabel_WithOem1_ShouldReturnOe()
    {
        var result = _mapper.MapToLabel(Key.Oem1);

        result.Should().Be("ö");
    }

    [Fact]
    public void MapToLabel_WithOem3_ShouldReturnAe()
    {
        var result = _mapper.MapToLabel(Key.Oem3);

        result.Should().Be("ä");
    }

    [Fact]
    public void MapToLabel_WithOem7_ShouldReturnSzett()
    {
        var result = _mapper.MapToLabel(Key.Oem7);

        result.Should().Be("ß");
    }

    [Fact]
    public void MapToLabel_WithOemOpenBrackets_ShouldReturnUe()
    {
        var result = _mapper.MapToLabel(Key.OemOpenBrackets);

        result.Should().Be("ü");
    }

    #endregion

    #region Punctuation Keys

    [Fact]
    public void MapToLabel_WithOemComma_ShouldReturnComma()
    {
        var result = _mapper.MapToLabel(Key.OemComma);

        result.Should().Be(",");
    }

    [Fact]
    public void MapToLabel_WithOemPeriod_ShouldReturnPeriod()
    {
        var result = _mapper.MapToLabel(Key.OemPeriod);

        result.Should().Be(".");
    }

    [Fact]
    public void MapToLabel_WithOemMinus_ShouldReturnMinus()
    {
        var result = _mapper.MapToLabel(Key.OemMinus);

        result.Should().Be("-");
    }

    [Fact]
    public void MapToLabel_WithOemPlus_ShouldReturnPlus()
    {
        var result = _mapper.MapToLabel(Key.OemPlus);

        result.Should().Be("+");
    }

    [Fact]
    public void MapToLabel_WithOem102_ShouldReturnAngleBrackets()
    {
        var result = _mapper.MapToLabel(Key.Oem102);

        result.Should().Be("< > |");
    }

    [Fact]
    public void MapToLabel_WithOem2_ShouldReturnHash()
    {
        var result = _mapper.MapToLabel(Key.Oem2);

        result.Should().Be("#");
    }

    [Fact]
    public void MapToLabel_WithOem5_ShouldReturnCaret()
    {
        var result = _mapper.MapToLabel(Key.Oem5);

        result.Should().Be("^");
    }

    #endregion

    #region Modifier Keys

    [Theory]
    [InlineData(Key.LeftShift)]
    [InlineData(Key.RightShift)]
    public void MapToLabel_WithShiftKeys_ShouldReturnShift(Key key)
    {
        var result = _mapper.MapToLabel(key);

        result.Should().Be("Shift");
    }

    [Theory]
    [InlineData(Key.LeftCtrl)]
    [InlineData(Key.RightCtrl)]
    public void MapToLabel_WithCtrlKeys_ShouldReturnCtrl(Key key)
    {
        var result = _mapper.MapToLabel(key);

        result.Should().Be("Ctrl");
    }

    [Fact]
    public void MapToLabel_WithLeftAlt_ShouldReturnAlt()
    {
        var result = _mapper.MapToLabel(Key.LeftAlt);

        result.Should().Be("Alt");
    }

    [Fact]
    public void MapToLabel_WithRightAlt_ShouldReturnAltGr()
    {
        var result = _mapper.MapToLabel(Key.RightAlt);

        result.Should().Be("AltGr");
    }

    #endregion

    #region Unmapped Keys

    [Theory]
    [InlineData(Key.F1)]
    [InlineData(Key.F12)]
    [InlineData(Key.Home)]
    [InlineData(Key.End)]
    [InlineData(Key.PageUp)]
    [InlineData(Key.PageDown)]
    [InlineData(Key.Insert)]
    [InlineData(Key.Delete)]
    public void MapToLabel_WithUnmappedKeys_ShouldReturnNull(Key key)
    {
        var result = _mapper.MapToLabel(key);

        result.Should().BeNull();
    }

    #endregion
}
