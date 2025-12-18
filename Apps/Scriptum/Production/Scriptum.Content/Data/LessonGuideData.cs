namespace Scriptum.Content.Data;

/// <summary>
/// Repräsentiert eine Lektions-Anleitung als DTO.
/// </summary>
/// <param name="LessonId">Die eindeutige ID der zugehörigen Lektion (Pflicht, nicht leer).</param>
/// <param name="GuideTextMarkdown">Der Anleitungstext im Markdown-Format (optional).</param>
/// <remarks>
/// <para>
/// Die 1:1 Beziehung zu <see cref="LessonData"/> wird über <see cref="LessonId"/> hergestellt.
/// Jede Lektion kann optional eine Anleitung haben.
/// </para>
/// </remarks>
/// <exception cref="ArgumentException">Wird ausgelöst, wenn LessonId leer ist.</exception>
public sealed record LessonGuideData
{
    public string LessonId { get; init; }
    public string GuideTextMarkdown { get; init; }

    /// <summary>
    /// Erstellt eine neue Instanz von <see cref="LessonGuideData"/>.
    /// </summary>
    /// <param name="lessonId">Die eindeutige ID der zugehörigen Lektion.</param>
    /// <param name="guideTextMarkdown">Der Anleitungstext im Markdown-Format.</param>
    /// <exception cref="ArgumentException">Wird ausgelöst, wenn lessonId leer ist.</exception>
    public LessonGuideData(
        string lessonId,
        string guideTextMarkdown = "")
    {
        if (string.IsNullOrWhiteSpace(lessonId))
            throw new ArgumentException("LessonId darf nicht leer sein.", nameof(lessonId));

        LessonId = lessonId;
        GuideTextMarkdown = guideTextMarkdown ?? string.Empty;
    }
}
