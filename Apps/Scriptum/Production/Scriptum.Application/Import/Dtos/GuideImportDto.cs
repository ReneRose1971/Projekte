using System.Text.Json.Serialization;

namespace Scriptum.Application.Import.Dtos;

/// <summary>
/// Import-DTO für eine Lektionsanleitung.
/// </summary>
public sealed class GuideImportDto
{
    [JsonPropertyName("LessonId")]
    public string? LessonId { get; set; }
    
    [JsonPropertyName("Title")]
    public string? Title { get; set; }
    
    [JsonPropertyName("BodyMarkDown")]
    public string BodyMarkDown { get; set; } = string.Empty;
    
    // Für Abwärtskompatibilität
    [JsonIgnore]
    public string? LessonTitel
    {
        get => Title;
        set => Title = value;
    }
    
    [JsonIgnore]
    public string GuideTextMarkdown
    {
        get => BodyMarkDown;
        set => BodyMarkDown = value;
    }
}
