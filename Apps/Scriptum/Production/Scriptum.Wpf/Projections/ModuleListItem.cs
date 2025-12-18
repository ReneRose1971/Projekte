using Scriptum.Wpf.Projections.Models;

namespace Scriptum.Wpf.Projections;

/// <summary>
/// UI-Projektion für ein Modul in der Liste.
/// </summary>
public sealed record ModuleListItem(
    string ModuleId,
    string Title,
    string? Description,
    int LessonCount,
    ModuleProgressSummary? Progress);
