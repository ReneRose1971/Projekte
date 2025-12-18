namespace Scriptum.Wpf.Projections.Models;

/// <summary>
/// Statistik-Zeile für eine Lektion.
/// </summary>
public sealed record LessonStatRow(
    string LessonId,
    string ModuleId,
    string Title,
    int SessionsCount,
    int CompletedCount,
    double AvgErrors,
    double AvgAccuracy,
    TimeSpan? BestDuration,
    DateTime? LastTrainedAt);
