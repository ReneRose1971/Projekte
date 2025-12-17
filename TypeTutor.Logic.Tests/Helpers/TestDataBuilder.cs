using TypeTutor.Logic.Core;
using TypeTutor.Logic.Data;

namespace TypeTutor.Logic.Tests.Helpers;

/// <summary>
/// Builder-Klasse zur Erstellung von Test-Daten für TypeTutor.Logic-Tests.
/// Bietet fluent API für einfache und konsistente Test-Daten-Generierung.
/// </summary>
public static class TestDataBuilder
{
    /// <summary>
    /// Erstellt eine Standard-LessonData für Tests.
    /// </summary>
    public static LessonData CreateLessonData(
        string? lessonId = null,
        string? title = null,
        string? content = null,
        string? description = null,
        int difficulty = 1,
        string? moduleId = null)
    {
        return new LessonData(
            lessonId: lessonId ?? "L0001",
            title: title ?? "Test Lesson",
            content: content ?? "test content"
        )
        {
            Description = description ?? "Test description",
            Difficulty = difficulty,
            ModuleId = moduleId ?? string.Empty
        };
    }

    /// <summary>
    /// Erstellt eine LessonData mit mehreren Blöcken.
    /// </summary>
    public static LessonData CreateMultiBlockLessonData(
        string lessonId,
        string title,
        params string[] blocks)
    {
        return new LessonData(
            lessonId: lessonId,
            title: title,
            content: string.Join(Environment.NewLine, blocks)
        )
        {
            Description = $"Lesson with {blocks.Length} blocks",
            Difficulty = 1
        };
    }

    /// <summary>
    /// Erstellt eine Standard-LessonGuideData für Tests.
    /// </summary>
    public static LessonGuideData CreateLessonGuideData(
        string? lessonId = null,
        string? bodyMarkdown = null)
    {
        return new LessonGuideData(
            lessonId: lessonId ?? "L0001",
            bodyMarkdown: bodyMarkdown ?? "Test guide markdown content"
        );
    }

    /// <summary>
    /// Erstellt ein Standard-LessonMetaData für Tests.
    /// </summary>
    public static LessonMetaData CreateLessonMetaData(
        string? title = null,
        string? description = null,
        int difficulty = 1)
    {
        return new LessonMetaData(title ?? "Test Lesson")
        {
            Description = description ?? "Test description",
            Difficulty = difficulty
        };
    }

    /// <summary>
    /// Erstellt einen KeyStroke mit einem druckbaren Zeichen.
    /// </summary>
    public static KeyStroke CreateKeyStroke(
        char character,
        ModifierKeys modifiers = ModifierKeys.None)
    {
        return new KeyStroke(
            key: KeyCode.None,
            ch: character,
            mods: modifiers
        );
    }

    /// <summary>
    /// Erstellt eine Sequenz von KeyStrokes aus einem String.
    /// </summary>
    public static IEnumerable<KeyStroke> CreateKeyStrokes(string text)
    {
        foreach (char c in text)
        {
            yield return CreateKeyStroke(c);
        }
    }

    /// <summary>
    /// Erstellt ein einfaches Lesson-Objekt für Tests.
    /// </summary>
    public static Lesson CreateLesson(
        string? title = null,
        params string[] blocks)
    {
        var meta = CreateLessonMetaData(title: title);
        var blockList = blocks.Length > 0 ? blocks : new[] { "test", "block" };
        return new Lesson(meta, blockList);
    }

    /// <summary>
    /// Erstellt eine Lesson mit einem einzigen Block.
    /// </summary>
    public static Lesson CreateSimpleLesson(string text, string? title = null)
    {
        var meta = CreateLessonMetaData(title: title ?? "Simple Lesson");
        return new Lesson(meta, new[] { text });
    }
}
