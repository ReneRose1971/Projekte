using Scriptum.Core;

namespace Scriptum.Progress;

/// <summary>
/// Repräsentiert eine gespeicherte Bewertung einer Eingabe.
/// </summary>
public sealed record StoredEvaluation
{
    private readonly int _tokenIndex;
    private readonly string _erwartet = null!;
    private readonly string _tatsaechlich = null!;
    
    /// <summary>
    /// Der Index des bewerteten Tokens (muss >= 0 sein).
    /// </summary>
    public int TokenIndex
    {
        get => _tokenIndex;
        init
        {
            if (value < 0)
                throw new ArgumentException("TokenIndex muss >= 0 sein.", nameof(value));
            _tokenIndex = value;
        }
    }
    
    /// <summary>
    /// Der erwartete Wert (darf nicht null oder leer sein).
    /// </summary>
    public string Erwartet
    {
        get => _erwartet;
        init
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Erwartet darf nicht null oder leer sein.", nameof(value));
            _erwartet = value;
        }
    }
    
    /// <summary>
    /// Der tatsächlich eingegebene Wert (darf nicht null sein; darf leer sein).
    /// </summary>
    public string Tatsaechlich
    {
        get => _tatsaechlich;
        init
        {
            if (value is null)
                throw new ArgumentException("Tatsaechlich darf nicht null sein.", nameof(value));
            _tatsaechlich = value;
        }
    }
    
    /// <summary>
    /// Das Bewertungsergebnis.
    /// </summary>
    public EvaluationOutcome Ergebnis { get; init; }
}
