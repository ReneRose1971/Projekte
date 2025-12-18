namespace Scriptum.Application.Import.Dtos;

/// <summary>
/// Import-DTO für ein Modul.
/// </summary>
public sealed class ModuleImportDto
{
    public string ModuleId { get; set; } = string.Empty;
    public string Titel { get; set; } = string.Empty;
    public string Beschreibung { get; set; } = string.Empty;
}
