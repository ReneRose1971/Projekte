using Scriptum.Core;
using Scriptum.Engine;
using Scriptum.Progress;

namespace Scriptum.Application.Factories;

/// <summary>
/// Factory zur Erstellung von <see cref="StoredInput"/> aus <see cref="InputEvent"/>.
/// </summary>
public static class StoredInputFactory
{
    /// <summary>
    /// Erstellt ein StoredInput aus einem InputEvent und KeyChord.
    /// </summary>
    /// <param name="inputEvent">Das InputEvent von der Engine.</param>
    /// <param name="chord">Der KeyChord (Taste + Modifier).</param>
    /// <param name="timestamp">Der Zeitpunkt der Eingabe.</param>
    /// <returns>Ein neues StoredInput.</returns>
    /// <exception cref="ArgumentNullException">Wenn inputEvent null ist.</exception>
    public static StoredInput FromInputEvent(InputEvent inputEvent, KeyChord chord, DateTimeOffset timestamp)
    {
        if (inputEvent == null)
            throw new ArgumentNullException(nameof(inputEvent));
        
        return new StoredInput
        {
            Zeitpunkt = timestamp,
            Taste = chord.Key,
            Umschalter = chord.Modifiers,
            Art = MapInputEventKind(inputEvent.Kind),
            ErzeugtesGraphem = inputEvent.Graphem ?? string.Empty
        };
    }
    
    private static StoredInputKind MapInputEventKind(InputEventKind kind)
    {
        return kind switch
        {
            InputEventKind.Zeichen => StoredInputKind.Zeichen,
            InputEventKind.Ruecktaste => StoredInputKind.Ruecktaste,
            InputEventKind.Ignoriert => StoredInputKind.Ignoriert,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unbekannte InputEventKind.")
        };
    }
}
