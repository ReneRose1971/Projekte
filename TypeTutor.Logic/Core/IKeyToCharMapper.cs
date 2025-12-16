namespace TypeTutor.Logic.Core;

/// <summary>
/// Übersetzt einen physikalischen Tastendruck (<see cref="KeyStroke"/>) in ein druckbares Zeichen.
/// Gibt <c>null</c> zurück, wenn die Taste kein direktes Zeichen produziert (z. B. Enter, Pfeiltasten, Dead Keys).
/// </summary>
public interface IKeyToCharMapper
{
    /// <summary>
    /// Mappt einen KeyStroke auf ein druckbares Zeichen gemäß Tastaturlayout.
    /// </summary>
    /// <param name="stroke">Der Tastendruck (physische Taste + Modifikatoren + optionales bereits ermitteltes Zeichen).</param>
    /// <returns>Ein druckbares Zeichen oder <c>null</c>, wenn kein Zeichen entsteht.</returns>
    char? Map(KeyStroke stroke);
}
