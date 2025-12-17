using Scriptum.Core;

namespace Scriptum.Progress;

/// <summary>
/// Repräsentiert eine gespeicherte Benutzereingabe während einer Trainingssession.
/// </summary>
public sealed record StoredInput
{
    private readonly DateTimeOffset _zeitpunkt;
    private readonly string _erzeugtesGraphem = null!;
    
    /// <summary>
    /// Der Zeitpunkt der Eingabe.
    /// </summary>
    public DateTimeOffset Zeitpunkt
    {
        get => _zeitpunkt;
        init
        {
            if (value == default)
                throw new ArgumentException("Zeitpunkt darf nicht default sein.", nameof(value));
            _zeitpunkt = value;
        }
    }
    
    /// <summary>
    /// Die gedrückte Taste.
    /// </summary>
    public KeyId Taste { get; init; }
    
    /// <summary>
    /// Die aktiven Umschalttasten.
    /// </summary>
    public ModifierSet Umschalter { get; init; }
    
    /// <summary>
    /// Die Art der Eingabe.
    /// </summary>
    public StoredInputKind Art { get; init; }
    
    /// <summary>
    /// Das erzeugte Graphem (darf nie null sein; bei Rücktaste/Ignoriert darf es leer sein).
    /// </summary>
    public string ErzeugtesGraphem
    {
        get => _erzeugtesGraphem;
        init
        {
            if (value is null)
                throw new ArgumentException("ErzeugtesGraphem darf nicht null sein.", nameof(value));
            _erzeugtesGraphem = value;
        }
    }
}
