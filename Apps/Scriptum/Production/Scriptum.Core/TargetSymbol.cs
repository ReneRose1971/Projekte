namespace Scriptum.Core;

/// <summary>
/// Ein Zielzeichen ist die kleinste didaktische Einheit einer Übung.
/// </summary>
/// <remarks>
/// Ein Zielzeichen definiert eine Position und das erwartete Graphem.
/// </remarks>
public readonly record struct TargetSymbol
{
    /// <summary>
    /// Die Position des Zielzeichens in der Sequenz (0-basiert).
    /// </summary>
    public int Index { get; }
    
    /// <summary>
    /// Das erwartete Graphem (z.B. "a", "A", "ä", "\n").
    /// </summary>
    public string Graphem { get; }
    
    /// <summary>
    /// Erstellt ein neues Zielzeichen.
    /// </summary>
    /// <param name="index">Die Position (muss >= 0 sein).</param>
    /// <param name="graphem">Das erwartete Graphem (darf nicht null oder leer sein).</param>
    /// <exception cref="ArgumentOutOfRangeException">Index ist negativ.</exception>
    /// <exception cref="ArgumentException">Graphem ist null oder leer.</exception>
    public TargetSymbol(int index, string graphem)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), "Index muss >= 0 sein.");
        
        if (string.IsNullOrEmpty(graphem))
            throw new ArgumentException("Graphem darf nicht null oder leer sein.", nameof(graphem));
        
        Index = index;
        Graphem = graphem;
    }
}
