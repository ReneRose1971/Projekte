namespace Scriptum.Core;

/// <summary>
/// Fachliche Kennung einer Taste auf einer DE-QWERTZ-Tastatur.
/// </summary>
public enum KeyId
{
    None = 0,
    
    // Buchstaben
    A, B, C, D, E, F, G, H, I, J, K, L, M,
    N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
    
    // Ziffern
    Digit0, Digit1, Digit2, Digit3, Digit4,
    Digit5, Digit6, Digit7, Digit8, Digit9,
    
    // Sonderzeichen (OEM-Tasten)
    Oem1,      // Ü
    Oem2,      // #
    Oem3,      // Ö
    Oem4,      // ß
    Oem5,      // ^
    Oem6,      // ´
    Oem7,      // Ä
    Oem102,    // < > |
    OemComma,  // ,
    OemMinus,  // -
    OemPeriod, // .
    OemPlus,   // +
    
    // Steuerungstasten
    Space,
    Enter,
    Tab,
    Backspace,
    Escape,
    
    // Umschalttasten (für vollständige Abdeckung)
    LeftShift,
    RightShift,
    LeftAlt,
    RightAlt,
    LeftCtrl,
    RightCtrl
}
