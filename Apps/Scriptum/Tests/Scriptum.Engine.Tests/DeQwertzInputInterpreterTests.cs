using FluentAssertions;
using Scriptum.Core;
using Scriptum.Engine;
using Xunit;

namespace Scriptum.Engine.Tests;

public class DeQwertzInputInterpreterTests
{
    private readonly DeQwertzInputInterpreter _interpreter = new();
    private readonly DateTime _timestamp = DateTime.UtcNow;

    [Fact]
    public void Interpret_A_WithoutShift_ReturnsLowercaseA()
    {
        var chord = new KeyChord(KeyId.A, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("a");
    }

    [Fact]
    public void Interpret_A_WithShift_ReturnsUppercaseA()
    {
        var chord = new KeyChord(KeyId.A, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("A");
    }

    [Fact]
    public void Interpret_Z_WithoutShift_ReturnsLowercaseZ()
    {
        var chord = new KeyChord(KeyId.Z, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("z");
    }

    [Fact]
    public void Interpret_Z_WithShift_ReturnsUppercaseZ()
    {
        var chord = new KeyChord(KeyId.Z, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("Z");
    }

    [Fact]
    public void Interpret_M_WithoutShift_ReturnsLowercaseM()
    {
        var chord = new KeyChord(KeyId.M, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("m");
    }

    [Fact]
    public void Interpret_M_WithShift_ReturnsUppercaseM()
    {
        var chord = new KeyChord(KeyId.M, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("M");
    }

    [Fact]
    public void Interpret_Space_ReturnsSpace()
    {
        var chord = new KeyChord(KeyId.Space, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be(" ");
    }

    [Fact]
    public void Interpret_Enter_ReturnsNewline()
    {
        var chord = new KeyChord(KeyId.Enter, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("\n");
    }

    [Fact]
    public void Interpret_Backspace_ReturnsRuecktaste()
    {
        var chord = new KeyChord(KeyId.Backspace, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Ruecktaste);
    }

    [Fact]
    public void Interpret_Backspace_ReturnsNullGraphem()
    {
        var chord = new KeyChord(KeyId.Backspace, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Graphem.Should().BeNull();
    }

    [Fact]
    public void Interpret_Digit1_WithoutShift_Returns1()
    {
        var chord = new KeyChord(KeyId.Digit1, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("1");
    }

    [Fact]
    public void Interpret_Digit1_WithShift_ReturnsExclamationMark()
    {
        var chord = new KeyChord(KeyId.Digit1, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("!");
    }

    [Fact]
    public void Interpret_Digit2_WithoutShift_Returns2()
    {
        var chord = new KeyChord(KeyId.Digit2, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("2");
    }

    [Fact]
    public void Interpret_Digit2_WithShift_ReturnsQuotationMark()
    {
        var chord = new KeyChord(KeyId.Digit2, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("\"");
    }

    [Fact]
    public void Interpret_Digit3_WithoutShift_Returns3()
    {
        var chord = new KeyChord(KeyId.Digit3, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("3");
    }

    [Fact]
    public void Interpret_Digit3_WithShift_ReturnsSectionSign()
    {
        var chord = new KeyChord(KeyId.Digit3, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("§");
    }

    [Fact]
    public void Interpret_Digit4_WithoutShift_Returns4()
    {
        var chord = new KeyChord(KeyId.Digit4, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("4");
    }

    [Fact]
    public void Interpret_Digit4_WithShift_ReturnsDollarSign()
    {
        var chord = new KeyChord(KeyId.Digit4, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("$");
    }

    [Fact]
    public void Interpret_Digit5_WithoutShift_Returns5()
    {
        var chord = new KeyChord(KeyId.Digit5, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("5");
    }

    [Fact]
    public void Interpret_Digit5_WithShift_ReturnsPercentSign()
    {
        var chord = new KeyChord(KeyId.Digit5, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("%");
    }

    [Fact]
    public void Interpret_Digit6_WithoutShift_Returns6()
    {
        var chord = new KeyChord(KeyId.Digit6, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("6");
    }

    [Fact]
    public void Interpret_Digit6_WithShift_ReturnsAmpersand()
    {
        var chord = new KeyChord(KeyId.Digit6, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("&");
    }

    [Fact]
    public void Interpret_Digit7_WithoutShift_Returns7()
    {
        var chord = new KeyChord(KeyId.Digit7, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("7");
    }

    [Fact]
    public void Interpret_Digit7_WithShift_ReturnsSlash()
    {
        var chord = new KeyChord(KeyId.Digit7, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("/");
    }

    [Fact]
    public void Interpret_Digit8_WithoutShift_Returns8()
    {
        var chord = new KeyChord(KeyId.Digit8, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("8");
    }

    [Fact]
    public void Interpret_Digit8_WithShift_ReturnsOpenParenthesis()
    {
        var chord = new KeyChord(KeyId.Digit8, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("(");
    }

    [Fact]
    public void Interpret_Digit9_WithoutShift_Returns9()
    {
        var chord = new KeyChord(KeyId.Digit9, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("9");
    }

    [Fact]
    public void Interpret_Digit9_WithShift_ReturnsCloseParenthesis()
    {
        var chord = new KeyChord(KeyId.Digit9, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be(")");
    }

    [Fact]
    public void Interpret_Digit0_WithoutShift_Returns0()
    {
        var chord = new KeyChord(KeyId.Digit0, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("0");
    }

    [Fact]
    public void Interpret_Digit0_WithShift_ReturnsEquals()
    {
        var chord = new KeyChord(KeyId.Digit0, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("=");
    }

    [Fact]
    public void Interpret_OemPlus_WithoutShift_ReturnsPlus()
    {
        var chord = new KeyChord(KeyId.OemPlus, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("+");
    }

    [Fact]
    public void Interpret_OemPlus_WithShift_ReturnsAsterisk()
    {
        var chord = new KeyChord(KeyId.OemPlus, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("*");
    }

    [Fact]
    public void Interpret_OemMinus_WithoutShift_ReturnsMinus()
    {
        var chord = new KeyChord(KeyId.OemMinus, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("-");
    }

    [Fact]
    public void Interpret_OemMinus_WithShift_ReturnsUnderscore()
    {
        var chord = new KeyChord(KeyId.OemMinus, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("_");
    }

    [Fact]
    public void Interpret_OemComma_WithoutShift_ReturnsComma()
    {
        var chord = new KeyChord(KeyId.OemComma, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be(",");
    }

    [Fact]
    public void Interpret_OemComma_WithShift_ReturnsSemicolon()
    {
        var chord = new KeyChord(KeyId.OemComma, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be(";");
    }

    [Fact]
    public void Interpret_OemPeriod_WithoutShift_ReturnsPeriod()
    {
        var chord = new KeyChord(KeyId.OemPeriod, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be(".");
    }

    [Fact]
    public void Interpret_OemPeriod_WithShift_ReturnsColon()
    {
        var chord = new KeyChord(KeyId.OemPeriod, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be(":");
    }

    [Fact]
    public void Interpret_Oem1_WithoutShift_ReturnsLowercaseUUmlaut()
    {
        var chord = new KeyChord(KeyId.Oem1, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("ü");
    }

    [Fact]
    public void Interpret_Oem1_WithShift_ReturnsUppercaseUUmlaut()
    {
        var chord = new KeyChord(KeyId.Oem1, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("Ü");
    }

    [Fact]
    public void Interpret_Oem3_WithoutShift_ReturnsLowercaseOUmlaut()
    {
        var chord = new KeyChord(KeyId.Oem3, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("ö");
    }

    [Fact]
    public void Interpret_Oem3_WithShift_ReturnsUppercaseOUmlaut()
    {
        var chord = new KeyChord(KeyId.Oem3, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("Ö");
    }

    [Fact]
    public void Interpret_Oem7_WithoutShift_ReturnsLowercaseAUmlaut()
    {
        var chord = new KeyChord(KeyId.Oem7, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("ä");
    }

    [Fact]
    public void Interpret_Oem7_WithShift_ReturnsUppercaseAUmlaut()
    {
        var chord = new KeyChord(KeyId.Oem7, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("Ä");
    }

    [Fact]
    public void Interpret_Oem102_WithoutShift_ReturnsLessThan()
    {
        var chord = new KeyChord(KeyId.Oem102, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("<");
    }

    [Fact]
    public void Interpret_Oem102_WithShift_ReturnsGreaterThan()
    {
        var chord = new KeyChord(KeyId.Oem102, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be(">");
    }

    [Fact]
    public void Interpret_Oem2_WithoutShift_ReturnsHash()
    {
        var chord = new KeyChord(KeyId.Oem2, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("#");
    }

    [Fact]
    public void Interpret_Oem2_WithShift_ReturnsSingleQuote()
    {
        var chord = new KeyChord(KeyId.Oem2, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("'");
    }

    [Fact]
    public void Interpret_Oem5_WithoutShift_ReturnsCaret()
    {
        var chord = new KeyChord(KeyId.Oem5, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("^");
    }

    [Fact]
    public void Interpret_Oem5_WithShift_ReturnsDegree()
    {
        var chord = new KeyChord(KeyId.Oem5, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("°");
    }

    [Fact]
    public void Interpret_Oem4_WithoutShift_ReturnsSzett()
    {
        var chord = new KeyChord(KeyId.Oem4, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("ß");
    }

    [Fact]
    public void Interpret_Oem4_WithShift_ReturnsQuestionMark()
    {
        var chord = new KeyChord(KeyId.Oem4, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("?");
    }

    [Fact]
    public void Interpret_Oem6_WithoutShift_ReturnsAcuteAccent()
    {
        var chord = new KeyChord(KeyId.Oem6, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("´");
    }

    [Fact]
    public void Interpret_Oem6_WithShift_ReturnsGraveAccent()
    {
        var chord = new KeyChord(KeyId.Oem6, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("`");
    }

    [Fact]
    public void Interpret_E_WithAltGr_ReturnsEuroSign()
    {
        var chord = new KeyChord(KeyId.E, ModifierSet.AltGr);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("€");
    }

    [Fact]
    public void Interpret_OemPlus_WithAltGr_ReturnsTilde()
    {
        var chord = new KeyChord(KeyId.OemPlus, ModifierSet.AltGr);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Zeichen);
        result.Graphem.Should().Be("~");
    }

    [Fact]
    public void Interpret_A_WithAltGr_ReturnsIgnoriert()
    {
        var chord = new KeyChord(KeyId.A, ModifierSet.AltGr);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Ignoriert);
    }

    [Fact]
    public void Interpret_Tab_ReturnsIgnoriert()
    {
        var chord = new KeyChord(KeyId.Tab, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Ignoriert);
    }

    [Fact]
    public void Interpret_Escape_ReturnsIgnoriert()
    {
        var chord = new KeyChord(KeyId.Escape, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Ignoriert);
    }

    [Fact]
    public void Interpret_None_ReturnsIgnoriert()
    {
        var chord = new KeyChord(KeyId.None, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Ignoriert);
    }

    [Fact]
    public void Interpret_LeftShift_ReturnsIgnoriert()
    {
        var chord = new KeyChord(KeyId.LeftShift, ModifierSet.None);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Kind.Should().Be(InputEventKind.Ignoriert);
    }

    [Fact]
    public void Interpret_ReturnsCorrectTimestamp()
    {
        var specificTime = new DateTime(2025, 1, 15, 10, 30, 45);
        var chord = new KeyChord(KeyId.A, ModifierSet.None);

        var result = _interpreter.Interpret(chord, specificTime);

        result.Timestamp.Should().Be(specificTime);
    }

    [Fact]
    public void Interpret_ReturnsCorrectChord()
    {
        var chord = new KeyChord(KeyId.B, ModifierSet.Shift);

        var result = _interpreter.Interpret(chord, _timestamp);

        result.Chord.Should().Be(chord);
    }

    [Fact]
    public void Interpret_AllLetters_WithoutShift_ReturnLowercase()
    {
        var letters = new[] { KeyId.A, KeyId.B, KeyId.C, KeyId.D, KeyId.E, KeyId.F, KeyId.G, KeyId.H, KeyId.I, KeyId.J, KeyId.K, KeyId.L, KeyId.M, KeyId.N, KeyId.O, KeyId.P, KeyId.Q, KeyId.R, KeyId.S, KeyId.T, KeyId.U, KeyId.V, KeyId.W, KeyId.X, KeyId.Y, KeyId.Z };
        var expectedChars = "abcdefghijklmnopqrstuvwxyz";

        for (int i = 0; i < letters.Length; i++)
        {
            var chord = new KeyChord(letters[i], ModifierSet.None);
            var result = _interpreter.Interpret(chord, _timestamp);

            result.Graphem.Should().Be(expectedChars[i].ToString(), $"Letter {letters[i]} should map to {expectedChars[i]}");
        }
    }

    [Fact]
    public void Interpret_AllLetters_WithShift_ReturnUppercase()
    {
        var letters = new[] { KeyId.A, KeyId.B, KeyId.C, KeyId.D, KeyId.E, KeyId.F, KeyId.G, KeyId.H, KeyId.I, KeyId.J, KeyId.K, KeyId.L, KeyId.M, KeyId.N, KeyId.O, KeyId.P, KeyId.Q, KeyId.R, KeyId.S, KeyId.T, KeyId.U, KeyId.V, KeyId.W, KeyId.X, KeyId.Y, KeyId.Z };
        var expectedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        for (int i = 0; i < letters.Length; i++)
        {
            var chord = new KeyChord(letters[i], ModifierSet.Shift);
            var result = _interpreter.Interpret(chord, _timestamp);

            result.Graphem.Should().Be(expectedChars[i].ToString(), $"Letter {letters[i]} with Shift should map to {expectedChars[i]}");
        }
    }
}
