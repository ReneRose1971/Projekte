namespace TypeTutor.Logic.Core
{
    /// <summary>
    /// Beschreibt ein Modul mit Titel und ausführlicher Anweisung im Markdown-Format.
    /// Title dient als eindeutiger Identifier.
    /// </summary>
    public sealed record ModuleGuide
    {
        public string Title { get; init; }
        public string BodyMarkDown { get; init; } = string.Empty;

        public ModuleGuide(string title)
        {
            Title = title ?? string.Empty;
        }

        // Parameterless ctor for deserializers
        public ModuleGuide() { Title = string.Empty; }
    }
}
