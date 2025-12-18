namespace Scriptum.Wpf.Projections.Models;

/// <summary>
/// Filter für Session-Abfragen.
/// </summary>
public sealed record SessionFilter(
    string? ModuleId,
    string? LessonId,
    DateTime? From,
    DateTime? To,
    bool? OnlyCompleted);
