namespace Scriptum.Wpf.Projections.Models;

/// <summary>
/// UI-Projektion für ein Session-Event (Input oder Evaluation).
/// </summary>
public sealed record SessionEventRow(
    int Index,
    DateTime Timestamp,
    string Kind,
    string Expected,
    string Actual,
    bool IsError);
