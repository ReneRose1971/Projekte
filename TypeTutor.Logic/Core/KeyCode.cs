using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeTutor.Logic.Core;

public enum KeyCode : ushort
{
    None = 0,

    // Buchstaben
    A, B, C, D, E, F, G, H, I, J, K, L, M,
    N, O, P, Q, R, S, T, U, V, W, X, Y, Z,

    // Zahlenreihe
    D0, D1, D2, D3, D4, D5, D6, D7, D8, D9,

    // Sonder- und Steuerzeichen
    Space,
    Enter,
    Backspace,
    Tab,
    Escape,

    // Satzzeichen / OEM-Tasten
    OemComma,     // ,
    OemPeriod,    // .
    OemMinus,     // -
    OemPlus,      // +
    Oem1,         // lokale Taste (z. B. Ü)
    Oem2,         // lokale Taste (z. B. -/_ je nach Layout)
    Oem3,         // lokale Taste (z. B. Ö)
    Oem4,         // ]
    Oem5,         // \
    Oem6,         // [
    Oem7,         // '
    Oem102,       // < > |

    // Pfeiltasten
    Left,
    Right,
    Up,
    Down,

    // Numpad
    NumPad0,
    NumPad1,
    NumPad2,
    NumPad3,
    NumPad4,
    NumPad5,
    NumPad6,
    NumPad7,
    NumPad8,
    NumPad9,
    NumPadDecimal, // decimal/comma key on numpad

    // Numpad-Arithmetic
    Add,
    Subtract,
    Multiply,
    Divide
}

