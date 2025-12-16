namespace TypeTutor.Logic.Core
{
    /// <summary>
    /// Steuert, ob die TypingEngine Groß-/Kleinschreibung streng prüft
    /// oder beim Vergleich ignoriert.
    /// </summary>
    public enum CaseSensitivity
    {
        /// <summary>Zeichenvergleich ist exakt (Standard).</summary>
        Strict = 0,

        /// <summary>Groß-/Kleinschreibung wird ignoriert (kulturell invariant).</summary>
        IgnoreCase = 1
    }
}
