using Scriptum.Wpf.Projections;

namespace Scriptum.Wpf.Projections.Services;

/// <summary>
/// Query-Service für Content-Daten (Module, Lektionen, Guides).
/// </summary>
public interface IContentQueryService
{
    /// <summary>
    /// Lädt alle Module mit Progress-Informationen.
    /// </summary>
    Task<IReadOnlyList<ModuleListItem>> GetModulesAsync(CancellationToken ct = default);

    /// <summary>
    /// Lädt alle Lektionen eines Moduls mit Progress-Informationen.
    /// </summary>
    Task<IReadOnlyList<LessonListItem>> GetLessonsByModuleAsync(string moduleId, CancellationToken ct = default);

    /// <summary>
    /// Lädt die Details einer Lektion.
    /// </summary>
    Task<LessonDetailsModel?> GetLessonDetailsAsync(string moduleId, string lessonId, CancellationToken ct = default);

    /// <summary>
    /// Lädt die Anleitung einer Lektion.
    /// </summary>
    Task<LessonGuideModel?> GetLessonGuideAsync(string lessonId, CancellationToken ct = default);
}
