using System.Text.Json.Serialization;

namespace Scriptum.Application.Import.Dtos;

/// <summary>
/// Import-DTO für ein Modul.
/// </summary>
public sealed class ModuleImportDto
{
    [JsonPropertyName("ModuleId")]
    public string ModuleId { get; set; } = string.Empty;
    
    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("IntroMarkDown")]
    public string IntroMarkDown { get; set; } = string.Empty;
    
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
        get => IntroMarkDown;
        set => IntroMarkDown = value;
    }
}
