namespace Scriptum.Core;

/// <summary>
/// Eine unveränderliche, geordnete Liste von Zielzeichen.
/// </summary>
/// <remarks>
/// Die Zielsequenz definiert den didaktischen Inhalt einer Übung.
/// </remarks>
public sealed class TargetSequence
{
    private readonly IReadOnlyList<TargetSymbol> _symbols;
    
    /// <summary>
    /// Die Zielzeichen dieser Sequenz.
    /// </summary>
    public IReadOnlyList<TargetSymbol> Symbols => _symbols;
    
    /// <summary>
    /// Die Anzahl der Zielzeichen.
    /// </summary>
    public int Length => _symbols.Count;
    
    /// <summary>
    /// Erstellt eine neue Zielsequenz.
    /// </summary>
    /// <param name="symbols">Die Zielzeichen (darf nicht null sein).</param>
    /// <exception cref="ArgumentNullException">symbols ist null.</exception>
    /// <exception cref="ArgumentException">Die Indizes sind nicht konsistent oder die Liste ist leer.</exception>
    public TargetSequence(IReadOnlyList<TargetSymbol> symbols)
    {
        if (symbols is null)
            throw new ArgumentNullException(nameof(symbols));
        
        if (symbols.Count == 0)
            throw new ArgumentException("Die Zielsequenz darf nicht leer sein.", nameof(symbols));
        
        ValidateIndices(symbols);
        
        _symbols = symbols;
    }
    
    /// <summary>
    /// Erstellt eine neue Zielsequenz aus Graphemen.
    /// </summary>
    /// <param name="graphemes">Die Grapheme (darf nicht null oder leer sein).</param>
    /// <exception cref="ArgumentNullException">graphemes ist null.</exception>
    /// <exception cref="ArgumentException">Die Liste ist leer oder enthält ungültige Grapheme.</exception>
    public TargetSequence(IEnumerable<string> graphemes)
    {
        if (graphemes is null)
            throw new ArgumentNullException(nameof(graphemes));
        
        var symbols = graphemes
            .Select((g, i) => new TargetSymbol(i, g))
            .ToList();
        
        if (symbols.Count == 0)
            throw new ArgumentException("Die Zielsequenz darf nicht leer sein.", nameof(graphemes));
        
        _symbols = symbols;
    }
    
    private static void ValidateIndices(IReadOnlyList<TargetSymbol> symbols)
    {
        for (int i = 0; i < symbols.Count; i++)
        {
            if (symbols[i].Index != i)
            {
                throw new ArgumentException(
                    $"Inkonsistenter Index an Position {i}: erwartet {i}, gefunden {symbols[i].Index}.",
                    nameof(symbols));
            }
        }
    }
}
