namespace Scriptum.Application.Import.Dtos;

/// <summary>
/// Import-DTO für eine Lektion.
/// </summary>
public sealed class LessonImportDto
{
    public string? LessonId { get; set; }
    public string ModuleId { get; set; } = string.Empty;
    public string Titel { get; set; } = string.Empty;
    public string Beschreibung { get; set; } = string.Empty;
    public int Schwierigkeit { get; set; }
    public List<string> Tags { get; set; } = new();
    public string Uebungstext { get; set; } = string.Empty;
}
