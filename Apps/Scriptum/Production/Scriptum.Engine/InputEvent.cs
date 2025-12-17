using Scriptum.Core;

namespace Scriptum.Engine;

/// <summary>
/// Beschreibt eine Handlung des Nutzers.
/// </summary>
/// <remarks>
/// Ein InputEvent weiß nicht, ob die Eingabe richtig oder falsch war.
/// </remarks>
public sealed class InputEvent
{
    /// <summary>
    /// Zeitpunkt der Eingabe.
    /// </summary>
    public DateTime Timestamp { get; }
    
    /// <summary>
    /// Die gedrückte Tastenkombination.
    /// </summary>
    public KeyChord Chord { get; }
    
    /// <summary>
    /// Art der Eingabe.
    /// </summary>
    public InputEventKind Kind { get; }
    
    /// <summary>
    /// Das erzeugte Graphem (nur bei Kind == Zeichen).
    /// </summary>
    public string? Graphem { get; }
    
    /// <summary>
    /// Erstellt ein neues Eingabeereignis.
    /// </summary>
    /// <param name="timestamp">Zeitpunkt der Eingabe.</param>
    /// <param name="chord">Die Tastenkombination.</param>
    /// <param name="kind">Art der Eingabe.</param>
    /// <param name="graphem">Das erzeugte Graphem (optional, nur bei Zeichen).</param>
    /// <exception cref="ArgumentException">Kind ist Zeichen, aber Graphem ist null oder leer.</exception>
    public InputEvent(DateTime timestamp, KeyChord chord, InputEventKind kind, string? graphem = null)
    {
        if (kind == InputEventKind.Zeichen && string.IsNullOrEmpty(graphem))
            throw new ArgumentException("Bei Kind == Zeichen muss Graphem gesetzt sein.", nameof(graphem));
        
        Timestamp = timestamp;
        Chord = chord;
        Kind = kind;
        Graphem = graphem;
    }
}
