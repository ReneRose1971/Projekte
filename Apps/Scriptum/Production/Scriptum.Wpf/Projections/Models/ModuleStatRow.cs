namespace Scriptum.Wpf.Projections.Models;

/// <summary>
/// Statistik-Zeile für ein Modul.
/// </summary>
public sealed record ModuleStatRow(
    string ModuleId,
    string Title,
    int SessionsCount,
    int CompletedCount,
    double AvgErrors,
    double AvgAccuracy,
    TimeSpan? BestDuration,
    DateTime? LastTrainedAt);
