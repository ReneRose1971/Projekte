namespace Scriptum.Wpf.Projections.Models;

/// <summary>
/// UI-Projektion für einen Session-Fehler.
/// </summary>
public sealed record SessionErrorRow(
    int Index,
    DateTime Timestamp,
    string Expected,
    string Actual,
    string? Reason);
