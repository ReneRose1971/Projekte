namespace Scriptum.Application.Import.Dtos;

/// <summary>
/// Import-DTO für eine Lektionsanleitung.
/// </summary>
public sealed class GuideImportDto
{
    public string? LessonId { get; set; }
    public string? LessonTitel { get; set; }
    public string GuideTextMarkdown { get; set; } = string.Empty;
}
