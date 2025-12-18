namespace Scriptum.Wpf.Projections.Models;

/// <summary>
/// Filter für Statistik-Abfragen.
/// </summary>
public sealed record StatisticsFilter(
    string? ModuleId,
    string? LessonId,
    DateTime? From,
    DateTime? To);
