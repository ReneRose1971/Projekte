namespace Scriptum.Wpf.Projections;

/// <summary>
/// UI-Projektion für eine Session in der Liste.
/// </summary>
public sealed record SessionListItem(
    int SessionId,
    DateTime StartedAt,
    DateTime? CompletedAt,
    string ModuleId,
    string LessonId,
    string ModuleTitle,
    string LessonTitle,
    bool IsCompleted,
    int TotalInputs,
    int TotalErrors,
    TimeSpan? Duration);
