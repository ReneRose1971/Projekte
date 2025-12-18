using System.Text.Json.Serialization;

namespace Scriptum.Application.Import.Dtos;

/// <summary>
/// Import-DTO für eine Lektion.
/// </summary>
public sealed class LessonImportDto
{
    [JsonPropertyName("LessonId")]
    public string? LessonId { get; set; }
    
    [JsonPropertyName("ModuleId")]
    public string ModuleId { get; set; } = string.Empty;
    
    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("Description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("Difficulty")]
    public int Difficulty { get; set; }
    
    [JsonPropertyName("Tags")]
    public List<string> Tags { get; set; } = new();
    
    [JsonPropertyName("Content")]
    public string Content { get; set; } = string.Empty;
    
    // Für Abwärtskompatibilität mit deutschen Property-Namen
    [JsonIgnore]
    public string Titel
    {
        get => Title;
        set => Title = value;
    }
    
    [JsonIgnore]
    public string Beschreibung
    {
        get => Description;
        set => Description = value;
    }
    
    [JsonIgnore]
    public int Schwierigkeit
    {
        get => Difficulty;
        set => Difficulty = value;
    }
    
    [JsonIgnore]
    public string Uebungstext
    {
        get => Content;
        set => Content = value;
    }
}
