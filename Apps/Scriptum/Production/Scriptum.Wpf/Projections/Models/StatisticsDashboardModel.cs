namespace Scriptum.Wpf.Projections.Models;

/// <summary>
/// Statistik-Dashboard-Modell.
/// </summary>
public sealed record StatisticsDashboardModel(
    IReadOnlyList<ModuleStatRow> Modules,
    IReadOnlyList<LessonStatRow> Lessons);
