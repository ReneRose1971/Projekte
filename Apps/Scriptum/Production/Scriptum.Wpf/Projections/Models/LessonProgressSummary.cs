namespace Scriptum.Wpf.Projections.Models;

/// <summary>
/// Progress-Zusammenfassung für eine Lektion.
/// </summary>
public sealed record LessonProgressSummary(
    int TotalSessions,
    int CompletedSessions,
    double BestAccuracy,
    TimeSpan? BestDuration,
    DateTime? LastTrainedAt);
