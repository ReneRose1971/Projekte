namespace TypeTutor.Logic.Core;

/// <summary>
/// Repräsentiert eine Tipp-Lektion mit Metadaten, normalisierten Textblöcken und
/// einem engine-freundlichen Zieltext (genau ein Leerzeichen zwischen den Blöcken).
/// Die Normalisierung/Erzeugung findet in der ILessonFactory statt; der Konstruktor
/// validiert lediglich, dass ein gültiger, bereits normalisierter Zustand übergeben wird.
/// </summary>
public sealed class Lesson
{
    /// <summary>
    /// Erstellt eine Lesson aus Metadaten und (bereits normalisierten) Blöcken.
    /// Validiert:
    /// - <see cref="LessonMetadata"/> ist nicht null, Id/Title nicht leer
    /// - <paramref name="blocks"/> ist nicht null
    /// - Es gibt keine null-/Leer-Elemente in <paramref name="blocks"/>
    /// </summary>
    public Lesson(LessonMetaData meta, IReadOnlyList<string> blocks)
    {
        Meta = meta ?? throw new ArgumentNullException(nameof(meta));
        if (string.IsNullOrWhiteSpace(Meta.Title))
            throw new ArgumentException("Metadata.Title must not be empty.", nameof(meta));

        Blocks = blocks ?? throw new ArgumentNullException(nameof(blocks));

        for (int i = 0; i < Blocks.Count; i++)
        {
            var b = Blocks[i];
            if (b is null)
                throw new ArgumentException("Blocks must not contain null elements.", nameof(blocks));
            if (b.Length == 0)
                throw new ArgumentException("Blocks must not contain empty elements.", nameof(blocks));
            // Hinweis: weitere Normalisierungsregeln (Trim/Whitespace) übernimmt die Factory.
        }

        TargetText = string.Join(" ", Blocks);
        Metrics = LessonMetrics.FromBlocks(Blocks);
    }

    /// <summary> Metadaten der Lesson (Id, Title, …).</summary>
    public LessonMetaData Meta { get; }

    /// <summary> Normalisierte, nicht-leere Textblöcke der Lektion. </summary>
    public IReadOnlyList<string> Blocks { get; }

    /// <summary> Engine-freundlicher Zieltext: genau ein Space zwischen den Blöcken. </summary>
    public string TargetText { get; }

    /// <summary> Dauerhafte Kenngrößen (Block-/Zeichenanzahl, Leer-Flag). </summary>
    public LessonMetrics Metrics { get; }

    public override string ToString()
        => $"Lesson(Title='{Meta.Title}', Blocks={Metrics.BlockCount}, Characters={Metrics.CharacterCount})";
}
