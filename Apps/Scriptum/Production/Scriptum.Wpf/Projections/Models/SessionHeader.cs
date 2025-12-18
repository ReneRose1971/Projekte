namespace Scriptum.Wpf.Projections.Models;

/// <summary>
/// Header-Informationen einer Session.
/// </summary>
public sealed record SessionHeader(
    DateTime StartedAt,
    DateTime? CompletedAt,
    string ModuleTitle,
    string LessonTitle,
    string ModuleId,
    string LessonId);
