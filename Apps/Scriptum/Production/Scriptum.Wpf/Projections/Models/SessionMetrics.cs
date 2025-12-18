namespace Scriptum.Wpf.Projections.Models;

/// <summary>
/// Session-Metriken für die Detailansicht.
/// </summary>
public sealed record SessionMetrics(
    int TotalInputs,
    int TotalErrors,
    double Accuracy,
    TimeSpan? Duration,
    double? InputsPerMinute);
