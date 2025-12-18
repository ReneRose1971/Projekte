namespace Scriptum.Wpf.Projections.Models;

/// <summary>
/// Progress-Zusammenfassung für ein Modul.
/// </summary>
public sealed record ModuleProgressSummary(
    int TotalSessions,
    int CompletedSessions,
    DateTime? LastTrainedAt);
